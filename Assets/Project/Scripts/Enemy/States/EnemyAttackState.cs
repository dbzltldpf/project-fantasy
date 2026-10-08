using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 근접 공격: 공격마다 1 ~ 최대 콤보 수 중 무작위 횟수만큼 이어 치기 (콤보 창에서 대상이 가까우면 예약), 반격은 단일 액션
    public sealed class EnemyAttackState : EnemyStateBase, IActionContext
    {
        private const int FirstComboIndex = 0;
        private const float ChainRangeMultiplier = 1.5f;
        private const int InclusiveMaxOffset = 1;
        private const int MinComboActions = 1;

        private readonly ActionPlayer actionPlayer;
        private ActionData counterAction;
        private Vector3 moveDirection;
        private float moveSpeed;
        private int comboIndex;
        private int comboTransitionFrame;
        private bool isComboWindowOpen;
        private bool hasQueuedCombo;
        private bool isCounter;
        private int comboLimit;

        private AttackComboData ComboData => Data.ComboData;
        private MeleeAttacker Attacker => Controller.Attacker;

        public EnemyAttackState(EnemyController controller) : base(controller)
        {
            actionPlayer = new ActionPlayer(this);
        }

        // 다음 진입을 반격 액션 1회로 지정
        public void SetCounter(ActionData action) => counterAction = action;

        public override void Enter()
        {
            isCounter = counterAction != null;
            if (isCounter)
            {
                StartAction(counterAction, FirstComboIndex);
                counterAction = null;
                return;
            }

            // 이번 공격에서 칠 횟수: 1 ~ 최대 콤보 수 (무기 콤보 길이 이하)
            int maxActions = Mathf.Min(Data.MaxComboActions, ComboData.ActionCount);
            comboLimit = Random.Range(MinComboActions, maxActions + InclusiveMaxOffset);
            StartAction(ComboData.GetAction(FirstComboIndex), FirstComboIndex);
        }

        public override void Tick(float deltaTime)
        {
            actionPlayer.Tick(deltaTime);
            if (!actionPlayer.IsPlaying) return;

            Navigator.MoveImmediate(moveDirection * (moveSpeed * actionPlayer.Current.PlaybackSpeed), deltaTime);

            if (TryChainCombo()) return;

            if (actionPlayer.IsFinished)
            {
                Controller.StartAttackCooldown();
                Controller.ReturnToCombat();
            }
        }

        public override void Exit()
        {
            actionPlayer.Stop();
            Attacker.EndSwing();
            counterAction = null;
        }

        // 콤보 창에서 이어 칠지 예약(대상이 가까울 때), 전이 프레임에 다음 액션 (창이 먼저 닫혀도 예약은 유지, 플레이어와 같은 방식)
        private bool TryChainCombo()
        {
            // 반격은 단일 액션
            int nextIndex = comboIndex + 1;
            if (isCounter || nextIndex >= comboLimit || !ComboData.HasNextAction(comboIndex)) return false;

            if (isComboWindowOpen && !hasQueuedCombo) hasQueuedCombo = IsTargetNear();
            if (!hasQueuedCombo || actionPlayer.CurrentFrame < comboTransitionFrame) return false;

            StartAction(ComboData.GetAction(nextIndex), nextIndex);
            return true;
        }

        private bool IsTargetNear()
        {
            float chainRange = Data.AttackRange * ChainRangeMultiplier;
            return Perception.HasTarget && Perception.GetDirectionToTarget().sqrMagnitude <= chainRange * chainRange;
        }

        private void StartAction(ActionData action, int index)
        {
            comboIndex = index;
            isComboWindowOpen = false;
            hasQueuedCombo = false;
            moveSpeed = 0f;

            Navigator.Stop();
            if (Perception.HasTarget) Navigator.SnapRotation(Perception.GetDirectionToTarget());
            moveDirection = Controller.transform.forward;

            EnemyAnimator.PlayAction(action.StateHash, action.CrossFadeDuration, action.PlaybackSpeed);
            actionPlayer.Play(action);
        }

        void IActionContext.BeginHit(float damageMultiplier, float hitStopDuration)
        {
            int damage = Mathf.RoundToInt(Data.AttackPower * damageMultiplier);
            Attacker.BeginSwing(damage, hitStopDuration);
        }

        void IActionContext.TickHit() => Attacker.TickHit();
        void IActionContext.EndHit() => Attacker.EndSwing();

        void IActionContext.OpenComboWindow(int transitionFrame)
        {
            isComboWindowOpen = true;
            comboTransitionFrame = transitionFrame;
        }

        void IActionContext.CloseComboWindow() => isComboWindowOpen = false;
        void IActionContext.SetMoveSpeed(float speed) => moveSpeed = speed;
    }
}

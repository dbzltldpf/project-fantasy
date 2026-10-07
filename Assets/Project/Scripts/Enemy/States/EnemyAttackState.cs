using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 근접 공격: 무기 콤보를 최대 횟수까지 이어 치기 (대상이 거리 안일 때만), 반격은 단일 액션
    public sealed class EnemyAttackState : EnemyStateBase, IActionContext
    {
        private const int FirstComboIndex = 0;
        private const float ChainRangeMultiplier = 1.5f;

        private readonly ActionPlayer actionPlayer;
        private ActionData counterAction;
        private Vector3 moveDirection;
        private float moveSpeed;
        private int comboIndex;
        private int comboTransitionFrame;
        private bool isComboWindowOpen;

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
            if (counterAction != null)
            {
                StartAction(counterAction, FirstComboIndex);
                counterAction = null;
                return;
            }

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

        // 전이 프레임에 도달하고 대상이 아직 가까우면 다음 액션
        private bool TryChainCombo()
        {
            int nextIndex = comboIndex + 1;
            if (!isComboWindowOpen || nextIndex >= Data.MaxComboActions || !ComboData.HasNextAction(comboIndex)) return false;
            if (actionPlayer.CurrentFrame < comboTransitionFrame || !IsTargetNear()) return false;

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

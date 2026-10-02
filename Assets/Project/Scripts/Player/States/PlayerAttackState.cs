using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 콤보 공격: ActionPlayer로 액션 타임라인 재생, 이벤트 요청(판정·콤보 창·전진)을 처리
    public sealed class PlayerAttackState : PlayerStateBase, IActionContext
    {
        private const int FirstComboIndex = 0;

        private readonly ActionPlayer actionPlayer;
        private Vector3 moveDirection;
        private float moveSpeed;
        private int comboIndex;
        private int comboTransitionFrame;
        private bool isComboWindowOpen;
        private bool hasQueuedAttack;

        private AttackComboData ComboData => Controller.AttackComboData;

        public PlayerAttackState(PlayerController controller) : base(controller)
        {
            actionPlayer = new ActionPlayer(this);
        }

        public override void Enter()
        {
            StartAction(FirstComboIndex);
        }

        public override void Tick(float deltaTime)
        {
            actionPlayer.Tick(deltaTime);
            if (!actionPlayer.IsPlaying) return;

            Motor.SetVelocityImmediate(moveDirection * (moveSpeed * actionPlayer.Current.PlaybackSpeed));

            if (TryChainCombo()) return;

            if (actionPlayer.IsFinished)
            {
                Controller.ChangeState(Controller.LocomotionState);
            }
        }

        public override void Exit()
        {
            actionPlayer.Stop();
            Attacker.EndSwing();
        }

        // 콤보 창에서는 예약만, 전이 프레임에 다음 액션 시작
        private bool TryChainCombo()
        {
            if (!ComboData.HasNextAction(comboIndex)) return false;

            if (isComboWindowOpen && !hasQueuedAttack)
            {
                hasQueuedAttack = InputHandler.ConsumeAttack();
            }

            if (!hasQueuedAttack || actionPlayer.CurrentFrame < comboTransitionFrame) return false;

            StartAction(comboIndex + 1);
            return true;
        }

        private void StartAction(int index)
        {
            comboIndex = index;
            hasQueuedAttack = false;
            ActionData action = ComboData.GetAction(index);

            // 입력 방향으로 즉시 회전 후 공격
            Motor.SnapRotation(Controller.GetCameraRelativeMove());
            moveDirection = Controller.transform.forward;

            PlayerAnimator.PlayAction(action.StateHash, action.CrossFadeDuration, action.PlaybackSpeed);
            actionPlayer.Play(action);
        }

        void IActionContext.BeginHit(float damageMultiplier, float hitStopDuration)
        {
            int damage = Mathf.RoundToInt(Loadout.CurrentWeapon.AttackPower * damageMultiplier);
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

using ProjectFantasy.Audio;
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
        private bool CanChainCombo => isComboWindowOpen && ComboData.HasNextAction(comboIndex);

        private AttackComboData ComboData => Controller.AttackComboData;
        private bool isCancelWindowOpen;
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
            if (TryCancel()) return;
            if (TryChainCombo()) return;

            if (actionPlayer.IsFinished)
            {
                Controller.ChangeState(Controller.LocomotionState);
            }
        }
        private bool TryCancel()
        {
            if (!isCancelWindowOpen) return false;
            if (TryStartDodge())
            {
                Controller.ChangeState(Controller.DodgeState);
                return true;
            }
            if (hasQueuedAttack || CanChainCombo) return false;
            if (HasMoveInput)
            {
                Controller.ChangeState(Controller.LocomotionState);
                return true;
            }
            return false;
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
            isCancelWindowOpen = false;
            comboIndex = index;
            hasQueuedAttack = false;
            ActionData action = ComboData.GetAction(index);

            // 조준점 방향(카메라 기준)으로 즉시 회전 후 공격
            Motor.SnapRotation(Controller.GetAimFacingDirection());
            moveDirection = Controller.transform.forward;

            PlayerAnimator.PlayAction(action.StateHash, action.CrossFadeDuration, action.PlaybackSpeed);
            actionPlayer.Play(action);
        }

        void IActionContext.BeginHit(float damageMultiplier, float hitStopDuration)
        {
            int damage = Mathf.RoundToInt(Loadout.AttackPower * damageMultiplier);
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

        void IActionContext.OpenCancelWindow() => isCancelWindowOpen = true;
        void IActionContext.CloseCancelWindow() => isCancelWindowOpen = false;
        void IActionContext.PlaySound(SoundData sound) => Attacker.CueSound(sound);
    }
}

namespace ProjectFantasy.Player
{
    // 지상 대기/걷기/달리기 (애니메이션은 속도로 분기), 줍기·퀵슬롯 입력 처리
    public sealed class PlayerLocomotionState : PlayerStateBase
    {
        public PlayerLocomotionState(PlayerController controller) : base(controller) { }

        public override void Tick(float deltaTime)
        {
            if (TryStartPrimaryAction()) return;

            if (Motor.CanJump && InputHandler.ConsumeJump())
            {
                Motor.Jump();
                Controller.ChangeState(Controller.AirState);
                return;
            }

            if (Motor.IsAirborne)
            {
                Controller.ChangeState(Controller.AirState);
                return;
            }

            if (TryStartSecondaryAction()) return;

            if (RangedWeapon.NeedsReload)
            {
                Controller.ChangeState(Controller.ReloadState);
                return;
            }

            if (InputHandler.ConsumeInteract() && ItemHandler.TryStartPickUp()) return;

            // 무기는 즉시 장착, 소모품은 사용 상태로 전이
            int quickSlot = InputHandler.ConsumeQuickSlot();
            if (quickSlot != PlayerInputHandler.NoQuickSlot)
            {
                ItemHandler.TryActivateQuickSlot(quickSlot);
                if (!Controller.IsInLocomotion) return;
            }

            ApplyMoveInput(MovementData.Acceleration, MovementData.Deceleration, deltaTime);
            PlayerAnimator.PlayLocomotion(Motor.HorizontalSpeed);
        }
    }
}

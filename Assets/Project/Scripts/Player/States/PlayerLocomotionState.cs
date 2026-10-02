namespace ProjectFantasy.Player
{
    // 지상 대기/걷기/달리기 (애니메이션은 속도로 분기)
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

            int cycleDirection = InputHandler.ConsumeWeaponCycle();
            if (cycleDirection != PlayerInputHandler.NoWeaponCycle)
            {
                Loadout.CycleWeapon(cycleDirection);
            }

            ApplyMoveInput(MovementData.Acceleration, MovementData.Deceleration, deltaTime);
            PlayerAnimator.PlayLocomotion(Motor.HorizontalSpeed);
        }
    }
}

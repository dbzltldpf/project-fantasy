namespace ProjectFantasy.Player
{
    // 지상 대기/걷기/달리기 (애니메이션은 속도로 분기)
    public sealed class PlayerLocomotionState : PlayerStateBase
    {
        public PlayerLocomotionState(PlayerController controller) : base(controller) { }

        public override void Tick(float deltaTime)
        {
            if (Controller.CanAttack && InputHandler.ConsumeAttack())
            {
                Controller.ChangeState(Controller.AttackState);
                return;
            }

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

            ApplyMoveInput(MovementData.Acceleration, MovementData.Deceleration, deltaTime);
            PlayerAnimator.PlayLocomotion(Motor.HorizontalSpeed);
        }
    }
}

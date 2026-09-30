namespace ProjectFantasy.Player
{
    // 점프/낙하 중 공중 제어
    public sealed class PlayerAirState : PlayerStateBase
    {
        public PlayerAirState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            PlayerAnimator.PlayAir();
        }

        public override void Tick(float deltaTime)
        {
            // 코요테 타임 내 점프 허용
            if (Motor.CanJump && InputHandler.ConsumeJump())
            {
                Motor.Jump();
            }

            ApplyMoveInput(MovementData.AirAcceleration, MovementData.AirAcceleration, deltaTime);

            if (Motor.IsGrounded && !Motor.IsRising)
            {
                Controller.ChangeState(Controller.LocomotionState);
            }
        }
    }
}

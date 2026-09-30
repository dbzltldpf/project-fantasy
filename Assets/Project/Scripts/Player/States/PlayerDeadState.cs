using UnityEngine;

namespace ProjectFantasy.Player
{
    // 사망 (종료 상태, 입력 무시)
    public sealed class PlayerDeadState : PlayerStateBase
    {
        public PlayerDeadState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            Attacker.EndSwing();
            InputHandler.ClearBuffers();
            Motor.SetVelocityImmediate(Vector3.zero);
            PlayerAnimator.PlayDeath();
        }

        public override void Tick(float deltaTime) { }
    }
}

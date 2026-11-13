using UnityEngine;

namespace ProjectFantasy.Player
{
    // 회피 (진입 시 방향 고정, 일정 속도로 이동 후 복귀)
    public sealed class PlayerDodgeState : PlayerStateBase
    {
        private float elapsedTime;
        private Vector3 dodgeDirection;
        private float nextAvailableTime;
        public bool IsReady => Time.time >= nextAvailableTime;
        public PlayerDodgeState(PlayerController controller) : base(controller) { }
        public override void Enter()
        {
            elapsedTime = 0;
            Controller.SetInvincible(true);
            dodgeDirection = Controller.GetCameraRelativeMove();
            dodgeDirection = (dodgeDirection == Vector3.zero) ? -Controller.transform.forward : dodgeDirection.normalized;
            Motor.SetVelocityImmediate(dodgeDirection * MovementData.DodgeSpeed);
            Vector3 localDirection = Controller.transform.InverseTransformDirection(dodgeDirection);
            PlayerAnimator.PlayDodge(localDirection);
        }
        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            Motor.SetVelocityImmediate(dodgeDirection * MovementData.DodgeSpeed);
            if (elapsedTime >= MovementData.DodgeDuration)
            {
                if (TryStartSecondaryAction()) return;
                Controller.ChangeState(Controller.LocomotionState);
            }
        }
        public override void Exit()
        {
            Controller.SetInvincible(false);
            nextAvailableTime = Time.time + MovementData.DodgeCooldown;
        }
    }

}

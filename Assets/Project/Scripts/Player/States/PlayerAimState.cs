using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 우클릭 조준 (숄더뷰, 카메라 방향 고정, 느린 스트레이프 이동)
    public sealed class PlayerAimState : PlayerStateBase
    {
        public override bool UsesAimView => true;

        public PlayerAimState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            Controller.AmmoVisual.SetNocked(true);
        }

        public override void Exit()
        {
            Controller.AmmoVisual.SetNocked(false);
            if (Controller.TrajectoryPreview != null) Controller.TrajectoryPreview.Hide();
        }

        public override void Tick(float deltaTime)
        {
            if (!InputHandler.IsSecondaryHeld || !RangedWeapon.CanAim)
            {
                Controller.ChangeState(Controller.LocomotionState);
                return;
            }

            if (Motor.IsAirborne)
            {
                Controller.ChangeState(Controller.AirState);
                return;
            }

            if (TryStartPrimaryAction()) return;

            ApplyMoveInput(MovementData.AimMoveSpeed, MovementData.Acceleration, MovementData.Deceleration, deltaTime, false);
            FaceAim(deltaTime);

            Vector3 localVelocity = Controller.transform.InverseTransformDirection(Motor.HorizontalVelocity);
            PlayerAnimator.PlayAimLocomotion(localVelocity, RangedWeapon.Current.AimIdleStateHash, RangedWeapon.Current.AimHoldTime);

            UpdateTrajectoryPreview();
        }

        // 실제 발사와 같은 계산으로 경로 미리보기
        private void UpdateTrajectoryPreview()
        {
            TrajectoryPreview preview = Controller.TrajectoryPreview;
            if (preview == null) return;

            if (!Controller.TryResolveLaunch(out Vector3 origin, out Vector3 launchVelocity, out ProjectileProfile profile))
            {
                preview.Hide();
                return;
            }

            int pointCount = Controller.RangedAttacker.PredictPath(origin, launchVelocity, profile, preview.Buffer);
            preview.Show(pointCount);
        }
    }
}

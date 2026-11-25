using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 우클릭 조준 (숄더뷰, 카메라 방향 고정, 느린 스트레이프 이동)
    public sealed class PlayerAimState : PlayerStateBase
    {
        // 활은 windup 시간만큼 조준해야 발사 가능 (비조준 발사와 템포 일치), 석궁은 즉시
        private bool CanRelease => !RangedWeapon.Current.HasDraw || Controller.Draw.Elapsed >= RangedWeapon.Current.WindupDuration;
        public override bool UsesAimView => true;

        public PlayerAimState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            Controller.AmmoVisual.SetNocked(true);
            Controller.Draw.Begin(RangedWeapon.Current.DrawDuration);
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
            if (CanRelease && TryStartPrimaryAction()) return;
            if (TryStartDodge())
            {
                Controller.ChangeState(Controller.DodgeState);
                return;
            }
            Controller.Draw.Tick(deltaTime);
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

            if (!Controller.TryResolveLaunch(RangedWeapon.Current.GetDrawSpeedMultiplier(Controller.Draw.Ratio), out Vector3 origin, out Vector3 launchVelocity, out ProjectileProfile profile))
            {
                preview.Hide();
                return;
            }

            int pointCount = Controller.RangedAttacker.PredictPath(origin, launchVelocity, profile, preview.Buffer);
            preview.Show(pointCount);
        }
    }
}

using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 원거리 발사: (비조준 시 windup) → fire 모션, release 시점에 화살 소모 후 발사
    public sealed class PlayerRangedFireState : PlayerStateBase
    {
        private RangedWeaponData weapon;
        private float elapsedTime;
        private bool isWindingUp;
        private bool hasReleased;
        private bool isAimedShot;

        public override bool UsesAimView => isAimedShot;

        public PlayerRangedFireState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            weapon = RangedWeapon.Current;
            isAimedShot = Controller.IsAimViewActive;
            hasReleased = false;

            Motor.SnapRotation(Controller.GetAimFacingDirection());
            Controller.AmmoVisual.SetNocked(true);

            // 조준 중이면 이미 당긴 상태라 windup 생략
            if (!isAimedShot && weapon.HasWindup) StartWindup();
            else StartFire();
        }

        public override void Exit()
        {
            Controller.AmmoVisual.SetNocked(false);
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            Motor.SetTargetVelocity(Vector3.zero, MovementData.Deceleration);
            FaceAim(deltaTime);

            if (isWindingUp)
            {
                if (elapsedTime >= weapon.WindupDuration) StartFire();
                return;
            }

            if (!hasReleased && elapsedTime >= weapon.ReleaseTime) Release();
            if (elapsedTime >= weapon.FireDuration) Finish();
        }

        private void StartWindup()
        {
            isWindingUp = true;
            elapsedTime = 0f;
            PlayerAnimator.PlayAction(weapon.WindupStateHash);
        }

        private void StartFire()
        {
            isWindingUp = false;
            elapsedTime = 0f;
            PlayerAnimator.PlayAction(weapon.FireStateHash);
        }

        private void Release()
        {
            hasReleased = true;
            Controller.AmmoVisual.SetNocked(false);

            if (!Controller.TryResolveLaunch(out Vector3 origin, out Vector3 launchVelocity, out ProjectileProfile profile)) return;
            if (!RangedWeapon.TryConsumeShot()) return;

            Controller.RangedAttacker.Fire(origin, launchVelocity, profile, Loadout.WeaponStats.AttackPower, DamageType.Physical);
        }

        private void Finish()
        {
            if (RangedWeapon.NeedsReload)
            {
                Controller.ChangeState(Controller.ReloadState);
                return;
            }

            bool keepAiming = InputHandler.IsSecondaryHeld && RangedWeapon.CanAim;
            Controller.ChangeState(keepAiming ? Controller.AimState : Controller.LocomotionState);
        }
    }
}

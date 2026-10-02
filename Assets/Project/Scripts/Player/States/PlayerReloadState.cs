using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 석궁 장전 (모션 종료 시 화살 1개를 장전, 피격 등으로 중단되면 장전 안 됨)
    public sealed class PlayerReloadState : PlayerStateBase
    {
        private RangedWeaponData weapon;
        private float elapsedTime;

        public PlayerReloadState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            weapon = RangedWeapon.Current;
            elapsedTime = 0f;
            PlayerAnimator.PlayAction(weapon.ReloadStateHash);
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            Motor.SetTargetVelocity(Vector3.zero, MovementData.Deceleration);
            if (elapsedTime < weapon.ReloadDuration) return;

            RangedWeapon.TryCompleteReload();

            bool keepAiming = InputHandler.IsSecondaryHeld && RangedWeapon.CanAim;
            Controller.ChangeState(keepAiming ? Controller.AimState : Controller.LocomotionState);
        }
    }
}

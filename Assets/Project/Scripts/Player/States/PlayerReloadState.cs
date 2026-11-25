using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 석궁 장전 (모션 종료 시 1발 장전, 회피·이동·피격으로 중단되면 장전 안 됨)
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
            if (TryStartDodge())
            {
                Controller.ChangeState(Controller.DodgeState);
                return;
            }
            if (HasMoveInput)
            {
                Controller.ChangeState(Controller.LocomotionState);
                return;
            }
            // 소모품 사용으로 상태가 바뀌었으면 종료
            TryHandleQuickSlot();
            if (!Controller.IsCurrentState(this)) return;
            // 무기가 바뀌면 (퀵슬롯·인벤토리) 장전 취소
            if (RangedWeapon.Current != weapon)
            {
                Controller.ChangeState(Controller.LocomotionState);
                return;
            }
            elapsedTime += deltaTime;
            Motor.SetTargetVelocity(Vector3.zero, MovementData.Deceleration);
            if (elapsedTime < weapon.ReloadDuration) return;

            RangedWeapon.TryCompleteReload();

            bool keepAiming = InputHandler.IsSecondaryHeld && RangedWeapon.CanAim;
            Controller.ChangeState(keepAiming ? Controller.AimState : Controller.LocomotionState);
        }
    }
}

using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 방패 가드 (느린 이동, 막기 경직/넉백, 가드 중 공격 가능)
    public sealed class PlayerGuardState : PlayerStateBase
    {
        private readonly ShieldGuard shieldGuard;
        private float blockStunRemaining;

        private ShieldData Shield => Loadout.ActiveShield;

        public PlayerGuardState(PlayerController controller) : base(controller)
        {
            shieldGuard = controller.ShieldGuard;
        }

        public override void Enter()
        {
            blockStunRemaining = 0f;
            shieldGuard.SetGuarding(true);
            PlayerAnimator.PlayGuard();
        }

        public override void Exit()
        {
            shieldGuard.SetGuarding(false);
        }

        public override void Tick(float deltaTime)
        {
            if (blockStunRemaining > 0f)
            {
                TickBlockStun(deltaTime);
                return;
            }

            if (!InputHandler.IsSecondaryHeld || !Loadout.CanGuard || Motor.IsAirborne)
            {
                Controller.ChangeState(Motor.IsAirborne ? Controller.AirState : Controller.LocomotionState);
                return;
            }

            // 가드 중 좌클릭: 근접 공격 또는 한손 석궁 발사
            if (TryStartPrimaryAction()) return;

            ApplyMoveInput(MovementData.GuardMoveSpeed, MovementData.Acceleration, MovementData.Deceleration, deltaTime);
        }

        // 가드 성공 시 PlayerController가 호출
        public void OnBlocked(in DamageInfo damageInfo)
        {
            blockStunRemaining = Shield.BlockStunDuration;
            Motor.SnapRotation(-damageInfo.Direction);
            Motor.SetVelocityImmediate(damageInfo.Direction * Shield.BlockKnockbackSpeed);
            PlayerAnimator.PlayBlockHit();
        }

        private void TickBlockStun(float deltaTime)
        {
            blockStunRemaining -= deltaTime;
            Motor.SetTargetVelocity(Vector3.zero, MovementData.Deceleration);

            if (blockStunRemaining <= 0f)
            {
                PlayerAnimator.PlayGuard();
            }
        }
    }
}

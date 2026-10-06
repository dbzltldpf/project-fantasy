using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.Magic;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 마법 시전: 시전 모션 → release 시점에 마법탄 발사 또는 지정 위치에 범위 마법 설치
    public sealed class PlayerCastState : PlayerStateBase
    {
        private SpellData spell;
        private Vector3 targetPosition;
        private bool hasTargetPosition;
        private bool isTargetedCast;
        private bool hasReleased;
        private float elapsedTime;

        // 시전 중에는 조준 모드 해제 (종료 시 우클릭 유지면 조준 모드 복귀)
        public PlayerCastState(PlayerController controller) : base(controller) { }

        // 범위 마법은 ChangeState 전에 위치 지정
        public void SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
            hasTargetPosition = true;
        }

        public override void Enter()
        {
            spell = MagicCaster.CurrentSpell;
            isTargetedCast = hasTargetPosition;
            hasReleased = false;
            elapsedTime = 0f;

            Motor.SnapRotation(isTargetedCast ? targetPosition - Controller.transform.position : Controller.GetAimFacingDirection());
            MagicCaster.StartCooldown();
            PlayerAnimator.PlayAction(spell.CastStateHash);
        }

        public override void Exit()
        {
            hasTargetPosition = false;
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            Motor.SetTargetVelocity(Vector3.zero, MovementData.Deceleration);

            if (!hasReleased && elapsedTime >= spell.ReleaseTime) Release();
            if (elapsedTime >= spell.CastDuration) Finish();
        }

        private void Release()
        {
            hasReleased = true;
            int damage = MagicCaster.CalculateDamage();

            switch (spell)
            {
                case ProjectileSpellData projectileSpell:
                    CastProjectile(projectileSpell, damage);
                    break;
                case AreaSpellData areaSpell when isTargetedCast:
                    Controller.SpellCaster.CastArea(areaSpell, targetPosition, damage);
                    break;
            }
        }

        // 조준점 방향 직선 발사
        private void CastProjectile(ProjectileSpellData projectileSpell, int damage)
        {
            ProjectileProfile profile = projectileSpell.ToProfile();
            Vector3 origin = Controller.transform.TransformPoint(MagicCaster.CurrentWeapon.MuzzleOffset);
            Vector3 launchVelocity = Controller.RangedAttacker.ResolveLaunchVelocity(origin, Controller.GetAimPoint(), profile);
            Controller.RangedAttacker.Fire(origin, launchVelocity, profile, damage, DamageType.Magic);
        }

        private void Finish()
        {
            bool keepTargeting = isTargetedCast && InputHandler.IsSecondaryHeld && MagicCaster.RequiresTargeting;
            Controller.ChangeState(keepTargeting ? Controller.SpellTargetState : Controller.LocomotionState);
        }
    }
}

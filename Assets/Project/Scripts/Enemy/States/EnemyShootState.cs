using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 사격: (석궁) 장전 → 조준(대상 추적, 조준 클립 끝까지 재생 후 Aim Duration만큼 유지) → 발사(대상 위치로 중력 보정 + 퍼짐) → 쿨타임 후 복귀
    // 평소에는 무기 대기 모션(Idle_A 등), 공격할 때마다 장전·조준부터 시작
    public sealed class EnemyShootState : EnemyStateBase
    {
        private enum Phase { Reload, Aim, Fire }

        private ShooterCombatStyle style;
        private RangedWeaponData weapon;
        private Phase phase;
        private float elapsedTime;
        private float aimHoldElapsed;
        private bool hasFired;

        public EnemyShootState(EnemyController controller) : base(controller) { }

        public override void Enter()
        {
            style = (ShooterCombatStyle)Data.CombatStyle;
            weapon = (RangedWeaponData)Data.Weapon;
            Navigator.Stop();

            if (weapon.RequiresReload) BeginPhase(Phase.Reload, weapon.ReloadStateHash);
            else BeginAim();
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;

            switch (phase)
            {
                case Phase.Reload:
                    FaceTarget(deltaTime);
                    if (elapsedTime >= weapon.ReloadDuration) BeginAim();
                    break;

                case Phase.Aim:
                    TickAim(deltaTime);
                    break;

                case Phase.Fire:
                    if (!hasFired && elapsedTime >= weapon.ReleaseTime) Fire();
                    if (elapsedTime >= weapon.FireDuration) Finish();
                    break;
            }
        }

        // 조준 클립이 끝난 뒤 추가 유지 시간이 지나면 발사
        private void TickAim(float deltaTime)
        {
            if (!Perception.HasTarget)
            {
                Controller.ReturnToCombat();
                return;
            }

            FaceTarget(deltaTime);
            if (!EnemyAnimator.IsStateFinished(weapon.AimIdleStateHash)) return;

            aimHoldElapsed += deltaTime;
            if (aimHoldElapsed >= style.AimDuration) BeginPhase(Phase.Fire, weapon.FireStateHash);
        }

        private void BeginAim()
        {
            phase = Phase.Aim;
            elapsedTime = 0f;
            aimHoldElapsed = 0f;
            EnemyAnimator.PlayHeldState(weapon.AimIdleStateHash, weapon.AimHoldTime);
        }

        private void BeginPhase(Phase next, int stateHash)
        {
            phase = next;
            elapsedTime = 0f;
            hasFired = false;
            EnemyAnimator.PlayState(stateHash, true);
        }

        // 대상이 사라졌으면 발사 생략
        private void Fire()
        {
            hasFired = true;
            if (!Perception.HasTarget) return;

            Vector3 origin = Controller.transform.TransformPoint(weapon.MuzzleOffset);
            ProjectileProfile profile = weapon.Ammo.ToProfile(weapon.LaunchSpeed);
            Ballistics.TrySolveLaunchVelocity(origin, style.GetAimPoint(Perception.Target), profile.Speed, profile.Gravity, out Vector3 velocity);
            Controller.RangedAttacker.Fire(origin, style.ApplySpread(velocity), profile, Data.AttackPower, DamageType.Physical);
        }

        private void Finish()
        {
            Controller.StartAttackCooldown();
            Controller.ReturnToCombat();
        }
    }
}

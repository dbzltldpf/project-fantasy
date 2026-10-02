using System;
using ProjectFantasy.Utils;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 조준 지점·발사 속도 계산, 경로 예측, 투사체 발사와 투사체·명중 이펙트 풀링
    [DisallowMultipleComponent]
    public sealed class RangedAttacker : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float maxAimDistance = 100f;
        [Tooltip("조준 레이 충돌 레이어 (Player, Projectile 제외)")]
        [SerializeField] private LayerMask aimLayers = Physics.DefaultRaycastLayers;
        [Tooltip("투사체/경로 미리보기 충돌 레이어 (Player, Projectile 제외)")]
        [SerializeField] private LayerMask projectileHitLayers = Physics.DefaultRaycastLayers;
        [SerializeField, Min(0.001f)] private float previewTimeStep = 0.03f;

        private readonly PrefabPool<Projectile> projectilePool = new PrefabPool<Projectile>();
        private readonly EffectPool effectPool = new EffectPool();

        public event Action<RangedShot> ShotFired;

        // 화면 중앙 레이가 처음 닿는 지점 (없으면 최대 거리 지점)
        public Vector3 ResolveAimPoint(Ray aimRay)
        {
            bool hasTarget = Physics.Raycast(aimRay, out RaycastHit hit, maxAimDistance, aimLayers, QueryTriggerInteraction.Ignore);
            return hasTarget ? hit.point : aimRay.GetPoint(maxAimDistance);
        }

        // 조준점 방향으로 직사 (보정 없음, 멀수록 중력으로 조준점 아래로 떨어짐)
        public Vector3 ResolveLaunchVelocity(Vector3 origin, Vector3 aimPoint, in ProjectileProfile profile)
        {
            return (aimPoint - origin).normalized * profile.Speed;
        }

        // 실제 비행과 같은 공식으로 경로 샘플링
        public int PredictPath(Vector3 origin, Vector3 launchVelocity, in ProjectileProfile profile, Vector3[] buffer)
        {
            return Ballistics.SamplePath(origin, launchVelocity, profile.Gravity, previewTimeStep, profile.Lifetime, projectileHitLayers, buffer);
        }

        public void Fire(Vector3 origin, Vector3 launchVelocity, in ProjectileProfile profile, int damage)
        {
            RangedShot shot = new RangedShot(origin, launchVelocity, damage, gameObject);

            if (profile.Prefab != null)
            {
                Projectile projectile = projectilePool.Rent(profile.Prefab);
                projectile.Launch(this, profile.Prefab, shot, profile, projectileHitLayers);
            }

            ShotFired?.Invoke(shot);
        }

        public void ReturnProjectile(Projectile prefab, Projectile projectile) => projectilePool.Return(prefab, projectile);

        public void PlayImpactEffect(in ProjectileProfile profile, Vector3 position, Vector3 normal)
        {
            effectPool.Spawn(profile.ImpactEffectPrefab, position, Quaternion.LookRotation(normal), EffectPool.OneShot);
        }
    }
}

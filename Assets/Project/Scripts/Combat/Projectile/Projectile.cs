using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 범용 투사체: 해석적 포물선 비행 + 구간 레이캐스트, 명중 시 피해·이펙트 후 박힘 또는 소멸, 풀 반환
    [DisallowMultipleComponent]
    public sealed class Projectile : MonoBehaviour
    {
        private Transform cachedTransform;
        private RangedAttacker owner;
        private Projectile sourcePrefab;
        private RangedShot shot;
        private ProjectileProfile profile;
        private LayerMask hitLayers;
        private Vector3 previousPosition;
        private float elapsedTime;
        private bool isStuck;

        // 박힌 대상 기준 로컬 자세 (부모로 붙이지 않아 대상 파괴 시에도 풀 반환 가능)
        private Transform stuckTarget;
        private Vector3 stuckLocalPosition;
        private Quaternion stuckLocalRotation;

        private void Awake()
        {
            cachedTransform = transform;
        }

        public void Launch(RangedAttacker shooter, Projectile prefab, in RangedShot rangedShot, in ProjectileProfile projectileProfile, LayerMask layers)
        {
            owner = shooter;
            sourcePrefab = prefab;
            shot = rangedShot;
            profile = projectileProfile;
            hitLayers = layers;
            elapsedTime = 0f;
            isStuck = false;
            stuckTarget = null;
            previousPosition = shot.Origin;

            cachedTransform.SetPositionAndRotation(shot.Origin, Quaternion.LookRotation(shot.LaunchVelocity));
            gameObject.SetActive(true);
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;

            if (isStuck)
            {
                if (elapsedTime >= profile.StickDuration) Release();
                return;
            }

            Vector3 nextPosition = Ballistics.GetPosition(shot.Origin, shot.LaunchVelocity, profile.Gravity, elapsedTime);
            Vector3 velocity = Ballistics.GetVelocity(shot.LaunchVelocity, profile.Gravity, elapsedTime);

            if (Ballistics.Linecast(previousPosition, nextPosition, hitLayers, out RaycastHit hit) && !IsInstigator(hit.collider))
            {
                HandleHit(hit, velocity);
                return;
            }

            cachedTransform.SetPositionAndRotation(nextPosition, Quaternion.LookRotation(velocity));
            previousPosition = nextPosition;

            if (elapsedTime >= profile.Lifetime) Release();
        }

        // 애니메이션·이동 이후 박힌 대상을 따라감, 대상이 사라지면 풀 반환
        private void LateUpdate()
        {
            if (!isStuck) return;

            if (stuckTarget == null || !stuckTarget.gameObject.activeInHierarchy)
            {
                Release();
                return;
            }

            cachedTransform.SetPositionAndRotation(stuckTarget.TransformPoint(stuckLocalPosition), stuckTarget.rotation * stuckLocalRotation);
        }

        private void HandleHit(in RaycastHit hit, Vector3 velocity)
        {
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                Vector3 direction = velocity;
                direction.y = 0f;
                target.TakeDamage(new DamageInfo(shot.Damage, hit.point, direction.normalized, shot.Instigator, shot.DamageType));
            }

            if (owner != null) owner.PlayImpactEffect(profile, hit.point, hit.normal);

            if (profile.StickOnHit) StickTo(hit.collider.transform, hit.point, Quaternion.LookRotation(velocity));
            else Release();
        }

        private void StickTo(Transform target, Vector3 position, Quaternion rotation)
        {
            stuckTarget = target;
            stuckLocalPosition = target.InverseTransformPoint(position);
            stuckLocalRotation = Quaternion.Inverse(target.rotation) * rotation;
            isStuck = true;
            elapsedTime = 0f;

            cachedTransform.SetPositionAndRotation(position, rotation);
        }

        private bool IsInstigator(Collider hitCollider) => shot.Instigator != null && hitCollider.transform.IsChildOf(shot.Instigator.transform);

        private void Release()
        {
            isStuck = false;
            stuckTarget = null;
            if (owner != null) owner.ReturnProjectile(sourcePrefab, this);
            else gameObject.SetActive(false);
        }
    }
}

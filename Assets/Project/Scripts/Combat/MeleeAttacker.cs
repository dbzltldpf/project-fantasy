using System.Collections.Generic;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 근접 공격 타격 판정 (한 번 휘두를 때 대상당 1회 타격)
    [DisallowMultipleComponent]
    public sealed class MeleeAttacker : MonoBehaviour
    {
        private const int HitBufferSize = 16;

        [SerializeField] private Transform hitOrigin;
        [SerializeField] private Vector3 hitOffset = new Vector3(0f, 1f, 1f);
        [SerializeField, Min(0f)] private float hitRadius = 0.8f;
        [SerializeField] private LayerMask targetLayers = Physics.AllLayers;
        [SerializeField] private Color gizmoColor = Color.red;

        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

        private IDamageable owner;
        private int currentDamage;

        public bool IsSwinging { get; private set; }

        private void Awake()
        {
            owner = GetComponent<IDamageable>();
            if (hitOrigin == null) hitOrigin = transform;
        }

        // 장착 무기에 맞춰 판정 범위 교체
        public void SetHitShape(Vector3 offset, float radius)
        {
            hitOffset = offset;
            hitRadius = radius;
        }

        public void BeginSwing(int damage)
        {
            currentDamage = damage;
            hitTargets.Clear();
            IsSwinging = true;
        }

        public void EndSwing()
        {
            IsSwinging = false;
            hitTargets.Clear();
        }

        // 히트 윈도우 동안 매 프레임 호출
        public void TickHit()
        {
            if (!IsSwinging) return;

            Vector3 center = hitOrigin.TransformPoint(hitOffset);
            int hitCount = Physics.OverlapSphereNonAlloc(center, hitRadius, hitBuffer, targetLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = hitBuffer[i];
                IDamageable target = hitCollider.GetComponentInParent<IDamageable>();
                if (target == null || target == owner || !target.IsAlive || !hitTargets.Add(target)) continue;

                Vector3 hitPoint = hitCollider.ClosestPointOnBounds(center);
                target.TakeDamage(new DamageInfo(currentDamage, hitPoint, GetHitDirection(hitPoint), gameObject));
            }
        }

        private Vector3 GetHitDirection(Vector3 hitPoint)
        {
            Vector3 direction = hitPoint - hitOrigin.position;
            direction.y = 0f;
            return direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : hitOrigin.forward;
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = hitOrigin != null ? hitOrigin : transform;
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(origin.TransformPoint(hitOffset), hitRadius);
        }
    }
}

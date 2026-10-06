using System.Collections.Generic;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 근접 타격 판정 (칼날 궤적 스윕 또는 몸 기준 구체, 판정 구간마다 대상당 1회 타격)
    [DisallowMultipleComponent]
    public sealed class MeleeAttacker : MonoBehaviour
    {
        private const int HitBufferSize = 16;

        private static readonly Vector3 DefaultHitOffset = new Vector3(0f, 1f, 1f);
        private const float DefaultHitRadius = 0.8f;

        [Tooltip("몸 기준 판정의 원점·방향 (비우면 자신)")]
        [SerializeField] private Transform hitOrigin;
        [Tooltip("타격 대상 레이어 (자기 레이어 제외 권장)")]
        [SerializeField] private LayerMask targetLayers = Physics.AllLayers;
        [Tooltip("선택 시 판정 범위 Gizmo 색")]
        [SerializeField] private Color gizmoColor = Color.red;

        // 장착 무기 데이터가 SetHitShape로 덮어씀 (인스펙터 노출 안 함)
        private Vector3 hitOffset = DefaultHitOffset;
        private float hitRadius = DefaultHitRadius;

        private readonly Collider[] hitBuffer = new Collider[HitBufferSize];
        private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
        private readonly WeaponTrace weaponTrace = new WeaponTrace();

        private IDamageable owner;
        private HitStop hitStop;
        private int currentDamage;
        private float currentHitStopDuration;

        public bool IsSwinging { get; private set; }

        private void Awake()
        {
            owner = GetComponent<IDamageable>();
            hitStop = GetComponent<HitStop>();
            if (hitOrigin == null) hitOrigin = transform;
        }

        // 칼날 정보가 없는 무기(맨손 등)용 몸 기준 판정 범위
        public void SetHitShape(Vector3 offset, float radius)
        {
            hitOffset = offset;
            hitRadius = radius;
        }

        // 장착 무기 모델의 칼날 (모델 로컬 좌표)
        public void SetBlade(Transform blade, Vector3 localBase, Vector3 localTip, float radius) => weaponTrace.Attach(blade, localBase, localTip, radius);

        public void ClearBlade() => weaponTrace.Detach();

        public void BeginSwing(int damage, float hitStopDuration)
        {
            currentDamage = damage;
            currentHitStopDuration = hitStopDuration;
            hitTargets.Clear();
            IsSwinging = true;

            if (weaponTrace.IsAttached) weaponTrace.Begin();
        }

        public void EndSwing()
        {
            IsSwinging = false;
            hitTargets.Clear();
        }

        // 판정 구간 동안 매 프레임 호출
        public void TickHit()
        {
            if (!IsSwinging) return;

            if (weaponTrace.IsAttached) SweepBlade();
            else OverlapBody();
        }

        // 샘플 지점마다 이전 → 현재 위치 캡슐로 휘두른 궤적 전체 판정
        private void SweepBlade()
        {
            weaponTrace.Advance();

            for (int i = 0; i < weaponTrace.SampleCount; i++)
            {
                Vector3 current = weaponTrace.GetCurrent(i);
                int hitCount = Physics.OverlapCapsuleNonAlloc(
                    weaponTrace.GetPrevious(i), current, weaponTrace.Radius, hitBuffer, targetLayers, QueryTriggerInteraction.Ignore);
                ApplyHits(hitCount, current);
            }
        }

        private void OverlapBody()
        {
            Vector3 center = hitOrigin.TransformPoint(hitOffset);
            int hitCount = Physics.OverlapSphereNonAlloc(center, hitRadius, hitBuffer, targetLayers, QueryTriggerInteraction.Ignore);
            ApplyHits(hitCount, center);
        }

        private void ApplyHits(int hitCount, Vector3 referencePoint)
        {
            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = hitBuffer[i];
                IDamageable target = hitCollider.GetComponentInParent<IDamageable>();
                if (target == null || target == owner || !target.IsAlive || !hitTargets.Add(target)) continue;

                Vector3 hitPoint = hitCollider.ClosestPointOnBounds(referencePoint);
                target.TakeDamage(new DamageInfo(currentDamage, hitPoint, GetHitDirection(hitPoint), gameObject));
                ApplyHitStop(hitCollider);
            }
        }

        // 공격자와 피격자를 함께 정지 (명중 시에만 탐색)
        private void ApplyHitStop(Collider hitCollider)
        {
            if (currentHitStopDuration <= 0f) return;

            if (hitStop != null) hitStop.Apply(currentHitStopDuration);

            HitStop targetHitStop = hitCollider.GetComponentInParent<HitStop>();
            if (targetHitStop != null) targetHitStop.Apply(currentHitStopDuration);
        }

        private Vector3 GetHitDirection(Vector3 hitPoint)
        {
            Vector3 direction = hitPoint - hitOrigin.position;
            direction.y = 0f;
            return direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : hitOrigin.forward;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;

            if (weaponTrace.IsAttached)
            {
                for (int i = 0; i < weaponTrace.SampleCount; i++)
                {
                    Gizmos.DrawWireSphere(weaponTrace.GetWorldPoint(i), weaponTrace.Radius);
                }
                return;
            }

            Transform origin = hitOrigin != null ? hitOrigin : transform;
            Gizmos.DrawWireSphere(origin.TransformPoint(hitOffset), hitRadius);
        }
    }
}

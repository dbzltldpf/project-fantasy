using System;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 대상 감지: 일정 간격으로 거리 → 시야각 → 가림 순 판정 (NonAlloc), 공격받으면 공격자를 대상으로 잡고 일정 시간 거리 무관 추적
    [DisallowMultipleComponent]
    public sealed class EnemyPerception : MonoBehaviour
    {
        private const int ScanBufferSize = 8;
        private const float HalfAngleDivisor = 2f;

        [Tooltip("감지 대상 레이어 (Player)")]
        [SerializeField] private LayerMask targetLayers;
        [Tooltip("시야를 가리는 레이어 (지형·벽, 캐릭터 제외)")]
        [SerializeField] private LayerMask obstacleLayers = Physics.DefaultRaycastLayers;
        [Tooltip("눈 높이 (가림 판정 레이 시작점, m)")]
        [SerializeField, Min(0f)] private float eyeHeight = 1.5f;
        [Tooltip("감지 간격 (초)")]
        [SerializeField, Min(0.01f)] private float scanInterval = 0.2f;

        private readonly Collider[] scanBuffer = new Collider[ScanBufferSize];
        private float sightRange;
        private float sightHalfAngle;
        private float loseRange;
        private float provokedDuration;
        private float provokedEndTime;
        private float nextScanTime;
        private IDamageable targetDamageable;

        public Transform Target { get; private set; }
        public bool HasTarget => Target != null && targetDamageable != null && targetDamageable.IsAlive;

        // 대상 획득(대상) / 상실(null)
        public event Action<Transform> TargetChanged;

        public void Configure(float sight, float sightAngle, float lose, float provoked)
        {
            sightRange = sight;
            sightHalfAngle = sightAngle / HalfAngleDivisor;
            loseRange = lose;
            provokedDuration = provoked;
        }

        public void Tick()
        {
            if (Target != null && !IsTargetValid()) ClearTarget();
            if (Target != null || Time.time < nextScanTime) return;

            nextScanTime = Time.time + scanInterval;
            Scan();
        }

        // 시야 밖·먼 거리에서 맞아도 공격자를 대상으로 (맞을 때마다 추적 시간 갱신)
        public void NotifyAttacked(GameObject instigator)
        {
            if (instigator == null) return;

            IDamageable damageable = instigator.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) return;

            provokedEndTime = Time.time + provokedDuration;
            SetTarget(((Component)damageable).transform, damageable);
        }

        public void ClearTarget()
        {
            provokedEndTime = 0f;
            if (Target == null) return;

            Target = null;
            targetDamageable = null;
            TargetChanged?.Invoke(null);
        }

        public Vector3 GetDirectionToTarget()
        {
            Vector3 direction = Target.position - transform.position;
            direction.y = 0f;
            return direction;
        }

        public float GetDistanceToTarget() => GetDirectionToTarget().magnitude;

        // 공격받은 직후에는 추적 포기 거리를 무시 (리쉬는 EnemyController가 판정)
        private bool IsTargetValid()
        {
            if (targetDamageable == null || !targetDamageable.IsAlive || !Target.gameObject.activeInHierarchy) return false;
            return Time.time < provokedEndTime || GetDirectionToTarget().sqrMagnitude <= loseRange * loseRange;
        }

        private void Scan()
        {
            Vector3 origin = transform.position;
            int count = Physics.OverlapSphereNonAlloc(origin, sightRange, scanBuffer, targetLayers, QueryTriggerInteraction.Ignore);

            float closestSqr = float.MaxValue;
            IDamageable closest = null;
            for (int i = 0; i < count; i++)
            {
                IDamageable damageable = scanBuffer[i].GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive) continue;

                Transform candidate = ((Component)damageable).transform;
                Vector3 toCandidate = candidate.position - origin;
                toCandidate.y = 0f;

                float sqrDistance = toCandidate.sqrMagnitude;
                if (sqrDistance >= closestSqr || !IsInSight(candidate, toCandidate)) continue;

                closestSqr = sqrDistance;
                closest = damageable;
            }

            if (closest != null) SetTarget(((Component)closest).transform, closest);
        }

        private bool IsInSight(Transform candidate, Vector3 flatDirection)
        {
            if (Vector3.Angle(transform.forward, flatDirection) > sightHalfAngle) return false;

            Vector3 eye = transform.position + Vector3.up * eyeHeight;
            Vector3 targetEye = candidate.position + Vector3.up * eyeHeight;
            return !Physics.Linecast(eye, targetEye, obstacleLayers, QueryTriggerInteraction.Ignore);
        }

        private void SetTarget(Transform target, IDamageable damageable)
        {
            if (target == Target) return;

            Target = target;
            targetDamageable = damageable;
            TargetChanged?.Invoke(target);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }
    }
}

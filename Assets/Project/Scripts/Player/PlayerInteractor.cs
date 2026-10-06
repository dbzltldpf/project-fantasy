using System;
using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 주변 필드 아이템 중 가장 가까운 것을 줍기 대상으로 추적 (일정 간격 NonAlloc 탐색)
    [DisallowMultipleComponent]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        private const int BufferSize = 16;

        [Tooltip("줍기 탐색 반경 (m)")]
        [SerializeField, Min(0f)] private float radius = 1.5f;
        [Tooltip("필드 아이템 레이어 (Interactable)")]
        [SerializeField] private LayerMask interactableLayers;
        [Tooltip("주변 탐색 간격 (초)")]
        [SerializeField, Min(0f)] private float scanInterval = 0.1f;

        private readonly Collider[] buffer = new Collider[BufferSize];
        private float nextScanTime;

        public WorldItem Target { get; private set; }

        public event Action<WorldItem> TargetChanged;

        private void Update()
        {
            if (Time.time < nextScanTime) return;

            nextScanTime = Time.time + scanInterval;
            Scan();
        }

        // 줍기 직후 등 즉시 갱신
        public void Scan()
        {
            Vector3 origin = transform.position;
            int count = Physics.OverlapSphereNonAlloc(origin, radius, buffer, interactableLayers, QueryTriggerInteraction.Collide);

            WorldItem nearest = null;
            float nearestSqrDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                WorldItem item = buffer[i].GetComponentInParent<WorldItem>();
                if (item == null || item.Item == null) continue;

                float sqrDistance = (item.transform.position - origin).sqrMagnitude;
                if (sqrDistance >= nearestSqrDistance) continue;

                nearest = item;
                nearestSqrDistance = sqrDistance;
            }

            SetTarget(nearest);
        }

        private void SetTarget(WorldItem target)
        {
            if (target == Target) return;

            Target = target;
            TargetChanged?.Invoke(Target);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}

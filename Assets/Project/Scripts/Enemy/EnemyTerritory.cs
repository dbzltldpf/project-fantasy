using UnityEngine;
namespace ProjectFantasy.Enemy
{
    // 적 진영 영역: 대상이 이 안에 있는 동안만 추격 (울타리 안쪽에 배치)
    [DisallowMultipleComponent]
    public sealed class EnemyTerritory : MonoBehaviour
    {
        [Tooltip("영역을 이루는 콜라이더들 (Convex 조각 여러 개로 울타리 모양을 덮음)")]
        [SerializeField] private Collider[] areas;
        public bool Contains(Vector3 point)
        {
            foreach (var area in areas)
            {
                if (area.ClosestPoint(point) == point) return true;
            }
            return false;
        }
    }

}

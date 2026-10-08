using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 판정에 걸린 콜라이더 묶음에서 공격이 실제로 닿은 콜라이더를 고르는 도구 (근접·범위 공격 공용)
    public static class HitColliderUtility
    {
        // buffer 앞쪽 count개 중 target에 속한 콜라이더에서 referencePoint와 가장 가까운 것 (bufferTargets = buffer와 같은 칸의 대상, 없으면 null)
        public static Collider FindNearest(Collider[] buffer, IDamageable[] bufferTargets, int count, IDamageable target, Vector3 referencePoint)
        {
            Collider closest = null;
            float closestSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Collider candidate = buffer[i];
                if (bufferTargets[i] != target) continue;

                Vector3 point = candidate.ClosestPoint(referencePoint);
                float sqrDistance = (point - referencePoint).sqrMagnitude;

                if (sqrDistance < closestSqr)
                {
                    closestSqr = sqrDistance;
                    closest = candidate;
                }
            }
            return closest;
        }
    }
}
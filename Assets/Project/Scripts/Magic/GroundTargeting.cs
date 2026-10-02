using UnityEngine;

namespace ProjectFantasy.Magic
{
    // 조준 레이가 닿는 지면 지점 (최대 사거리를 넘으면 사거리 끝 지면으로 제한)
    public static class GroundTargeting
    {
        private const float ProbeDistanceFactor = 2f;

        public static bool TryResolve(Vector3 casterPosition, Ray aimRay, float maxRange, float maxRayDistance, float probeHeight,
            LayerMask groundLayers, out Vector3 point)
        {
            bool hitGround = Physics.Raycast(aimRay, out RaycastHit hit, maxRayDistance, groundLayers, QueryTriggerInteraction.Ignore);
            Vector3 candidate = hitGround ? hit.point : aimRay.GetPoint(maxRayDistance);

            Vector3 flatOffset = candidate - casterPosition;
            flatOffset.y = 0f;

            if (hitGround && flatOffset.sqrMagnitude <= maxRange * maxRange)
            {
                point = hit.point;
                return true;
            }

            // 사거리 끝 위에서 아래로 쏴서 지면에 붙임
            Vector3 clamped = casterPosition + Vector3.ClampMagnitude(flatOffset, maxRange);
            Vector3 probeOrigin = clamped + Vector3.up * probeHeight;
            if (Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit groundHit, probeHeight * ProbeDistanceFactor, groundLayers, QueryTriggerInteraction.Ignore))
            {
                point = groundHit.point;
                return true;
            }

            point = default;
            return false;
        }
    }
}

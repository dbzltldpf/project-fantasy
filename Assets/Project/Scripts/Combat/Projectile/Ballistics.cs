using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 중력 포물선 위치 계산과 경로 샘플링 (미리보기/실제 비행이 같은 공식 사용)
    public static class Ballistics
    {
        private const float HalfFactor = 0.5f;
        private const int OriginIndex = 0;

        public static Vector3 GetPosition(Vector3 origin, Vector3 launchVelocity, float gravity, float time)
        {
            return origin + launchVelocity * time + Vector3.down * (HalfFactor * gravity * time * time);
        }

        public static Vector3 GetVelocity(Vector3 launchVelocity, float gravity, float time)
        {
            return launchVelocity + Vector3.down * (gravity * time);
        }

        public static bool Linecast(Vector3 from, Vector3 to, LayerMask layers, out RaycastHit hit)
        {
            return Physics.Linecast(from, to, out hit, layers, QueryTriggerInteraction.Ignore);
        }

        // 경로를 timeStep 간격으로 샘플링, 충돌 지점에서 종료 (채운 점 개수 반환)
        public static int SamplePath(Vector3 origin, Vector3 launchVelocity, float gravity, float timeStep, float maxTime,
            LayerMask layers, Vector3[] buffer)
        {
            buffer[OriginIndex] = origin;
            int count = OriginIndex + 1;
            Vector3 previous = origin;

            for (; count < buffer.Length; count++)
            {
                float time = count * timeStep;
                if (time > maxTime) break;

                Vector3 next = GetPosition(origin, launchVelocity, gravity, time);
                if (Linecast(previous, next, layers, out RaycastHit hit))
                {
                    buffer[count] = hit.point;
                    return count + 1;
                }

                buffer[count] = next;
                previous = next;
            }

            return count;
        }
    }
}

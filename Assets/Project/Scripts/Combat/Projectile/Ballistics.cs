using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 중력 포물선 위치 계산과 경로 샘플링 (미리보기/실제 비행이 같은 공식 사용)
    public static class Ballistics
    {
        private const float HalfFactor = 0.5f;
        private const float DoubleFactor = 2f;
        private const int OriginIndex = 0;

        public static Vector3 GetPosition(Vector3 origin, Vector3 launchVelocity, float gravity, float time)
        {
            return origin + launchVelocity * time + Vector3.down * (HalfFactor * gravity * time * time);
        }

        public static Vector3 GetVelocity(Vector3 launchVelocity, float gravity, float time)
        {
            return launchVelocity + Vector3.down * (gravity * time);
        }

        // 고정 속력으로 target에 닿는 낮은 궤도 발사 속도 (사거리 밖이면 false, 중력 없으면 직선)
        public static bool TrySolveLaunchVelocity(Vector3 origin, Vector3 target, float speed, float gravity, out Vector3 velocity)
        {
            Vector3 toTarget = target - origin;
            Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
            float horizontal = flat.magnitude;

            if (gravity <= Mathf.Epsilon || horizontal <= Mathf.Epsilon)
            {
                velocity = toTarget.normalized * speed;
                return true;
            }

            float speedSqr = speed * speed;
            float discriminant = speedSqr * speedSqr - gravity * (gravity * horizontal * horizontal + DoubleFactor * toTarget.y * speedSqr);
            if (discriminant < 0f)
            {
                velocity = toTarget.normalized * speed;
                return false;
            }

            // tanθ = (v² − √판별식) / (g·x) : 두 해 중 낮은 궤도
            float angle = Mathf.Atan((speedSqr - Mathf.Sqrt(discriminant)) / (gravity * horizontal));
            velocity = flat / horizontal * (speed * Mathf.Cos(angle)) + Vector3.up * (speed * Mathf.Sin(angle));
            return true;
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

using UnityEngine;
using UnityEngine.AI;

namespace ProjectFantasy.Enemy
{
    // NavMeshAgent 래퍼: 목적지 이동·정지·회전(직접 제어)·넉백·일시 정지·순간 이동
    [RequireComponent(typeof(NavMeshAgent))]
    [DisallowMultipleComponent]
    public sealed class EnemyNavigator : MonoBehaviour
    {
        private const float RepathDistance = 0.5f;
        private const float SampleMaxDistance = 2f;
        private const int SampleAttempts = 8;

        private NavMeshAgent agent;
        private Vector3 lastDestination;
        private Vector3 knockbackVelocity;
        private float knockbackDeceleration;
        private bool isPaused;
        private bool wasStoppedBeforePause;

        public float HorizontalSpeed
        {
            get
            {
                if (!agent.enabled) return 0f;
                Vector3 velocity = agent.velocity;
                velocity.y = 0f;
                return velocity.magnitude;
            }
        }

        public bool HasArrived => agent.enabled && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
        }

        // 넉백은 경로와 무관하게 NavMesh 위에서만 밀림
        public void Tick(float deltaTime)
        {
            if (!agent.enabled || knockbackVelocity.sqrMagnitude <= Mathf.Epsilon) return;

            agent.Move(knockbackVelocity * deltaTime);
            knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, knockbackDeceleration * deltaTime);
        }

        public void MoveTo(Vector3 destination, float speed)
        {
            if (!agent.enabled) return;

            agent.speed = speed;
            agent.isStopped = false;
            if (agent.hasPath && (destination - lastDestination).sqrMagnitude < RepathDistance * RepathDistance) return;

            lastDestination = destination;
            agent.SetDestination(destination);
        }

        // 경로 없이 직접 이동 (공격 전진 등, NavMesh 밖으로는 나가지 않음)
        public void MoveImmediate(Vector3 velocity, float deltaTime)
        {
            if (agent.enabled && velocity.sqrMagnitude > Mathf.Epsilon) agent.Move(velocity * deltaTime);
        }

        public void Stop()
        {
            if (!agent.enabled) return;

            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        public void FaceTowards(Vector3 direction, float turnSpeed, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon) return;

            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * deltaTime);
        }

        // 경로 이동 중 진행 방향 바라보기
        public void FaceMovement(float turnSpeed, float deltaTime)
        {
            if (agent.enabled) FaceTowards(agent.desiredVelocity, turnSpeed, deltaTime);
        }

        public void SnapRotation(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > Mathf.Epsilon) transform.rotation = Quaternion.LookRotation(direction);
        }

        public void Knockback(Vector3 velocity, float deceleration)
        {
            Stop();
            knockbackVelocity = velocity;
            knockbackDeceleration = deceleration;
        }

        // 히트스톱 중 이동 정지, 해제 시 이전 상태 복원
        public void SetPaused(bool paused)
        {
            if (!agent.enabled || isPaused == paused) return;

            isPaused = paused;
            if (paused)
            {
                wasStoppedBeforePause = agent.isStopped;
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = wasStoppedBeforePause;
            }
        }

        // 리스폰: 에이전트를 켜고 NavMesh 위 지점으로 이동
        public void Warp(Vector3 position, Quaternion rotation)
        {
            agent.enabled = true;
            agent.Warp(position);
            transform.rotation = rotation;
            knockbackVelocity = Vector3.zero;
            isPaused = false;
            Stop();
        }

        // 사망: 다른 적·플레이어가 지나가도록 에이전트 끔
        public void Disable()
        {
            knockbackVelocity = Vector3.zero;
            agent.enabled = false;
        }

        // 반경 안 NavMesh 위 무작위 지점
        public static bool TryGetRandomPoint(Vector3 center, float radius, out Vector3 point)
        {
            for (int i = 0; i < SampleAttempts; i++)
            {
                Vector2 offset = Random.insideUnitCircle * radius;
                Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, SampleMaxDistance, NavMesh.AllAreas))
                {
                    point = hit.position;
                    return true;
                }
            }

            point = center;
            return false;
        }
    }
}

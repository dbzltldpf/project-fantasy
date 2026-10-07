using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 스폰 지점 주변 배회 (대기 → 무작위 지점으로 걷기 반복), 대상 발견 시 추격
    public sealed class EnemyPatrolState : EnemyStateBase
    {
        private float waitRemaining;
        private bool isMoving;

        public EnemyPatrolState(EnemyController controller) : base(controller) { }

        public override void Enter()
        {
            BeginWait();
        }

        public override void Tick(float deltaTime)
        {
            if (Perception.HasTarget)
            {
                Controller.ChangeState(Controller.ChaseState);
                return;
            }

            if (isMoving)
            {
                if (Navigator.HasArrived) BeginWait();
            }
            else
            {
                waitRemaining -= deltaTime;
                if (waitRemaining <= 0f) BeginMove();
            }

            UpdateLocomotion(deltaTime);
        }

        private void BeginWait()
        {
            isMoving = false;
            waitRemaining = Data.RollIdleTime();
            Navigator.Stop();
        }

        private void BeginMove()
        {
            if (!EnemyNavigator.TryGetRandomPoint(Controller.HomePosition, Data.PatrolRadius, out Vector3 point))
            {
                BeginWait();
                return;
            }

            isMoving = true;
            Navigator.MoveTo(point, Data.WalkSpeed);
        }
    }
}

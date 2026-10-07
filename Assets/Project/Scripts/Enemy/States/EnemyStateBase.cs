using ProjectFantasy.Core;

namespace ProjectFantasy.Enemy
{
    // 적 상태 공통 베이스 (컴포넌트 참조 캐싱)
    public abstract class EnemyStateBase : IState
    {
        protected readonly EnemyController Controller;
        protected readonly EnemyNavigator Navigator;
        protected readonly EnemyAnimator EnemyAnimator;
        protected readonly EnemyPerception Perception;

        protected EnemyData Data => Controller.Data;

        protected EnemyStateBase(EnemyController controller)
        {
            Controller = controller;
            Navigator = controller.Navigator;
            EnemyAnimator = controller.EnemyAnimator;
            Perception = controller.Perception;
        }

        public virtual void Enter() { }
        public abstract void Tick(float deltaTime);
        public virtual void Exit() { }

        // 이동 속도에 맞는 대기·걷기·달리기 모션 + 진행 방향 회전
        protected void UpdateLocomotion(float deltaTime)
        {
            Navigator.FaceMovement(Data.TurnSpeed, deltaTime);
            EnemyAnimator.PlayLocomotion(Navigator.HorizontalSpeed);
        }

        protected void FaceTarget(float deltaTime)
        {
            if (Perception.HasTarget) Navigator.FaceTowards(Perception.GetDirectionToTarget(), Data.TurnSpeed, deltaTime);
        }
    }
}

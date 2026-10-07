namespace ProjectFantasy.Enemy
{
    // 대상 추격, 공격 거리 안에서는 멈춰 바라보며 공격·막기 선택, 대상 상실·리쉬 초과 시 귀환
    public sealed class EnemyChaseState : EnemyStateBase
    {
        public EnemyChaseState(EnemyController controller) : base(controller) { }

        public override void Tick(float deltaTime)
        {
            if (!Perception.HasTarget || Controller.IsBeyondLeash)
            {
                Controller.ChangeState(Controller.ReturnState);
                return;
            }

            if (Perception.GetDistanceToTarget() > Data.AttackRange)
            {
                Navigator.MoveTo(Perception.Target.position, Data.RunSpeed);
                UpdateLocomotion(deltaTime);
                return;
            }

            Navigator.Stop();
            FaceTarget(deltaTime);
            EnemyAnimator.PlayLocomotion(Navigator.HorizontalSpeed);
            Controller.TryStartCombatAction();
        }
    }
}

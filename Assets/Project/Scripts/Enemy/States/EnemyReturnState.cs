namespace ProjectFantasy.Enemy
{
    // 리쉬: 대상을 버리고 스폰 지점으로 달려가 도착하면 체력 회복 후 배회
    public sealed class EnemyReturnState : EnemyStateBase
    {
        public EnemyReturnState(EnemyController controller) : base(controller) { }

        public override void Enter()
        {
            Perception.ClearTarget();
            Navigator.MoveTo(Controller.HomePosition, Data.RunSpeed);
        }

        public override void Tick(float deltaTime)
        {
            UpdateLocomotion(deltaTime);
            if (!Navigator.HasArrived) return;

            Controller.ResetCombat();
            Controller.ChangeState(Controller.PatrolState);
        }
    }
}

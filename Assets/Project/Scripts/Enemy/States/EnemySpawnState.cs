namespace ProjectFantasy.Enemy
{
    // 등장 모션 (땅에서 올라옴), 끝나면 배회
    public sealed class EnemySpawnState : EnemyStateBase
    {
        private float remainingTime;

        public EnemySpawnState(EnemyController controller) : base(controller) { }

        public override void Enter()
        {
            remainingTime = Data.SpawnDuration;
            Navigator.Stop();
            EnemyAnimator.PlaySpawn();
        }

        public override void Tick(float deltaTime)
        {
            remainingTime -= deltaTime;
            if (remainingTime <= 0f) Controller.ChangeState(Controller.PatrolState);
        }
    }
}

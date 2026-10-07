namespace ProjectFantasy.Enemy
{
    // 사망 모션 후 시체 유지 시간이 지나면 비활성화 (드랍은 EnemyLoot, 리스폰은 EnemySpawner)
    public sealed class EnemyDeadState : EnemyStateBase
    {
        private float remainingTime;

        public EnemyDeadState(EnemyController controller) : base(controller) { }

        public override void Enter()
        {
            remainingTime = Data.CorpseDuration;
            Controller.Attacker.EndSwing();
            Controller.SetCollidersEnabled(false);
            Perception.ClearTarget();
            Navigator.Disable();
            EnemyAnimator.PlayDeath();
        }

        public override void Tick(float deltaTime)
        {
            remainingTime -= deltaTime;
            if (remainingTime <= 0f) Controller.Despawn();
        }
    }
}

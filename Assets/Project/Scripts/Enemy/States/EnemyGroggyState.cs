using UnityEngine;
namespace ProjectFantasy.Enemy
{
    // 일정 시간 행동 불능 (맞아도 경직 없음), 끝나면 전투 복귀
    public sealed class EnemyGroggyState : EnemyStateBase
    {
        private float duration;
        private float elapsedTime;

        public EnemyGroggyState(EnemyController controller) : base(controller) { }

        public void SetDuration(float duration)
        {
            this.duration = duration;
        }
        public override void Enter()
        {
            elapsedTime = 0f;
            Controller.Attacker.EndSwing();
            Navigator.Stop();
            EnemyAnimator.PlayGroggy();
        }
        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (elapsedTime < duration) return;

            Controller.ReturnToCombat();
        }
    }
}

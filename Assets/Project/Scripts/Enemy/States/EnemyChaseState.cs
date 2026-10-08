namespace ProjectFantasy.Enemy
{
    // 대상 추격: 전투 방식의 공격 거리 밖이거나 사선이 가리면 접근, 안이면 멈춰 바라보며 공격 선택 (가까워도 그 자리에서 공격)
    // 대상 상실·리쉬 초과 시 귀환
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

            EnemyCombatStyle style = Data.CombatStyle;
            bool isOutOfRange = Perception.GetDistanceToTarget() > style.GetAttackRange(Data);
            bool isBlocked = style.RequiresLineOfSight && !Perception.HasLineOfSightToTarget();

            if (isOutOfRange || isBlocked)
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

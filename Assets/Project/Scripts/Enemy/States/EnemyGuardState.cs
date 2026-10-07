using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 방패 막기: 일정 시간 대상을 바라보며 천천히 접근, 막으면 경직 후 확률적으로 반격
    public sealed class EnemyGuardState : EnemyStateBase
    {
        private readonly ShieldGuard shieldGuard;
        private float remainingTime;
        private float blockStunRemaining;
        private bool isCounterQueued;

        private EnemyGuardSettings Settings => Data.Guard;

        public EnemyGuardState(EnemyController controller) : base(controller)
        {
            shieldGuard = controller.Loadout.ShieldGuard;
        }

        public override void Enter()
        {
            remainingTime = Settings.GuardDuration;
            blockStunRemaining = 0f;
            isCounterQueued = false;

            Navigator.Stop();
            shieldGuard.SetGuarding(true);
            EnemyAnimator.PlayGuard();
        }

        public override void Exit()
        {
            shieldGuard.SetGuarding(false);
            Controller.StartGuardCooldown();
        }

        public override void Tick(float deltaTime)
        {
            if (blockStunRemaining > 0f)
            {
                TickBlockStun(deltaTime);
                return;
            }

            remainingTime -= deltaTime;
            if (remainingTime <= 0f || !Perception.HasTarget)
            {
                Controller.ReturnToCombat();
                return;
            }

            FaceTarget(deltaTime);
            if (Perception.GetDistanceToTarget() > Data.AttackRange)
            {
                Navigator.MoveImmediate(Controller.transform.forward * Settings.GuardMoveSpeed, deltaTime);
            }
        }

        // 가드 성공 시 EnemyController가 호출
        public void OnBlocked(in DamageInfo damageInfo)
        {
            ShieldData shield = Data.Shield;
            blockStunRemaining = shield.BlockStunDuration;
            Navigator.SnapRotation(-damageInfo.Direction);
            Navigator.Knockback(damageInfo.Direction * shield.BlockKnockbackSpeed, Data.HitReaction.KnockbackDeceleration);
            EnemyAnimator.PlayBlockHit();

            isCounterQueued = Settings.CounterAction != null && Random.value < Settings.CounterChance;
        }

        private void TickBlockStun(float deltaTime)
        {
            blockStunRemaining -= deltaTime;
            if (blockStunRemaining > 0f) return;

            if (isCounterQueued)
            {
                Controller.StartCounter(Settings.CounterAction);
                return;
            }

            EnemyAnimator.PlayGuard();
        }
    }
}

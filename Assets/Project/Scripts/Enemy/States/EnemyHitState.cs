using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 피격 경직·넉백, 끝나면 확률적으로 막기(방패) 또는 전투 복귀
    public sealed class EnemyHitState : EnemyStateBase
    {
        private DamageInfo pendingDamage;
        private float elapsedTime;

        private HitReactionData Reaction => Data.HitReaction;

        public EnemyHitState(EnemyController controller) : base(controller) { }

        public void SetDamageInfo(in DamageInfo damageInfo) => pendingDamage = damageInfo;

        public override void Enter()
        {
            elapsedTime = 0f;
            Controller.Attacker.EndSwing();

            Vector3 knockbackDirection = pendingDamage.Direction;
            Navigator.SnapRotation(-knockbackDirection);
            Navigator.Knockback(knockbackDirection * Reaction.KnockbackSpeed, Reaction.KnockbackDeceleration);
            EnemyAnimator.PlayHit();
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (elapsedTime < Reaction.StunDuration) return;

            if (Controller.CanGuard && Random.value < Data.Guard.ReactiveGuardChance)
            {
                Controller.ChangeState(Controller.GuardState);
                return;
            }

            Controller.ReturnToCombat();
        }
    }
}

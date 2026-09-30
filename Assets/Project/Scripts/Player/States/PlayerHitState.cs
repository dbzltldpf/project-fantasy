using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 피격 경직과 넉백
    public sealed class PlayerHitState : PlayerStateBase
    {
        private DamageInfo pendingDamage;
        private float elapsedTime;

        private HitReactionData ReactionData => Controller.HitReactionData;

        public PlayerHitState(PlayerController controller) : base(controller) { }

        public void SetDamageInfo(in DamageInfo damageInfo) => pendingDamage = damageInfo;

        public override void Enter()
        {
            elapsedTime = 0f;
            InputHandler.ClearBuffers();

            Vector3 knockbackDirection = pendingDamage.Direction;
            Motor.SnapRotation(-knockbackDirection);
            Motor.SetVelocityImmediate(knockbackDirection * ReactionData.KnockbackSpeed);
            PlayerAnimator.PlayHit();
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            Motor.SetTargetVelocity(Vector3.zero, ReactionData.KnockbackDeceleration);

            if (elapsedTime >= ReactionData.StunDuration)
            {
                Controller.ChangeState(Controller.LocomotionState);
            }
        }
    }
}

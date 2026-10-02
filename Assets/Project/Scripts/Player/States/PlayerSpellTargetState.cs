using ProjectFantasy.Magic;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 범위 마법 조준 모드: 마법진이 지면을 따라다니고 좌클릭 시 그 위치에 시전
    public sealed class PlayerSpellTargetState : PlayerStateBase
    {
        private Vector3 targetPoint;
        private bool hasTarget;

        public override bool UsesAimView => true;

        public PlayerSpellTargetState(PlayerController controller) : base(controller) { }

        public override void Exit()
        {
            Controller.SpellCaster.HideIndicator();
        }

        public override void Tick(float deltaTime)
        {
            AreaSpellData spell = MagicCaster.CurrentSpell as AreaSpellData;
            if (!InputHandler.IsSecondaryHeld || spell == null)
            {
                Controller.ChangeState(Controller.LocomotionState);
                return;
            }

            if (Motor.IsAirborne)
            {
                Controller.ChangeState(Controller.AirState);
                return;
            }

            UpdateTarget(spell);

            if (hasTarget && MagicCaster.IsReady && InputHandler.ConsumeAttack())
            {
                Controller.CastState.SetTargetPosition(targetPoint);
                Controller.ChangeState(Controller.CastState);
                return;
            }

            ApplyMoveInput(MovementData.AimMoveSpeed, MovementData.Acceleration, MovementData.Deceleration, deltaTime, false);
            Motor.RotateTowards(hasTarget ? targetPoint - Controller.transform.position : Controller.GetCameraFlatForward(), deltaTime);

            Vector3 localVelocity = Controller.transform.InverseTransformDirection(Motor.HorizontalVelocity);
            PlayerAnimator.PlayAimLocomotion(localVelocity, spell.TargetingIdleStateHash, spell.TargetingHoldTime);
        }

        private void UpdateTarget(AreaSpellData spell)
        {
            hasTarget = Controller.SpellCaster.TryGetTargetPoint(Controller.GetAimRay(), spell.MaxRange, out targetPoint);

            if (hasTarget) Controller.SpellCaster.ShowIndicator(spell, targetPoint);
            else Controller.SpellCaster.HideIndicator();
        }
    }
}

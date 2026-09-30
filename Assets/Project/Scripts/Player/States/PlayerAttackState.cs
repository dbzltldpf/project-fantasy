using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 콤보 공격 (히트 윈도우, 선입력 연계, 전진 스텝)
    public sealed class PlayerAttackState : PlayerStateBase
    {
        private const int FirstStepIndex = 0;

        private AttackStep currentStep;
        private Vector3 lungeDirection;
        private int comboIndex;
        private float elapsedTime;

        private AttackComboData ComboData => Controller.AttackComboData;

        public PlayerAttackState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            StartStep(FirstStepIndex);
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;

            Motor.SetVelocityImmediate(currentStep.IsLunging(elapsedTime) ? lungeDirection * currentStep.LungeSpeed : Vector3.zero);

            if (currentStep.IsInHitWindow(elapsedTime))
            {
                Attacker.TickHit();
            }

            if (TryChainCombo()) return;

            if (currentStep.IsFinished(elapsedTime))
            {
                Controller.ChangeState(Controller.LocomotionState);
            }
        }

        public override void Exit()
        {
            Attacker.EndSwing();
        }

        private bool TryChainCombo()
        {
            if (!ComboData.HasNextStep(comboIndex) || !currentStep.CanChainCombo(elapsedTime)) return false;
            if (!InputHandler.ConsumeAttack()) return false;

            StartStep(comboIndex + 1);
            return true;
        }

        private void StartStep(int stepIndex)
        {
            comboIndex = stepIndex;
            currentStep = ComboData.GetStep(stepIndex);
            elapsedTime = 0f;

            // 입력 방향으로 즉시 회전 후 공격
            Motor.SnapRotation(Controller.GetCameraRelativeMove());
            lungeDirection = Controller.transform.forward;

            Attacker.BeginSwing(currentStep.Damage);
            PlayerAnimator.PlayAttack(currentStep.StateHash, ComboData.CrossFadeDuration);
        }
    }
}

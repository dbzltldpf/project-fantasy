using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 콤보 공격 (히트 윈도우, 입력 예약 → 전이 시점 연계, 전진 스텝)
    public sealed class PlayerAttackState : PlayerStateBase
    {
        private const int FirstStepIndex = 0;

        private AttackStep currentStep;
        private Vector3 lungeDirection;
        private int comboIndex;
        private float elapsedTime;
        private bool hasQueuedAttack;

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

            // 원거리/마법 무기는 근접 판정 없음 (발사체는 원거리 전투 기능에서 처리)
            if (Loadout.CurrentWeapon.HasMeleeHit && currentStep.IsInHitWindow(elapsedTime))
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

        // 입력 창에서는 예약만, 전이 시점에 다음 단계 시작
        private bool TryChainCombo()
        {
            if (!ComboData.HasNextStep(comboIndex)) return false;

            if (!hasQueuedAttack && currentStep.CanQueueCombo(elapsedTime))
            {
                hasQueuedAttack = InputHandler.ConsumeAttack();
            }

            if (!hasQueuedAttack || !currentStep.CanTransitionCombo(elapsedTime)) return false;

            StartStep(comboIndex + 1);
            return true;
        }

        private void StartStep(int stepIndex)
        {
            comboIndex = stepIndex;
            currentStep = ComboData.GetStep(stepIndex);
            elapsedTime = 0f;
            hasQueuedAttack = false;

            // 입력 방향으로 즉시 회전 후 공격
            Motor.SnapRotation(Controller.GetCameraRelativeMove());
            lungeDirection = Controller.transform.forward;

            Attacker.BeginSwing(Mathf.RoundToInt(Loadout.CurrentWeapon.AttackPower * currentStep.DamageMultiplier));
            PlayerAnimator.PlayAttack(currentStep.StateHash, ComboData.CrossFadeDuration);
        }
    }
}

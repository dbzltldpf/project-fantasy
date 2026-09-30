using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 플레이어 상태 공통 베이스 (컴포넌트 참조 캐싱)
    public abstract class PlayerStateBase : IState
    {
        protected readonly PlayerController Controller;
        protected readonly PlayerInputHandler InputHandler;
        protected readonly PlayerMotor Motor;
        protected readonly PlayerAnimator PlayerAnimator;
        protected readonly MeleeAttacker Attacker;

        protected PlayerMovementData MovementData => Motor.Data;

        protected PlayerStateBase(PlayerController controller)
        {
            Controller = controller;
            InputHandler = controller.InputHandler;
            Motor = controller.Motor;
            PlayerAnimator = controller.PlayerAnimator;
            Attacker = controller.Attacker;
        }

        public virtual void Enter() { }
        public abstract void Tick(float deltaTime);
        public virtual void Exit() { }

        // 카메라 기준 이동 입력을 모터에 반영, 입력 여부 반환
        protected bool ApplyMoveInput(float accelerationRate, float decelerationRate, float deltaTime)
        {
            Vector3 moveInput = Controller.GetCameraRelativeMove();
            float inputMagnitude = moveInput.magnitude;
            bool hasInput = inputMagnitude > Mathf.Epsilon;

            if (!hasInput)
            {
                Motor.SetTargetVelocity(Vector3.zero, decelerationRate);
                return false;
            }

            Vector3 direction = moveInput / inputMagnitude;
            Motor.SetTargetVelocity(direction * ResolveTargetSpeed(inputMagnitude), accelerationRate);
            Motor.RotateTowards(direction, deltaTime);
            return true;
        }

        // 기본 걷기, 달리기 입력 시 달리기 (스틱 기울기에 비례)
        private float ResolveTargetSpeed(float inputMagnitude)
        {
            float baseSpeed = InputHandler.IsSprintHeld ? MovementData.RunSpeed : MovementData.WalkSpeed;
            return baseSpeed * inputMagnitude;
        }
    }
}

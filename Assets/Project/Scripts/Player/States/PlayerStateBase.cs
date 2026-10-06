using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 플레이어 상태 공통 베이스 (컴포넌트 참조 캐싱, 공통 입력 분기)
    public abstract class PlayerStateBase : IState
    {
        protected readonly PlayerController Controller;
        protected readonly PlayerInputHandler InputHandler;
        protected readonly PlayerMotor Motor;
        protected readonly PlayerAnimator PlayerAnimator;
        protected readonly MeleeAttacker Attacker;
        protected readonly PlayerLoadout Loadout;
        protected readonly PlayerRangedWeapon RangedWeapon;
        protected readonly PlayerMagicCaster MagicCaster;
        protected readonly PlayerItemHandler ItemHandler;

        protected PlayerMovementData MovementData => Motor.Data;

        // 숄더뷰/조준점 사용 여부 (상태 전이 시 PlayerController가 반영)
        public virtual bool UsesAimView => false;

        protected PlayerStateBase(PlayerController controller)
        {
            Controller = controller;
            InputHandler = controller.InputHandler;
            Motor = controller.Motor;
            PlayerAnimator = controller.PlayerAnimator;
            Attacker = controller.Attacker;
            Loadout = controller.Loadout;
            RangedWeapon = controller.RangedWeapon;
            MagicCaster = controller.MagicCaster;
            ItemHandler = controller.ItemHandler;
        }

        public virtual void Enter() { }
        public abstract void Tick(float deltaTime);
        public virtual void Exit() { }

        // 좌클릭: 원거리 무기는 발사, 마법 무기는 시전(범위 마법은 조준 모드에서만), 그 외는 근접 콤보
        protected bool TryStartPrimaryAction()
        {
            if (RangedWeapon.IsEquipped)
            {
                if (!RangedWeapon.CanFire || !InputHandler.ConsumeAttack()) return false;

                Controller.ChangeState(Controller.RangedFireState);
                return true;
            }

            if (MagicCaster.IsEquipped)
            {
                if (MagicCaster.RequiresTargeting || !MagicCaster.IsReady || !InputHandler.ConsumeAttack()) return false;

                Controller.ChangeState(Controller.CastState);
                return true;
            }

            if (!Controller.CanAttack || !InputHandler.ConsumeAttack()) return false;

            Controller.ChangeState(Controller.AttackState);
            return true;
        }

        // 우클릭 홀드: 원거리 조준 / 범위 마법 위치 지정 / 방패 가드
        protected bool TryStartSecondaryAction()
        {
            if (!InputHandler.IsSecondaryHeld) return false;

            if (RangedWeapon.CanAim)
            {
                Controller.ChangeState(Controller.AimState);
                return true;
            }

            if (MagicCaster.RequiresTargeting)
            {
                Controller.ChangeState(Controller.SpellTargetState);
                return true;
            }

            if (!Loadout.CanGuard) return false;

            Controller.ChangeState(Controller.GuardState);
            return true;
        }

        protected void FaceAim(float deltaTime) => Motor.RotateTowards(Controller.GetAimFacingDirection(), deltaTime);

        // 기본 걷기, 달리기 입력 시 달리기
        protected bool ApplyMoveInput(float accelerationRate, float decelerationRate, float deltaTime)
        {
            float baseSpeed = InputHandler.IsSprintHeld ? MovementData.RunSpeed : MovementData.WalkSpeed;
            return ApplyMoveInput(baseSpeed, accelerationRate, decelerationRate, deltaTime);
        }

        // 카메라 기준 이동 입력을 모터에 반영 (속도는 스틱 기울기에 비례), 입력 여부 반환
        protected bool ApplyMoveInput(float baseSpeed, float accelerationRate, float decelerationRate, float deltaTime, bool faceMoveDirection = true)
        {
            Vector3 moveInput = Controller.GetCameraRelativeMove();
            float inputMagnitude = moveInput.magnitude;

            if (inputMagnitude <= Mathf.Epsilon)
            {
                Motor.SetTargetVelocity(Vector3.zero, decelerationRate);
                return false;
            }

            Vector3 direction = moveInput / inputMagnitude;
            Motor.SetTargetVelocity(direction * (baseSpeed * inputMagnitude), accelerationRate);
            if (faceMoveDirection) Motor.RotateTowards(direction, deltaTime);
            return true;
        }
    }
}

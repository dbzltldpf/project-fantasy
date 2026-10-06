using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // 줍기 (대상을 바라보고 모션, 획득 시점에 가방에 넣음, 그 전에 피격되면 줍지 않음)
    public sealed class PlayerPickUpState : PlayerStateBase
    {
        private WorldItem target;
        private float elapsedTime;
        private bool isGrabbed;

        public PlayerPickUpState(PlayerController controller) : base(controller) { }

        public void SetTarget(WorldItem worldItem) => target = worldItem;

        public override void Enter()
        {
            elapsedTime = 0f;
            isGrabbed = false;

            if (target != null) Motor.SnapRotation(target.transform.position - Controller.transform.position);
            PlayerAnimator.PlayPickUp();
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            Motor.SetTargetVelocity(Vector3.zero, MovementData.Deceleration);

            if (!isGrabbed && elapsedTime >= ItemHandler.PickUpGrabTime)
            {
                isGrabbed = true;
                ItemHandler.CompletePickUp(target);
            }

            if (elapsedTime >= ItemHandler.PickUpDuration) Controller.ChangeState(Controller.LocomotionState);
        }

        public override void Exit() => target = null;
    }
}

using ProjectFantasy.Items;

namespace ProjectFantasy.Player
{
    // 소모품 사용 (느린 이동 가능, 효과 시점에 1개 소모 + 적용, 그 전에 피격되면 소모 안 됨)
    public sealed class PlayerUseItemState : PlayerStateBase
    {
        private const int SingleUse = 1;

        private ConsumableData item;
        private float elapsedTime;
        private bool isApplied;

        public PlayerUseItemState(PlayerController controller) : base(controller) { }

        public void SetItem(ConsumableData consumable) => item = consumable;

        public override void Enter()
        {
            elapsedTime = 0f;
            isApplied = false;
            PlayerAnimator.PlayAction(item.UseStateHash);
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            ApplyMoveInput(item.MoveSpeed, MovementData.Acceleration, MovementData.Deceleration, deltaTime);

            if (!isApplied && elapsedTime >= item.EffectTime)
            {
                isApplied = true;
                if (ItemHandler.Inventory.TryConsume(item, SingleUse)) item.ApplyEffects(Controller.gameObject);
            }

            if (elapsedTime >= item.Duration) Controller.ChangeState(Controller.LocomotionState);
        }
    }
}

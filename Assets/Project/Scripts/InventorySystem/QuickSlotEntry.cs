using ProjectFantasy.Items;

namespace ProjectFantasy.InventorySystem
{
    // 퀵슬롯 한 칸: 장비는 특정 개체, 소모품은 종류 (같은 종류면 남은 수량 전체 사용)
    public readonly struct QuickSlotEntry
    {
        public static readonly QuickSlotEntry Empty = default;

        public readonly ItemData Item;
        public readonly ItemInstance Instance;

        public bool IsEmpty => Item == null;

        private QuickSlotEntry(ItemData item, ItemInstance instance)
        {
            Item = item;
            Instance = instance;
        }

        public static QuickSlotEntry From(in ItemStack stack)
        {
            return stack.IsEmpty || !stack.Item.CanQuickSlot ? Empty : new QuickSlotEntry(stack.Item, stack.Instance);
        }

        public static QuickSlotEntry FromInstance(ItemInstance instance) => instance != null ? new QuickSlotEntry(instance.Data, instance) : Empty;
        public static QuickSlotEntry FromStackable(ItemData item) => item != null ? new QuickSlotEntry(item, null) : Empty;

        // 개체는 같은 개체, 소모품은 같은 종류
        public bool Matches(in QuickSlotEntry other)
        {
            if (IsEmpty || other.IsEmpty) return false;
            return Instance != null ? Instance == other.Instance : other.Instance == null && Item == other.Item;
        }
    }
}

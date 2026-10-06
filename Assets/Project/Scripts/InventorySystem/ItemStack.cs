using ProjectFantasy.Items;

namespace ProjectFantasy.InventorySystem
{
    // 가방 한 칸: 겹치는 아이템은 종류 + 수량, 겹치지 않는 아이템은 개체 1개
    public readonly struct ItemStack
    {
        private const int EmptyCount = 0;
        private const int SingleCount = 1;

        public static readonly ItemStack Empty = new ItemStack(null, EmptyCount);

        public readonly ItemData Item;
        public readonly int Count;
        public readonly ItemInstance Instance;

        public bool IsEmpty => Item == null || Count <= EmptyCount;
        public int SpaceLeft => IsEmpty ? EmptyCount : Item.MaxStack - Count;

        public ItemStack(ItemData item, int count)
        {
            Item = item;
            Count = count;
            Instance = null;
        }

        public ItemStack(ItemInstance instance)
        {
            Item = instance.Data;
            Count = SingleCount;
            Instance = instance;
        }

        public ItemStack WithCount(int count) => count > EmptyCount ? new ItemStack(Item, count) : Empty;
    }
}

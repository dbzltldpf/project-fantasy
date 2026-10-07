namespace ProjectFantasy.Items
{
    // 드랍 결과 하나 (겹치는 아이템은 종류+수량, 장비는 개체)
    public readonly struct LootDrop
    {
        private const int SingleCount = 1;

        public readonly ItemData Item;
        public readonly int Count;
        public readonly ItemInstance Instance;

        public LootDrop(ItemData item, int count)
        {
            Item = item;
            Count = count;
            Instance = null;
        }

        public LootDrop(ItemInstance instance)
        {
            Item = instance.Data;
            Count = SingleCount;
            Instance = instance;
        }
    }
}

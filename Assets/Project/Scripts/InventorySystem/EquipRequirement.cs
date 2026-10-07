namespace ProjectFantasy.InventorySystem
{
    // 장비 장착 요구 숙련 (문구는 UI가 결정)
    public readonly struct EquipRequirement
    {
        public readonly string MasteryName;
        public readonly int RequiredLevel;
        public readonly int Tier;
        public readonly bool IsMet;

        public EquipRequirement(string masteryName, int requiredLevel, int tier, bool isMet)
        {
            MasteryName = masteryName;
            RequiredLevel = requiredLevel;
            Tier = tier;
            IsMet = isMet;
        }
    }
}

namespace ProjectFantasy.Items
{
    // 겹치지 않는 아이템(장비) 하나하나의 런타임 개체: 티어·등급·굴린 능력치 (내구도 등 개별 상태 확장 지점)
    public sealed class ItemInstance
    {
        public ItemData Data { get; }
        public int Tier { get; }
        public ItemGrade Grade { get; }
        public ItemStats Stats { get; }
        // 티어 접두어 포함 이름 (예: 강철 한손검)
        public string DisplayName { get; }

        public ItemInstance(ItemData data, int tier, ItemTier tierInfo, ItemGrade grade, ItemStats stats)
        {
            Data = data;
            Tier = tier;
            Grade = grade;
            Stats = stats;
            DisplayName = string.IsNullOrEmpty(tierInfo?.Prefix) ? data.DisplayName : $"{tierInfo.Prefix} {data.DisplayName}";
        }
    }
}

namespace ProjectFantasy.Items
{
    // 겹치지 않는 아이템(장비) 하나하나의 런타임 개체: 등급·굴린 능력치 (내구도 등 개별 상태 확장 지점)
    public sealed class ItemInstance
    {
        public ItemData Data { get; }
        public ItemGrade Grade { get; }
        public ItemStats Stats { get; }

        public ItemInstance(ItemData data, ItemGrade grade, ItemStats stats)
        {
            Data = data;
            Grade = grade;
            Stats = stats;
        }
    }
}

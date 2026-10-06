namespace ProjectFantasy.Items
{
    // 장비 개체의 굴린 능력치 (값 타입, 없는 능력치는 0)
    public readonly struct ItemStats
    {
        private const int NoValue = 0;

        public static readonly ItemStats Zero = new ItemStats(NoValue, NoValue, NoValue, NoValue);

        public readonly int AttackPower;
        public readonly int MagicPower;
        public readonly int Defense;
        public readonly int MagicDefense;

        public ItemStats(int attackPower, int magicPower, int defense, int magicDefense)
        {
            AttackPower = attackPower;
            MagicPower = magicPower;
            Defense = defense;
            MagicDefense = magicDefense;
        }

        public static ItemStats Attack(int attackPower) => new ItemStats(attackPower, NoValue, NoValue, NoValue);
        public static ItemStats Magic(int magicPower) => new ItemStats(NoValue, magicPower, NoValue, NoValue);
        public static ItemStats Armor(int defense, int magicDefense) => new ItemStats(NoValue, NoValue, defense, magicDefense);

        public int Get(StatType stat)
        {
            switch (stat)
            {
                case StatType.AttackPower: return AttackPower;
                case StatType.MagicPower: return MagicPower;
                case StatType.Defense: return Defense;
                case StatType.MagicDefense: return MagicDefense;
                default: return NoValue;
            }
        }
    }
}

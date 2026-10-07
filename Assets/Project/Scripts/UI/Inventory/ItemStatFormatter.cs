using System;
using System.Text;
using ProjectFantasy.InventorySystem;
using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // 장비 능력치 + 장착 장비 대비 차이 문구 (리치 텍스트, StringBuilder 재사용)
    [Serializable]
    public sealed class ItemStatFormatter
    {
        private const int NoDifference = 0;

        [SerializeField] private string attackPowerLabel = "공격력";
        [SerializeField] private string magicPowerLabel = "마법력";
        [SerializeField] private string defenseLabel = "방어력";
        [SerializeField] private string magicDefenseLabel = "마법 방어력";
        [SerializeField] private Color increaseColor = new Color(0.4f, 0.9f, 0.4f);
        [SerializeField] private Color decreaseColor = new Color(1f, 0.4f, 0.4f);
        [Tooltip("{0} = 숙련 이름, {1} = 필요 레벨, {2} = 티어")]
        [SerializeField] private string requirementFormat = "요구 숙련: {0} Lv {1} (T{2})";
        [Tooltip("숙련도가 부족할 때 요구 숙련 문구 색")]
        [SerializeField] private Color unmetRequirementColor = new Color(1f, 0.35f, 0.35f);

        private static readonly StatType[] StatOrder = (StatType[])Enum.GetValues(typeof(StatType));
        private readonly StringBuilder builder = new StringBuilder();
        private string increaseHex;
        private string decreaseHex;

        // compareTo가 null이면 차이 없이 값만 표시
        public string Format(ItemInstance instance, ItemStats? compareTo)
        {
            increaseHex ??= ColorUtility.ToHtmlStringRGB(increaseColor);
            decreaseHex ??= ColorUtility.ToHtmlStringRGB(decreaseColor);
            builder.Clear();

            foreach (StatType stat in StatOrder)
            {
                if (!instance.Data.HasStat(stat)) continue;

                int value = instance.Stats.Get(stat);
                builder.Append(GetLabel(stat)).Append(' ').Append(value);
                if (compareTo.HasValue) AppendDifference(value - compareTo.Value.Get(stat));
                builder.AppendLine();
            }
            return builder.ToString();
        }

        // 부족하면 빨간색
        public string FormatRequirement(in EquipRequirement requirement)
        {
            string text = string.Format(requirementFormat, requirement.MasteryName, requirement.RequiredLevel, requirement.Tier);
            return requirement.IsMet ? text : ToColorTag(text, unmetRequirementColor);
        }

        public static string ToColorTag(string text, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";

        private void AppendDifference(int difference)
        {
            if (difference == NoDifference) return;

            bool isIncrease = difference > NoDifference;
            builder.Append("  <color=#").Append(isIncrease ? increaseHex : decreaseHex).Append('>')
                .Append(isIncrease ? "▲+" : "▼").Append(difference).Append("</color>");
        }

        private string GetLabel(StatType stat)
        {
            switch (stat)
            {
                case StatType.AttackPower: return attackPowerLabel;
                case StatType.MagicPower: return magicPowerLabel;
                case StatType.Defense: return defenseLabel;
                case StatType.MagicDefense: return magicDefenseLabel;
                default: return stat.ToString();
            }
        }
    }
}

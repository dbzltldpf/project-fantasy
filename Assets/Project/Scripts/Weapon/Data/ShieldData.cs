using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 방패 가드 범위·막기 반응·방어력 (보조 손이 비어 있고 방패를 허용하는 무기와 장착)
    [CreateAssetMenu(fileName = "ShieldData", menuName = "ProjectFantasy/Weapon/Shield Data")]
    public sealed class ShieldData : OffHandData
    {
        private const int DefaultDefense = 10;

        [Header("Guard")]
        [Tooltip("가드로 막는 정면 각도 (전체 각도)")]
        [SerializeField, Range(0f, 360f)] private float guardAngle = 120f;
        [Tooltip("막았을 때 밀리는 속도 (m/s)")]
        [SerializeField, Min(0f)] private float blockKnockbackSpeed = 2f;
        [Tooltip("막았을 때 경직 (초)")]
        [SerializeField, Min(0f)] private float blockStunDuration = 0.3f;

        [Header("Defense")]
        [Tooltip("물리 피해 감소: 피해 × 100 / (100 + 방어력)")]
        [SerializeField] private StatRange defenseRange = new StatRange(DefaultDefense, DefaultDefense);
        [Tooltip("마법 피해 감소: 피해 × 100 / (100 + 마법 방어력)")]
        [SerializeField] private StatRange magicDefenseRange = new StatRange(DefaultDefense, DefaultDefense);

        public float GuardAngle => guardAngle;
        public float BlockKnockbackSpeed => blockKnockbackSpeed;
        public float BlockStunDuration => blockStunDuration;

        public override ItemStats BaseStats => ItemStats.Armor(defenseRange.Min, magicDefenseRange.Min);
        public override bool HasStat(StatType stat) => stat == StatType.Defense || stat == StatType.MagicDefense;

        public override bool CanEquipWith(WeaponData weapon) => !weapon.OccupiesOffHand && weapon.AllowsShield;

        protected override ItemStats RollStats(float multiplier)
        {
            return ItemStats.Armor(defenseRange.Roll(multiplier), magicDefenseRange.Roll(multiplier));
        }
    }
}

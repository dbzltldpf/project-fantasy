using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 마법서 (한손 마법 무기와만 장착, 마법 위력 보정)
    [CreateAssetMenu(fileName = "SpellbookData", menuName = "ProjectFantasy/Weapon/Spellbook Data")]
    public sealed class SpellbookData : OffHandData
    {
        private const float NoGradeMultiplier = 1f;

        [Header("Magic")]
        [Tooltip("마법 피해 배율 (Wand·Staff와 함께 활성일 때)")]
        [SerializeField, Min(0f)] private float magicPowerMultiplier = 1.2f;

        public float MagicPowerMultiplier => magicPowerMultiplier;

        // 보너스 부분(배율 - 1)에만 개체 등급 배율 적용 (예: 1.2, 전설 ×1.5 → 1.3)
        public float GetMultiplier(ItemInstance instance)
        {
            float gradeMultiplier = instance?.Grade != null ? instance.Grade.StatMultiplier : NoGradeMultiplier;
            return NoGradeMultiplier + (magicPowerMultiplier - NoGradeMultiplier) * gradeMultiplier;
        }

        public override bool CanEquipWith(WeaponData weapon) => weapon.IsMagic && !weapon.OccupiesOffHand;
    }
}

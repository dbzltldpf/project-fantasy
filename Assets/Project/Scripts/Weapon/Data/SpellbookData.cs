using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 마법서 (한손 마법 무기와만 장착, 마법 위력 보정)
    [CreateAssetMenu(fileName = "SpellbookData", menuName = "ProjectFantasy/Weapon/Spellbook Data")]
    public sealed class SpellbookData : OffHandData
    {
        [Header("Magic")]
        [Tooltip("마법 피해 배율 (Wand·Staff와 함께 활성일 때)")]
        [SerializeField, Min(0f)] private float magicPowerMultiplier = 1.2f;

        public float MagicPowerMultiplier => magicPowerMultiplier;

        public override bool CanEquipWith(WeaponData weapon) => weapon.IsMagic && !weapon.OccupiesOffHand;
    }
}

using ProjectFantasy.Items;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Magic
{
    // 마법 무기 (Wand/Staff): 기본 마법, 발사 지점, 마법력 (공격력 대신 마법력으로 피해 계산)
    [CreateAssetMenu(fileName = "MagicWeaponData", menuName = "ProjectFantasy/Magic/Magic Weapon Data")]
    public sealed class MagicWeaponData : WeaponData
    {
        private const int DefaultMagicPower = 10;

        [Tooltip("좌클릭으로 시전하는 기본 마법 (직선 마법탄 / 지면 범위)")]
        [SerializeField] private SpellData spell;
        [Tooltip("캐릭터 로컬 기준 마법탄 발사 지점")]
        [SerializeField] private Vector3 muzzleOffset = new Vector3(0.3f, 1.4f, 0.6f);
        [Tooltip("개체 생성 시 굴리는 마법력 범위 (마법 피해 = 마법력 × 마법 배율 × 마법서 배율)")]
        [SerializeField] private StatRange magicPowerRange = new StatRange(DefaultMagicPower, DefaultMagicPower);

        public SpellData Spell => spell;
        public Vector3 MuzzleOffset => muzzleOffset;

        public override ItemStats BaseStats => ItemStats.Magic(magicPowerRange.Min);
        public override bool HasStat(StatType stat) => stat == StatType.MagicPower;

        protected override ItemStats RollStats(float multiplier) => ItemStats.Magic(magicPowerRange.Roll(multiplier));
    }
}

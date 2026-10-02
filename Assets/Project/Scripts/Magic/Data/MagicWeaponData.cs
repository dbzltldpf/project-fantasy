using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Magic
{
    // 마법 무기 (Wand/Staff): 기본 마법과 발사 지점
    [CreateAssetMenu(fileName = "MagicWeaponData", menuName = "ProjectFantasy/Magic/Magic Weapon Data")]
    public sealed class MagicWeaponData : WeaponData
    {
        [Header("Magic")]
        [SerializeField] private SpellData spell;
        [Tooltip("캐릭터 로컬 기준 마법탄 발사 지점")]
        [SerializeField] private Vector3 muzzleOffset = new Vector3(0.3f, 1.4f, 0.6f);

        public SpellData Spell => spell;
        public Vector3 MuzzleOffset => muzzleOffset;
    }
}

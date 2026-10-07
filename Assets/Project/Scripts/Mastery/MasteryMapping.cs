using ProjectFantasy.Items;
using ProjectFantasy.Weapon;

namespace ProjectFantasy.Mastery
{
    // 장비 → 숙련도 종류 (숙련도가 없는 아이템은 false)
    public static class MasteryMapping
    {
        public static bool TryGet(ItemData item, out MasteryType type)
        {
            switch (item)
            {
                case WeaponData weapon:
                    type = FromWeapon(weapon);
                    return true;
                case ShieldData _:
                    type = MasteryType.Shield;
                    return true;
                case SpellbookData _:
                    type = MasteryType.Spellbook;
                    return true;
                default:
                    type = default;
                    return false;
            }
        }

        private static MasteryType FromWeapon(WeaponData weapon)
        {
            switch (weapon.WeaponType)
            {
                case WeaponType.OneHanded: return MasteryType.OneHanded;
                case WeaponType.TwoHanded: return MasteryType.TwoHanded;
                case WeaponType.Bow: return MasteryType.Bow;
                case WeaponType.Crossbow: return weapon.OccupiesOffHand ? MasteryType.Crossbow2H : MasteryType.Crossbow1H;
                case WeaponType.Wand: return MasteryType.Wand;
                case WeaponType.Staff: return MasteryType.Staff;
                default: return MasteryType.Unarmed;
            }
        }
    }
}

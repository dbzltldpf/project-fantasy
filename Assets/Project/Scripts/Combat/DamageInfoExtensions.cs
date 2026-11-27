using ProjectFantasy.Core;
using ProjectFantasy.Weapon;
namespace ProjectFantasy.Combat
{
    // DamageInfo 가해자 무기 종류 확인 (가드 파괴·소리 공용)
    public static class DamageInfoExtensions
    {
        public static bool IsFromWeapon(this in DamageInfo damageInfo, WeaponType weaponType) =>
               damageInfo.Instigator != null
               && damageInfo.Instigator.TryGetComponent(out IWeaponWielder wielder)
               && wielder.CurrentWeaponType == weaponType;
    }

}

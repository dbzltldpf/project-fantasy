namespace ProjectFantasy.Weapon
{
    // 현재 든 무기 종류를 알려 주는 쪽 (피해 무기 제한 판정용)
    public interface IWeaponWielder
    {
        WeaponType CurrentWeaponType { get; }
    }

}

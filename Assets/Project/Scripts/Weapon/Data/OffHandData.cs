namespace ProjectFantasy.Weapon
{
    // 보조 손(왼손) 장비 공통 베이스
    public abstract class OffHandData : EquipmentData
    {
        // 현재 무기와 함께 장착 가능한지
        public abstract bool CanEquipWith(WeaponData weapon);
    }
}

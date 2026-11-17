namespace ProjectFantasy.Core
{
    // 피해 판정 전에 받을지 결정 (false면 무시: 피격 이벤트·데미지 숫자 없음)
    public interface IDamageFilter
    {
        bool CanReceive(in DamageInfo damageInfo);
    }
}
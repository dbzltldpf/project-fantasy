namespace ProjectFantasy.Core
{
    // 피해 적용 전 방어 여부 판정 (방패 가드 등)
    public interface IDamageBlocker
    {
        bool TryBlock(in DamageInfo damageInfo);
    }
}

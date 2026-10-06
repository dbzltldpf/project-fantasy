namespace ProjectFantasy.Core
{
    // 피해 적용 직전 피해량 감소 (방어력 등), 감소된 피해량 반환
    public interface IDamageReducer
    {
        int Reduce(in DamageInfo damageInfo);
    }
}

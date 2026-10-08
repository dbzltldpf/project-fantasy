namespace ProjectFantasy.Core
{
    // 피해를 받을 수 있는 모든 대상
    public interface IDamageable
    {
        bool IsAlive { get; }
        DamageResult TakeDamage(in DamageInfo damageInfo);
    }
}

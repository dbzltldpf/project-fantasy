namespace ProjectFantasy.Core
{
    // 피해 요청 결과 (투사체가 박힐지·튕길지 등 공격 측 반응 결정용)
    public enum DamageResult
    {
        // 체력 감소 적용
        Applied,
        // 가드로 막힘
        Blocked,
        // 사망·무적·0 피해로 무시
        Ignored
    }
}

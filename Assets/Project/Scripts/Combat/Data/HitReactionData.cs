using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 피격 경직/넉백 데이터 (플레이어/적 공용)
    [CreateAssetMenu(fileName = "HitReactionData", menuName = "ProjectFantasy/Combat/Hit Reaction Data")]
    public sealed class HitReactionData : ScriptableObject
    {
        [SerializeField, Min(0f)] private float stunDuration = 0.4f;
        [SerializeField, Min(0f)] private float knockbackSpeed = 4f;
        [SerializeField, Min(0f)] private float knockbackDeceleration = 12f;

        public float StunDuration => stunDuration;
        public float KnockbackSpeed => knockbackSpeed;
        public float KnockbackDeceleration => knockbackDeceleration;
    }
}

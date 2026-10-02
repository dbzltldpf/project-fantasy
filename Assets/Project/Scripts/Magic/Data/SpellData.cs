using UnityEngine;

namespace ProjectFantasy.Magic
{
    // 마법 공통: 시전 모션·시점·길이, 쿨타임, 데미지 배율 (시간 단위: 초)
    public abstract class SpellData : ScriptableObject
    {
        [SerializeField] private string displayName;

        [Header("Cast")]
        [SerializeField] private string castStateName = "Ranged_Magic_Shoot";
        [Tooltip("시전 모션 시작 후 마법이 발동되는 시점")]
        [SerializeField, Min(0f)] private float releaseTime = 0.3f;
        [SerializeField, Min(0f)] private float castDuration = 0.8f;
        [SerializeField, Min(0f)] private float cooldown = 1f;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float damageMultiplier = 1f;

        private int castStateHash;

        public string DisplayName => displayName;
        public string CastStateName => castStateName;
        public int CastStateHash => castStateHash;
        public float ReleaseTime => releaseTime;
        public float CastDuration => castDuration;
        public float Cooldown => cooldown;
        public float DamageMultiplier => damageMultiplier;

        // 지면 위치 지정(조준 모드)이 필요한 마법인지
        public abstract bool RequiresTargeting { get; }

        protected virtual void OnEnable() => CacheStateHashes();
        protected virtual void OnValidate() => CacheStateHashes();

        protected virtual void CacheStateHashes()
        {
            castStateHash = Animator.StringToHash(castStateName);
        }
    }
}

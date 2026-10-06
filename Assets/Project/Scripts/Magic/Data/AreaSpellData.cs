using UnityEngine;

namespace ProjectFantasy.Magic
{
    // 지면 범위 마법: 조준 모드에서 마법진으로 위치 지정 → 시전 → 지연 후 범위 피해 - Staff 범위 공격
    [CreateAssetMenu(fileName = "AreaSpell", menuName = "ProjectFantasy/Magic/Area Spell")]
    public sealed class AreaSpellData : SpellData
    {
        [Header("Targeting")]
        [Tooltip("마법진 최대 사거리 (m)")]
        [SerializeField, Min(0f)] private float maxRange = 15f;
        [Tooltip("마법진 조준 중 자세 상태")]
        [SerializeField] private string targetingIdleStateName = "Ranged_Magic_Raise";
        [Tooltip("조준 대기 자세를 멈출 정규화 시간 (1이면 고정 안 함)")]
        [SerializeField, Range(0f, 1f)] private float targetingHoldTime = 1f;
        [Tooltip("위치 지정·발동 대기 중 표시할 마법진 (Looping 켠 상태)")]
        [SerializeField] private GameObject indicatorPrefab;
        [Tooltip("마법진 프리팹 크기 배율 (반경에 맞게 조정)")]
        [SerializeField, Min(0f)] private float indicatorScale = 1f;

        [Header("Area")]
        [Tooltip("범위 반지름 (m)")]
        [SerializeField, Min(0f)] private float radius = 3f;
        [Tooltip("시전 후 범위 피해까지 지연")]
        [SerializeField, Min(0f)] private float activationDelay = 0.6f;
        [Tooltip("발동 시 1회 재생 이펙트 (Looping 끈 Prefab Variant)")]
        [SerializeField] private GameObject areaEffectPrefab;

        private int targetingIdleStateHash;

        public override bool RequiresTargeting => true;

        public float MaxRange => maxRange;
        public string TargetingIdleStateName => targetingIdleStateName;
        public int TargetingIdleStateHash => targetingIdleStateHash;
        public float TargetingHoldTime => targetingHoldTime;
        public GameObject IndicatorPrefab => indicatorPrefab;
        public float IndicatorScale => indicatorScale;
        public float Radius => radius;
        public float ActivationDelay => activationDelay;
        public GameObject AreaEffectPrefab => areaEffectPrefab;

        protected override void CacheStateHashes()
        {
            base.CacheStateHashes();
            targetingIdleStateHash = Animator.StringToHash(targetingIdleStateName);
        }
    }
}

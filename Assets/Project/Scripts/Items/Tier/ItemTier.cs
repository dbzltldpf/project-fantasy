using System;
using UnityEngine;

namespace ProjectFantasy.Items
{
    // 티어 한 단계 (이름 접두어, 능력치 배율, 모델 아우라, 선택 색조)
    [Serializable]
    public sealed class ItemTier
    {
        private const float DefaultMetallic = 0f;
        private const float DefaultSmoothness = 0.5f;

        [Tooltip("이름 앞에 붙는 접두어 (예: 강철 → 강철 한손검)")]
        [SerializeField] private string prefix;
        [Tooltip("T1 기준 능력치 범위에 곱하는 배율")]
        [SerializeField, Min(0f)] private float statMultiplier = 1f;

        [Header("Aura (무기 주변 파티클)")]
        [Tooltip("티어표의 아우라 프리팹을 이 색으로 표시 (보통 T2부터)")]
        [SerializeField] private bool useAura;
        [Tooltip("입자 색 (HDR, 강도가 높을수록 Bloom에서 밝게 번짐)")]
        [SerializeField, ColorUsage(true, true)] private Color auraColor = Color.white;
        [Tooltip("초당 입자 수 (클수록 진함)")]
        [SerializeField, Min(0f)] private float auraRate;

        [Header("Outline (무기 테두리 발광)")]
        [Tooltip("티어표의 아웃라인 머티리얼로 무기 실루엣을 감싸는 발광 테두리 표시")]
        [SerializeField] private bool useOutline;
        [Tooltip("테두리 색 (HDR, 강도가 높을수록 Bloom에서 밝게 번짐)")]
        [SerializeField, ColorUsage(false, true)] private Color outlineColor = Color.white;
        [Tooltip("테두리 두께 (m, 0.004~0.012 권장, 두꺼우면 각진 모서리에서 끊김이 보임)")]
        [SerializeField, Min(0f)] private float outlineWidth;

        [Header("Model Tint (선택)")]
        [Tooltip("켜면 원본 텍스처에 색조를 곱함 (손잡이까지 물들므로 은은하게)")]
        [SerializeField] private bool applyTint;
        [Tooltip("원본 텍스처에 곱하는 색 (Base Color)")]
        [SerializeField] private Color baseColor = Color.white;
        [SerializeField, Range(0f, 1f)] private float metallic = DefaultMetallic;
        [SerializeField, Range(0f, 1f)] private float smoothness = DefaultSmoothness;
        [Tooltip("발광 사용 (빛 번짐은 URP Volume의 Bloom 필요)")]
        [SerializeField] private bool useEmission;
        [SerializeField, ColorUsage(false, true)] private Color emissionColor = Color.black;

        public string Prefix => prefix;
        public float StatMultiplier => statMultiplier;
        public bool UseAura => useAura;
        public Color AuraColor => auraColor;
        public float AuraRate => auraRate;
        public bool UseOutline => useOutline;
        public Color OutlineColor => outlineColor;
        public float OutlineWidth => outlineWidth;
        public bool ApplyTint => applyTint;
        public Color BaseColor => baseColor;
        public float Metallic => metallic;
        public float Smoothness => smoothness;
        public bool UseEmission => useEmission;
        public Color EmissionColor => emissionColor;

        // 외형 변화 없는 티어
        public ItemTier(string prefix, float statMultiplier)
        {
            this.prefix = prefix;
            this.statMultiplier = statMultiplier;
        }

        // 불씨 아우라 + 테두리 발광 티어
        public ItemTier(string prefix, float statMultiplier, Color auraColor, float auraRate, Color outlineColor, float outlineWidth)
            : this(prefix, statMultiplier)
        {
            useAura = true;
            this.auraColor = auraColor;
            this.auraRate = auraRate;
            useOutline = true;
            this.outlineColor = outlineColor;
            this.outlineWidth = outlineWidth;
        }
    }
}

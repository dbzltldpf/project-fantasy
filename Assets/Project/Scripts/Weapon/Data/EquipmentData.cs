using System;
using ProjectFantasy.Items;
using ProjectFantasy.Utils;
using UnityEngine;

namespace ProjectFantasy.Weapon
{
    // 손에 드는 장비 공통 데이터 (모델, 손 위치 보정, 티어·등급표, 티어 외형), 필드 모델 기본값은 장비 모델
    public abstract class EquipmentData : ItemData
    {
        private const float NoMultiplier = 1f;

        [Tooltip("손에 쥐는 모델 (맨손은 비움)")]
        [SerializeField] private GameObject modelPrefab;
        [Tooltip("손 소켓 기준 위치 보정 (KayKit 무기는 0 유지)")]
        [SerializeField] private Vector3 gripPosition;
        [Tooltip("손 소켓 기준 회전 보정 (KayKit 무기는 0 유지)")]
        [SerializeField] private Vector3 gripRotation;
        [Tooltip("(선택) 티어별 전용 머티리얼, 첫 줄 = T0. 지정한 티어만 티어표 자동 색조 대신 사용 (칼날만 바꾼 텍스처 등)")]
        [SerializeField] private Material[] tierMaterials = Array.Empty<Material>();
        [Tooltip("티어 접두어·능력치 배율·모델 색조 표 (비우면 모든 개체 T0)")]
        [SerializeField] private ItemTierTable tierTable;
        [Tooltip("등급 추첨 표 (비우면 등급 없이 범위 값만 굴림)")]
        [SerializeField] private ItemGradeTable gradeTable;

        public GameObject ModelPrefab => modelPrefab;
        public Vector3 GripPosition => gripPosition;
        public Quaternion GripRotation => Quaternion.Euler(gripRotation);

        public override GameObject WorldModel => base.WorldModel != null ? base.WorldModel : modelPrefab;

        // 머티리얼: 전용 머티리얼 > 티어표 색조(선택) > 원본, 그 위에 불씨 아우라 + 테두리 발광
        public override void ApplyModelVisual(GameObject model, int tier)
        {
            ItemTier tierInfo = tierTable != null ? tierTable.Get(tier) : null;

            int index = tier - MinTier;
            Material overrideMaterial = index >= 0 && index < tierMaterials.Length ? tierMaterials[index] : null;
            if (overrideMaterial != null) model.ApplySharedMaterial(overrideMaterial);
            else TierTintCache.Apply(model, tierInfo);

            if (tierTable == null) return;

            // 아우라가 원본 메시 경계를 쓰도록 테두리 복제본보다 먼저 부착
            TierAura.Attach(model, tierTable.AuraPrefab, tierInfo);
            TierOutline.Attach(model, tierTable.OutlineMaterial, tierInfo);
        }

        // 개체 없이 쓰는 기본 능력치 (맨손 등, 범위 최솟값)
        public virtual ItemStats BaseStats => ItemStats.Zero;

        // 능력치 = T0 범위 랜덤 × 티어 배율 × 등급 배율
        public override ItemInstance CreateInstance(int tier = MinTier, bool useLowestGrade = false)
        {
            int clampedTier = tierTable != null ? tierTable.Clamp(tier) : MinTier;
            ItemTier tierInfo = tierTable != null ? tierTable.Get(clampedTier) : null;
            ItemGrade grade = gradeTable == null ? null
                : useLowestGrade ? gradeTable.Lowest
                : gradeTable.Roll();

            float multiplier = (tierInfo != null ? tierInfo.StatMultiplier : NoMultiplier) * (grade != null ? grade.StatMultiplier : NoMultiplier);
            return new ItemInstance(this, clampedTier, tierInfo, grade, RollStats(multiplier));
        }

        protected virtual ItemStats RollStats(float multiplier) => ItemStats.Zero;
    }
}

using System;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    [Serializable]
    public sealed class BreakablePart
    {
        private const int NoDurability = 0;
        private const int MinDurability = 1;
        [SerializeField]
        private string displayName;
        [SerializeField]
        private Collider[] colliders;
        [SerializeField]
        private Renderer[] renderers;
        private int maxDurability;
        [SerializeField, Min(0f)]
        private float normalMultiplier = 1f;
        [SerializeField, Min(0f)]
        private float brokenMultiplier = 1.5f;
        private int currentDurability;
        [Tooltip("최대 체력 대비 내구도 비율")]
        [SerializeField, Range(0f, 1f)]
        private float durabilityRatio;
        public string DisplayName => displayName;
        public Collider[] Colliders => colliders;
        public Renderer[] Renderers => renderers;
        public int MaxDurability => maxDurability;
        public float BrokenMultiplier => brokenMultiplier;
        public int CurrentDurability => currentDurability;
        public bool IsBroken => currentDurability <= NoDurability;

        public void ResetState(int ownerMaxHealth)
        {
            int durability = Mathf.RoundToInt(ownerMaxHealth * durabilityRatio);
            maxDurability = Mathf.Max(MinDurability, durability);
            currentDurability = maxDurability;
        }

        public float CurrentMultiplier() => IsBroken ? brokenMultiplier : normalMultiplier;

        // 이번 피해로 인해 파괴되었는지 여부를 반환
        public bool ApplyDamage(int amount)
        {
            if (IsBroken) return false;
            currentDurability = Mathf.Max(NoDurability, currentDurability - amount);
            return IsBroken;
        }
    }
}

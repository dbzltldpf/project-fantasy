using System;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    [Serializable]
    public sealed class BreakablePart
    {
        private const int NoDurability = 0;

        [SerializeField]
        private string displayName;
        [SerializeField]
        private Collider[] colliders;
        [SerializeField]
        private Renderer[] renderers;
        [SerializeField, Min(1)]
        private int maxDurability = 100;
        [SerializeField, Min(0f)]
        private float normalMultiplier = 1f;
        [SerializeField, Min(0f)]
        private float brokenMultiplier = 1.5f;
        private int currentDurability;

        public string DisplayName => displayName;
        public Collider[] Colliders => colliders;
        public Renderer[] Renderers => renderers;
        public int MaxDurability => maxDurability;
        public float BrokenMultiplier => brokenMultiplier;
        public int CurrentDurability => currentDurability;
        public bool IsBroken => currentDurability <= NoDurability;

        public void ResetState()
        {
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

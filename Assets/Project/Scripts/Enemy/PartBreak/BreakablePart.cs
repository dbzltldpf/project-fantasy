using System;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    [Serializable]
    public sealed class BreakablePart
    {
        [SerializeField]
        private string displayName;
        [SerializeField]
        private Collider[] colliders;
        [SerializeField, Min(1)]
        private int maxDurability = 100;
        [SerializeField, Min(0f)]
        private float normalMultiplier = 1f;
        [SerializeField, Min(0f)]
        private float brokenMultiplier = 1.5f;
        private int currentDurability;

        public string DisplayName => displayName;
        public Collider[] Colliders => colliders;
        public int MaxDurability => maxDurability;
        public float BrokenMultiplier => brokenMultiplier;
        public int CurrentDurability => currentDurability;
        public bool IsBroken => currentDurability <= 0;

        public void ResetState()
        {
            currentDurability = maxDurability;
        }
        public float CurrentMultiplier() => IsBroken ? brokenMultiplier : normalMultiplier;
        public void ApplyDamage(int amount)
        {
            currentDurability -= amount;
            if (IsBroken) { }
            ;

        }
    }
}

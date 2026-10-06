using System;
using ProjectFantasy.Combat;
using UnityEngine;

namespace ProjectFantasy.Items
{
    // 체력 회복
    [Serializable]
    public sealed class HealEffect : ConsumableEffect
    {
        [Tooltip("회복량")]
        [SerializeField, Min(1)] private int amount = 30;

        public override void Apply(GameObject user)
        {
            if (user.TryGetComponent(out Health health)) health.Heal(amount);
        }
    }
}

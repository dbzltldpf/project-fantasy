using System.Collections.Generic;
using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Enemy
{
    [DisallowMultipleComponent]
    public sealed class BreakablePartManager : MonoBehaviour, IDamageMultiplier
    {
        [SerializeField]
        private BreakablePart[] parts;
        private readonly Dictionary<Collider, BreakablePart> partByCollider = new Dictionary<Collider, BreakablePart>();

        private const float NoMultiplier = 1f;
        private Health health;
        private void Awake()
        {
            health = GetComponent<Health>();

            foreach (var part in parts)
            {
                part.ResetState();

                foreach (var hitbox in part.Colliders)
                {
                    if (hitbox == null) continue;
                    if (partByCollider.ContainsKey(hitbox))
                    {
                        Debug.LogWarning($"[{nameof(BreakablePartManager)}] '{hitbox.name}' 콜라이더가 여러 부위에 등록되어 있습니다. 첫 부위만 사용합니다.");
                        continue;
                    }
                    partByCollider[hitbox] = part;

                }
            }
        }
        private void OnEnable()
        {
            health.Damaged += HandleDamaged;
        }
        private void OnDisable()
        {
            health.Damaged -= HandleDamaged;
        }
        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (damageInfo.HitCollider == null) return;
            BreakablePart breakablePart = partByCollider[damageInfo.HitCollider];
            breakablePart.ApplyDamage(damageInfo.Amount);
        }
        public float GetMultiplier(in DamageInfo damageInfo)
        {
            if (damageInfo.HitCollider == null) return NoMultiplier;
            return partByCollider.TryGetValue(damageInfo.HitCollider, out BreakablePart hitPart) ? hitPart.CurrentMultiplier() : NoMultiplier;
        }
    }
}

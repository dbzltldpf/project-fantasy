using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ProjectFantasy.Enemy
{
    [DisallowMultipleComponent]
    public sealed class BreakablePartManager : MonoBehaviour, IDamageMultiplier
    {
        private const float NoMultiplier = 1f;
        [Tooltip("피해 로그 출력 (에디터/개발 빌드 전용)")]
        [SerializeField] private bool logPartDamage = true;

        [SerializeField]
        private BreakablePart[] breakableParts;
        private readonly Dictionary<Collider, BreakablePart> partByCollider = new Dictionary<Collider, BreakablePart>();
        private Health health;
        private EnemyController enemyController;
        public event Action<BreakablePart> PartBroken;
        public IReadOnlyList<BreakablePart> BreakableParts => breakableParts;
        private void Awake()
        {
            health = GetComponent<Health>();
            enemyController = GetComponent<EnemyController>();

            foreach (var part in breakableParts)
            {
                foreach (var hitbox in part.Colliders)
                {
                    if (hitbox == null) continue;
                    if (partByCollider.ContainsKey(hitbox))
                    {
                        Debug.LogWarning($"[{nameof(BreakablePartManager)}] '{hitbox.name}' 콜라이더가 여러 부위에 등록되어 있습니다. 첫 부위만 사용합니다.", this);
                        continue;
                    }
                    partByCollider[hitbox] = part;
                }
            }
        }
        private void OnEnable()
        {
            health.Damaged += HandleDamaged;
            enemyController.CombatReset += HandleCombatReset;
        }
        private void OnDisable()
        {
            health.Damaged -= HandleDamaged;
            enemyController.CombatReset -= HandleCombatReset;
        }
        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (damageInfo.HitCollider == null) return;
            if (!partByCollider.TryGetValue(damageInfo.HitCollider, out BreakablePart breakablePart)) return;
            bool justBroken = breakablePart.ApplyDamage(damageInfo.Amount);
            LogPartDamage(breakablePart, damageInfo.Amount, justBroken);
            if (justBroken)
            {
                PartBroken?.Invoke(breakablePart);
            }
        }
        private void HandleCombatReset()
        {
            foreach (var part in breakableParts)
            {
                part.ResetState(health.MaxHealth);
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void LogPartDamage(BreakablePart breakablePart, int amount, bool justBroken)
        {
            if (!logPartDamage) return;
            Debug.Log($"[{name}] {breakablePart.DisplayName} -{amount} ({breakablePart.CurrentDurability}/{breakablePart.MaxDurability}) {(justBroken ? "→ 파괴!" : "")}", this);
        }

        public float GetMultiplier(in DamageInfo damageInfo)
        {
            if (damageInfo.HitCollider == null) return NoMultiplier;
            return partByCollider.TryGetValue(damageInfo.HitCollider, out BreakablePart hitPart) ? hitPart.CurrentMultiplier() : NoMultiplier;
        }
    }
}
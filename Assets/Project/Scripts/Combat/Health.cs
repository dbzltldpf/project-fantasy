using System;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 체력과 피격 후 무적 시간 관리
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [Tooltip("최대 체력")]
        [SerializeField, Min(1)] private int maxHealth = 100;
        [Tooltip("피격 후 무적 시간 (초, 허수아비는 0)")]
        [SerializeField, Min(0f)] private float invincibleDuration = 0.5f;

        private IDamageBlocker damageBlocker;
        private IDamageReducer damageReducer;
        private float invincibleEndTime;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;
        public bool IsInvincible => Time.time < invincibleEndTime;

        public event Action<int, int> HealthChanged;
        public event Action<DamageInfo> Damaged;
        public event Action<DamageInfo> Blocked;
        public event Action Died;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            damageBlocker = GetComponent<IDamageBlocker>();
            damageReducer = GetComponent<IDamageReducer>();
        }

        // 방어(가드) → 방어력 감소 → 적용 순서, Damaged에는 감소 후 피해량 전달
        public void TakeDamage(in DamageInfo damageInfo)
        {
            if (!IsAlive || IsInvincible || damageInfo.Amount <= 0) return;

            if (damageBlocker != null && damageBlocker.TryBlock(damageInfo))
            {
                Blocked?.Invoke(damageInfo);
                return;
            }

            DamageInfo applied = damageReducer != null ? damageInfo.WithAmount(damageReducer.Reduce(damageInfo)) : damageInfo;

            CurrentHealth = Mathf.Max(0, CurrentHealth - applied.Amount);
            invincibleEndTime = Time.time + invincibleDuration;

            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            Damaged?.Invoke(applied);
            if (!IsAlive) Died?.Invoke();
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        // 최대 체력으로 복구 (사망 상태 포함, 리스폰·허수아비용)
        public void RestoreFull()
        {
            CurrentHealth = maxHealth;
            invincibleEndTime = 0f;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}

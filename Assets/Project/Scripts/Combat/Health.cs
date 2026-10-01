using System;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 체력과 피격 후 무적 시간 관리
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField, Min(0f)] private float invincibleDuration = 0.5f;

        private IDamageBlocker damageBlocker;
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
        }

        public void TakeDamage(in DamageInfo damageInfo)
        {
            if (!IsAlive || IsInvincible || damageInfo.Amount <= 0) return;

            if (damageBlocker != null && damageBlocker.TryBlock(damageInfo))
            {
                Blocked?.Invoke(damageInfo);
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damageInfo.Amount);
            invincibleEndTime = Time.time + invincibleDuration;

            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            Damaged?.Invoke(damageInfo);
            if (!IsAlive) Died?.Invoke();
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}

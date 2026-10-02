using System.Diagnostics;
using ProjectFantasy.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ProjectFantasy.Combat
{
    // 훈련용 허수아비: 피격 후 체력 복구로 죽지 않음 (데미지 숫자는 DamageNumberEmitter와 함께 사용)
    [RequireComponent(typeof(Health))]
    [DisallowMultipleComponent]
    public sealed class TrainingDummy : MonoBehaviour
    {
        [Tooltip("피해 로그 출력 (에디터/개발 빌드 전용)")]
        [SerializeField] private bool logDamage = true;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable() => health.Damaged += HandleDamaged;
        private void OnDisable() => health.Damaged -= HandleDamaged;

        // 사망 판정 전에 복구되도록 Damaged 시점에 처리
        private void HandleDamaged(DamageInfo damageInfo)
        {
            LogDamage(damageInfo);
            health.RestoreFull();
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void LogDamage(in DamageInfo damageInfo)
        {
            if (!logDamage) return;

            string instigatorName = damageInfo.Instigator != null ? damageInfo.Instigator.name : "Unknown";
            Debug.Log($"[{name}] {damageInfo.Amount} 피해 ← {instigatorName}", this);
        }
    }
}

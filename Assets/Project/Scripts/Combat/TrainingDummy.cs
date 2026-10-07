using System.Diagnostics;
using ProjectFantasy.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ProjectFantasy.Combat
{
    // 훈련용 허수아비: 피격 후 체력 복구로 죽지 않음, 누적 피해가 가상 체력에 도달하면 처치 판정(KillReward)
    [RequireComponent(typeof(Health))]
    [DisallowMultipleComponent]
    public sealed class TrainingDummy : MonoBehaviour
    {
        private const int NoDamage = 0;

        [Tooltip("이만큼 누적 피해를 받으면 처치로 판정해 보상 지급 후 초기화 (KillReward가 있을 때)")]
        [SerializeField, Min(1)] private int virtualHealth = 100;
        [Tooltip("피해 로그 출력 (에디터/개발 빌드 전용)")]
        [SerializeField] private bool logDamage = true;

        private Health health;
        private KillReward killReward;
        private int accumulatedDamage;

        private void Awake()
        {
            health = GetComponent<Health>();
            killReward = GetComponent<KillReward>();
        }

        private void OnEnable() => health.Damaged += HandleDamaged;
        private void OnDisable() => health.Damaged -= HandleDamaged;

        // 사망 판정 전에 복구되도록 Damaged 시점에 처리
        private void HandleDamaged(DamageInfo damageInfo)
        {
            LogDamage(damageInfo);
            health.RestoreFull();

            if (killReward == null) return;

            accumulatedDamage += damageInfo.Amount;
            if (accumulatedDamage < virtualHealth) return;

            accumulatedDamage = NoDamage;
            killReward.Grant(damageInfo);
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

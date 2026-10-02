using ProjectFantasy.Combat;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // Health 피격 시 데미지 숫자 표시 요청 (허수아비·적 공용)
    [RequireComponent(typeof(Health))]
    [DisallowMultipleComponent]
    public sealed class DamageNumberEmitter : MonoBehaviour
    {
        [Tooltip("비우면 씬에서 자동 탐색 (최초 1회)")]
        [SerializeField] private DamageNumberSpawner spawner;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
            if (spawner == null) spawner = FindFirstObjectByType<DamageNumberSpawner>();
        }

        private void OnEnable() => health.Damaged += HandleDamaged;
        private void OnDisable() => health.Damaged -= HandleDamaged;

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (spawner != null) spawner.Spawn(damageInfo.Amount, damageInfo.HitPoint);
        }
    }
}

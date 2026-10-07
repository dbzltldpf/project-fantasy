using System.Collections.Generic;
using ProjectFantasy.Core;
using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 처치 보상: 피해를 준 공격자를 기록했다가 사망 시 각자에게 경험치 전액 지급 (적·허수아비 공용)
    [RequireComponent(typeof(Health))]
    [DisallowMultipleComponent]
    public sealed class KillReward : MonoBehaviour
    {
        [Tooltip("처치 시 기여한 공격자 각각이 받는 경험치")]
        [SerializeField, Min(0)] private int experience = 30;

        private readonly HashSet<IKillRewardReceiver> contributors = new HashSet<IKillRewardReceiver>();
        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.Damaged += HandleDamaged;
            health.Died += Grant;
        }

        private void OnDisable()
        {
            health.Damaged -= HandleDamaged;
            health.Died -= Grant;
        }

        // 사망 외 처치 판정(허수아비 등): 마지막 타격자를 먼저 기록 (이벤트 구독 순서와 무관)
        public void Grant(in DamageInfo finalHit)
        {
            HandleDamaged(finalHit);
            Grant();
        }

        public void Grant()
        {
            foreach (IKillRewardReceiver receiver in contributors)
            {
                receiver.ReceiveKillReward(experience);
            }
            contributors.Clear();
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (damageInfo.Instigator != null && damageInfo.Instigator.TryGetComponent(out IKillRewardReceiver receiver))
            {
                contributors.Add(receiver);
            }
        }
    }
}

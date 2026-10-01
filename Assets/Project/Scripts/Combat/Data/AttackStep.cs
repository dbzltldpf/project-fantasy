using System;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace ProjectFantasy.Combat
{
    // 콤보 한 단계의 타이밍/데미지 배율 (시간 단위: 초, 데미지 = 무기 공격력 × 배율)
    [Serializable]
    public sealed class AttackStep
    {
        [SerializeField] private string stateName = "Melee_1H_Attack_Slice_Horizontal";
        [SerializeField, Min(0f)] private float damageMultiplier = 1f;
        [SerializeField, Min(0f)] private float duration = 0.7f;

        [Header("Hit")]
        [SerializeField, Min(0f)] private float hitStartTime = 0.2f;
        [SerializeField, Min(0f)] private float hitEndTime = 0.35f;

        [Header("Combo")]
        [Tooltip("이 시점부터 다음 공격 입력을 예약")]
        [SerializeField, Min(0f), FormerlySerializedAs("comboInputTime")] private float comboInputStartTime = 0.15f;
        [Tooltip("예약된 입력이 있으면 이 시점에 다음 단계로 전이")]
        [SerializeField, Min(0f)] private float comboTransitionTime = 0.45f;

        [Header("Lunge")]
        [SerializeField, Min(0f)] private float lungeSpeed;
        [SerializeField, Min(0f)] private float lungeDuration = 0.15f;

        [NonSerialized] private int stateHash;
        [NonSerialized] private bool isHashCached;

        public string StateName => stateName;
        public float DamageMultiplier => damageMultiplier;
        public float LungeSpeed => lungeSpeed;

        public int StateHash
        {
            get
            {
                if (!isHashCached)
                {
                    stateHash = Animator.StringToHash(stateName);
                    isHashCached = true;
                }
                return stateHash;
            }
        }

        public bool IsInHitWindow(float elapsedTime) => elapsedTime >= hitStartTime && elapsedTime <= hitEndTime;
        public bool CanQueueCombo(float elapsedTime) => elapsedTime >= comboInputStartTime;
        public bool CanTransitionCombo(float elapsedTime) => elapsedTime >= comboTransitionTime;
        public bool IsLunging(float elapsedTime) => elapsedTime < lungeDuration;
        public bool IsFinished(float elapsedTime) => elapsedTime >= duration;

        public void InvalidateCache() => isHashCached = false;

        // 타이밍 순서 검사 (에디터 OnValidate 전용)
        public void Validate(Object context, int stepIndex)
        {
            WarnIf(hitStartTime > hitEndTime, "hitStartTime > hitEndTime", context, stepIndex);
            WarnIf(comboInputStartTime > comboTransitionTime, "comboInputStartTime > comboTransitionTime", context, stepIndex);
            WarnIf(hitEndTime > comboTransitionTime, "hitEndTime > comboTransitionTime (타격 중 다음 단계로 전이됨)", context, stepIndex);
            WarnIf(comboTransitionTime > duration, "comboTransitionTime > duration (콤보 연계 불가)", context, stepIndex);
            WarnIf(hitEndTime > duration, "hitEndTime > duration", context, stepIndex);
        }

        private static void WarnIf(bool condition, string message, Object context, int stepIndex)
        {
            if (condition) Debug.LogWarning($"[{context.name}] Step {stepIndex}: {message}", context);
        }
    }
}

using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ProjectFantasy.Player
{
    // 애니메이터 상태 재생 전담 (상태 해시 캐싱, 중복 CrossFade 방지)
    [DisallowMultipleComponent]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        private const int BaseLayerIndex = 0;
        private const float StartTimeOffset = 0f;

        [SerializeField] private Animator animator;

        [Header("State Names")]
        [SerializeField] private string idleState = "Idle_A";
        [SerializeField] private string walkState = "Walking_B";
        [SerializeField] private string runState = "Running_B";
        [SerializeField] private string airState = "Jump_Idle";
        [SerializeField] private string hitState = "Hit_A";
        [SerializeField] private string deathState = "Death_A";

        [Header("Locomotion")]
        [SerializeField, Min(0f)] private float idleSpeedThreshold = 0.1f;
        [SerializeField, Min(0f)] private float runSpeedThreshold = 3.5f;

        [Header("Transition")]
        [SerializeField, Min(0f)] private float locomotionCrossFade = 0.15f;
        [SerializeField, Min(0f)] private float actionCrossFade = 0.1f;

        private int idleHash;
        private int walkHash;
        private int runHash;
        private int airHash;
        private int hitHash;
        private int deathHash;
        private int currentStateHash;

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            animator.applyRootMotion = false;

            idleHash = Animator.StringToHash(idleState);
            walkHash = Animator.StringToHash(walkState);
            runHash = Animator.StringToHash(runState);
            airHash = Animator.StringToHash(airState);
            hitHash = Animator.StringToHash(hitState);
            deathHash = Animator.StringToHash(deathState);

            ValidateBaseStates();
        }

        public void PlayLocomotion(float horizontalSpeed)
        {
            int targetHash = horizontalSpeed < idleSpeedThreshold ? idleHash
                : horizontalSpeed < runSpeedThreshold ? walkHash
                : runHash;
            CrossFade(targetHash, locomotionCrossFade, false);
        }

        public void PlayAir() => CrossFade(airHash, locomotionCrossFade, false);
        public void PlayHit() => CrossFade(hitHash, actionCrossFade, true);
        public void PlayDeath() => CrossFade(deathHash, actionCrossFade, true);
        public void PlayAttack(int stateHash, float crossFadeDuration) => CrossFade(stateHash, crossFadeDuration, true);

        // 누락된 상태를 이름으로 보고 (릴리즈 빌드에서는 호출 제거)
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void ValidateState(string stateName)
        {
            if (animator.HasState(BaseLayerIndex, Animator.StringToHash(stateName))) return;

            Debug.LogError($"[{nameof(PlayerAnimator)}] 애니메이터에 '{stateName}' 상태가 없습니다.", this);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void ValidateBaseStates()
        {
            ValidateState(idleState);
            ValidateState(walkState);
            ValidateState(runState);
            ValidateState(airState);
            ValidateState(hitState);
            ValidateState(deathState);
        }

        private void CrossFade(int stateHash, float duration, bool restart)
        {
            if (!restart && stateHash == currentStateHash) return;

            currentStateHash = stateHash;
            animator.CrossFadeInFixedTime(stateHash, duration, BaseLayerIndex, StartTimeOffset);
        }
    }
}

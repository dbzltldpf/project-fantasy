using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace ProjectFantasy.Player
{
    // 애니메이터 상태 재생 전담 (상태 해시 캐싱, 중복 CrossFade 방지, 조준 자세 고정)
    [DisallowMultipleComponent]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        private const int BaseLayerIndex = 0;
        private const int NoState = 0;
        private const float StartTimeOffset = 0f;
        private const float NormalPoseSpeed = 1f;
        private const float FrozenPoseSpeed = 0f;
        private const float NoHoldTime = 1f;

        [SerializeField] private Animator animator;
        [Tooltip("조준 대기 상태의 Speed Multiplier로 연결할 Float 파라미터")]
        [SerializeField] private string poseSpeedParameter = "PoseSpeed";
        [Tooltip("액션 상태의 Speed Multiplier로 연결할 Float 파라미터 (ActionData 재생 속도)")]
        [SerializeField] private string actionSpeedParameter = "ActionSpeed";

        [Header("State Names")]
        [SerializeField, FormerlySerializedAs("idleState")] private string defaultIdleState = "Idle_A";
        [SerializeField] private string walkState = "Walking_B";
        [SerializeField] private string runState = "Running_B";
        [SerializeField] private string airState = "Jump_Idle";
        [SerializeField] private string hitState = "Hit_A";
        [SerializeField] private string deathState = "Death_A";
        [SerializeField] private string guardState = "Melee_Blocking";
        [SerializeField] private string blockHitState = "Melee_Block_Hit";

        [Header("Aim Locomotion")]
        [SerializeField] private string walkBackwardState = "Walking_Backwards";
        [SerializeField] private string strafeLeftState = "Running_Strafe_Left";
        [SerializeField] private string strafeRightState = "Running_Strafe_Right";

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
        private int guardHash;
        private int blockHitHash;
        private int walkBackwardHash;
        private int strafeLeftHash;
        private int strafeRightHash;
        private int currentStateHash;

        private int poseSpeedHash;
        private bool hasPoseSpeedParameter;
        private int actionSpeedHash;
        private bool hasActionSpeedParameter;
        private int holdStateHash = NoState;
        private float holdNormalizedTime;
        private bool isPoseFrozen;

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            animator.applyRootMotion = false;

            idleHash = Animator.StringToHash(defaultIdleState);
            walkHash = Animator.StringToHash(walkState);
            runHash = Animator.StringToHash(runState);
            airHash = Animator.StringToHash(airState);
            hitHash = Animator.StringToHash(hitState);
            deathHash = Animator.StringToHash(deathState);
            guardHash = Animator.StringToHash(guardState);
            blockHitHash = Animator.StringToHash(blockHitState);
            walkBackwardHash = Animator.StringToHash(walkBackwardState);
            strafeLeftHash = Animator.StringToHash(strafeLeftState);
            strafeRightHash = Animator.StringToHash(strafeRightState);

            poseSpeedHash = Animator.StringToHash(poseSpeedParameter);
            hasPoseSpeedParameter = HasFloatParameter(poseSpeedHash);
            actionSpeedHash = Animator.StringToHash(actionSpeedParameter);
            hasActionSpeedParameter = HasFloatParameter(actionSpeedHash);

            ValidateBaseStates();
        }

        // 조준 대기 상태가 지정 지점에 도달하면 자세 고정
        private void Update()
        {
            if (holdStateHash == NoState || isPoseFrozen || !hasPoseSpeedParameter) return;
            if (animator.IsInTransition(BaseLayerIndex)) return;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(BaseLayerIndex);
            if (stateInfo.shortNameHash != holdStateHash || stateInfo.normalizedTime < holdNormalizedTime) return;

            animator.SetFloat(poseSpeedHash, FrozenPoseSpeed);
            isPoseFrozen = true;
        }

        // 무기 종류별 대기 모션 교체 (다음 PlayLocomotion에서 반영)
        public void SetIdleState(int stateHash) => idleHash = stateHash;

        public void PlayLocomotion(float horizontalSpeed)
        {
            int targetHash = horizontalSpeed < idleSpeedThreshold ? idleHash
                : horizontalSpeed < runSpeedThreshold ? walkHash
                : runHash;
            CrossFade(targetHash, locomotionCrossFade, false);
        }

        public void PlayAir() => CrossFade(airHash, locomotionCrossFade, false);
        public void PlayGuard() => CrossFade(guardHash, actionCrossFade, false);
        public void PlayBlockHit() => CrossFade(blockHitHash, actionCrossFade, true);
        public void PlayHit() => CrossFade(hitHash, actionCrossFade, true);
        public void PlayDeath() => CrossFade(deathHash, actionCrossFade, true);
        public void PlayAction(int stateHash) => CrossFade(stateHash, actionCrossFade, true);

        // 액션 타임라인 재생 (애니메이션 속도 = 이벤트 시계 속도)
        public void PlayAction(int stateHash, float crossFadeDuration, float playbackSpeed)
        {
            if (hasActionSpeedParameter) animator.SetFloat(actionSpeedHash, playbackSpeed);
            CrossFade(stateHash, crossFadeDuration, true);
        }

        // 조준 중 이동: 캐릭터 로컬 이동 방향으로 전진/후진/좌우 스트레이프 분기
        // holdNormalizedTime: 조준 대기 자세를 멈출 정규화 시간 (1이면 고정 안 함)
        public void PlayAimLocomotion(Vector3 localVelocity, int aimIdleHash, float holdNormalizedTime)
        {
            float absForward = Mathf.Abs(localVelocity.z);
            float absSide = Mathf.Abs(localVelocity.x);
            bool isMoving = absForward >= idleSpeedThreshold || absSide >= idleSpeedThreshold;

            int targetHash = !isMoving ? aimIdleHash
                : absForward >= absSide ? (localVelocity.z >= 0f ? walkHash : walkBackwardHash)
                : (localVelocity.x >= 0f ? strafeRightHash : strafeLeftHash);
            CrossFade(targetHash, locomotionCrossFade, false);

            if (targetHash == aimIdleHash && holdNormalizedTime < NoHoldTime)
            {
                holdStateHash = aimIdleHash;
                this.holdNormalizedTime = holdNormalizedTime;
            }
        }

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
            ValidateState(defaultIdleState);
            ValidateState(walkState);
            ValidateState(runState);
            ValidateState(airState);
            ValidateState(hitState);
            ValidateState(deathState);
            ValidateState(guardState);
            ValidateState(walkBackwardState);
            ValidateState(strafeLeftState);
            ValidateState(strafeRightState);
            ValidateState(blockHitState);

            if (!hasPoseSpeedParameter)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimator)}] Float 파라미터 '{poseSpeedParameter}'가 없어 조준 자세 고정이 비활성화됩니다.", this);
            }

            if (!hasActionSpeedParameter)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimator)}] Float 파라미터 '{actionSpeedParameter}'가 없어 액션 재생 속도 배율이 비활성화됩니다.", this);
            }
        }

        private void CrossFade(int stateHash, float duration, bool restart)
        {
            if (!restart && stateHash == currentStateHash) return;

            ReleasePoseHold();
            currentStateHash = stateHash;
            animator.CrossFadeInFixedTime(stateHash, duration, BaseLayerIndex, StartTimeOffset);
        }

        private void ReleasePoseHold()
        {
            holdStateHash = NoState;
            if (!isPoseFrozen) return;

            animator.SetFloat(poseSpeedHash, NormalPoseSpeed);
            isPoseFrozen = false;
        }

        // 초기화 시 1회 (parameters는 배열 할당)
        private bool HasFloatParameter(int parameterHash)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == parameterHash && parameter.type == AnimatorControllerParameterType.Float) return true;
            }
            return false;
        }
    }
}

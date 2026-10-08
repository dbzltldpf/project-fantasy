using System.Diagnostics;
using ProjectFantasy.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ProjectFantasy.Player
{
    // 애니메이터 상태 재생 전담 (상태 해시 캐싱, 중복 CrossFade 방지, 조준 자세 고정)
    [DisallowMultipleComponent]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        private const int BaseLayerIndex = 0;
        private const float StartTimeOffset = 0f;

        [Tooltip("비우면 자식에서 자동 탐색")]
        [SerializeField] private Animator animator;
        [Tooltip("상태 이름·파라미터·전환 시간 (애니메이터 컨트롤러와 맞춘 데이터)")]
        [SerializeField] private PlayerAnimationData data;

        private int idleHash;
        private int walkHash;
        private int runHash;
        private int airHash;
        private int hitHash;
        private int deathHash;
        private int guardHash;
        private int blockHitHash;
        private int pickUpHash;
        private int walkBackwardHash;
        private int strafeLeftHash;
        private int strafeRightHash;
        private int currentStateHash;

        private AnimatorPoseHold poseHold;
        private int actionSpeedHash;
        private bool hasActionSpeedParameter;

        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            animator.applyRootMotion = false;

            idleHash = Animator.StringToHash(data.DefaultIdleState);
            walkHash = Animator.StringToHash(data.WalkState);
            runHash = Animator.StringToHash(data.RunState);
            airHash = Animator.StringToHash(data.AirState);
            hitHash = Animator.StringToHash(data.HitState);
            deathHash = Animator.StringToHash(data.DeathState);
            guardHash = Animator.StringToHash(data.GuardState);
            blockHitHash = Animator.StringToHash(data.BlockHitState);
            pickUpHash = Animator.StringToHash(data.PickUpState);
            walkBackwardHash = Animator.StringToHash(data.WalkBackwardState);
            strafeLeftHash = Animator.StringToHash(data.StrafeLeftState);
            strafeRightHash = Animator.StringToHash(data.StrafeRightState);

            poseHold = new AnimatorPoseHold(animator, Animator.StringToHash(data.PoseSpeedParameter));
            actionSpeedHash = Animator.StringToHash(data.ActionSpeedParameter);
            hasActionSpeedParameter = animator.HasFloatParameter(actionSpeedHash);

            ValidateBaseStates();
        }

        // 조준 대기 상태가 지정 지점에 도달하면 자세 고정
        private void Update() => poseHold.Tick();

        // 무기 종류별 대기 모션 교체 (다음 PlayLocomotion에서 반영)
        public void SetIdleState(int stateHash) => idleHash = stateHash;

        public void PlayLocomotion(float horizontalSpeed)
        {
            int targetHash = horizontalSpeed < data.IdleSpeedThreshold ? idleHash
                : horizontalSpeed < data.RunSpeedThreshold ? walkHash
                : runHash;
            CrossFade(targetHash, data.LocomotionCrossFade, false);
        }

        public void PlayAir() => CrossFade(airHash, data.LocomotionCrossFade, false);
        public void PlayGuard() => CrossFade(guardHash, data.ActionCrossFade, false);
        public void PlayBlockHit() => CrossFade(blockHitHash, data.ActionCrossFade, true);
        public void PlayHit() => CrossFade(hitHash, data.ActionCrossFade, true);
        public void PlayDeath() => CrossFade(deathHash, data.ActionCrossFade, true);
        public void PlayPickUp() => CrossFade(pickUpHash, data.ActionCrossFade, true);
        public void PlayAction(int stateHash) => CrossFade(stateHash, data.ActionCrossFade, true);

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
            bool isMoving = absForward >= data.IdleSpeedThreshold || absSide >= data.IdleSpeedThreshold;

            int targetHash = !isMoving ? aimIdleHash
                : absForward >= absSide ? (localVelocity.z >= 0f ? walkHash : walkBackwardHash)
                : (localVelocity.x >= 0f ? strafeRightHash : strafeLeftHash);
            CrossFade(targetHash, data.LocomotionCrossFade, false);

            if (targetHash == aimIdleHash) poseHold.Hold(aimIdleHash, holdNormalizedTime);
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
            ValidateState(data.DefaultIdleState);
            ValidateState(data.WalkState);
            ValidateState(data.RunState);
            ValidateState(data.AirState);
            ValidateState(data.HitState);
            ValidateState(data.DeathState);
            ValidateState(data.GuardState);
            ValidateState(data.WalkBackwardState);
            ValidateState(data.StrafeLeftState);
            ValidateState(data.StrafeRightState);
            ValidateState(data.BlockHitState);
            ValidateState(data.PickUpState);

            if (!poseHold.IsAvailable)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimator)}] Float 파라미터 '{data.PoseSpeedParameter}'가 없어 조준 자세 고정이 비활성화됩니다.", this);
            }

            if (!hasActionSpeedParameter)
            {
                Debug.LogWarning($"[{nameof(PlayerAnimator)}] Float 파라미터 '{data.ActionSpeedParameter}'가 없어 액션 재생 속도 배율이 비활성화됩니다.", this);
            }
        }

        private void CrossFade(int stateHash, float duration, bool restart)
        {
            if (!restart && stateHash == currentStateHash) return;

            poseHold.Release();
            currentStateHash = stateHash;
            animator.CrossFadeInFixedTime(stateHash, duration, BaseLayerIndex, StartTimeOffset);
        }
    }
}

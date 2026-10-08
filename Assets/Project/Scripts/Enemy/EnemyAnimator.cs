using System.Diagnostics;
using ProjectFantasy.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ProjectFantasy.Enemy
{
    // 적 애니메이터 상태 재생 (상태 해시 캐싱, 중복 CrossFade 방지, 액션 재생 속도, 조준 자세 고정)
    [DisallowMultipleComponent]
    public sealed class EnemyAnimator : MonoBehaviour
    {
        private const int BaseLayerIndex = 0;
        private const float StartTimeOffset = 0f;
        private const float ClipEndNormalizedTime = 1f;

        [Tooltip("비우면 자식에서 자동 탐색")]
        [SerializeField] private Animator animator;

        private EnemyAnimationData data;
        private int idleHash;
        private int walkHash;
        private int runHash;
        private int hitHash;
        private int deathHash;
        private int spawnHash;
        private int guardHash;
        private int blockHitHash;
        private int actionSpeedHash;
        private bool hasActionSpeedParameter;
        private AnimatorPoseHold poseHold;
        private int currentStateHash;

        private void Reset() => animator = GetComponentInChildren<Animator>();

        // EnemyController가 Awake에서 1회 호출 (idleState가 비면 기본 대기)
        public void Initialize(EnemyAnimationData animationData, string idleState)
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            animator.applyRootMotion = false;
            data = animationData;

            idleHash = Animator.StringToHash(string.IsNullOrEmpty(idleState) ? data.DefaultIdleState : idleState);
            walkHash = Animator.StringToHash(data.WalkState);
            runHash = Animator.StringToHash(data.RunState);
            hitHash = Animator.StringToHash(data.HitState);
            deathHash = Animator.StringToHash(data.DeathState);
            spawnHash = Animator.StringToHash(data.SpawnState);
            guardHash = Animator.StringToHash(data.GuardState);
            blockHitHash = Animator.StringToHash(data.BlockHitState);
            actionSpeedHash = Animator.StringToHash(data.ActionSpeedParameter);
            hasActionSpeedParameter = animator.HasFloatParameter(actionSpeedHash);
            poseHold = new AnimatorPoseHold(animator, Animator.StringToHash(data.PoseSpeedParameter));
        }

        // 조준 대기 등 고정 상태가 지정 지점에 도달하면 정지
        private void Update()
        {
            if (poseHold != null) poseHold.Tick();
        }

        public void PlayLocomotion(float horizontalSpeed)
        {
            int targetHash = horizontalSpeed < data.IdleSpeedThreshold ? idleHash
                : horizontalSpeed < data.RunSpeedThreshold ? walkHash
                : runHash;
            CrossFade(targetHash, data.LocomotionCrossFade, false);
        }

        public void PlaySpawn() => CrossFade(spawnHash, data.ActionCrossFade, true);
        public void PlayHit() => CrossFade(hitHash, data.ActionCrossFade, true);
        public void PlayDeath() => CrossFade(deathHash, data.ActionCrossFade, true);
        public void PlayGuard() => CrossFade(guardHash, data.ActionCrossFade, false);
        public void PlayBlockHit() => CrossFade(blockHitHash, data.ActionCrossFade, true);

        // 이름으로 지정한 상태 (원거리 조준·발사·장전, 마법 시전 등)
        public void PlayState(int stateHash, bool restart) => CrossFade(stateHash, data.ActionCrossFade, restart);

        // 반복하지 않는 클립이 끝까지 재생됐는지 (전환 중이거나 다른 상태면 false)
        public bool IsStateFinished(int stateHash)
        {
            if (animator.IsInTransition(BaseLayerIndex)) return false;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(BaseLayerIndex);
            return stateInfo.shortNameHash == stateHash && stateInfo.normalizedTime >= ClipEndNormalizedTime;
        }

        // 정규화 시간 holdTime에 자세 고정 (1 이상이면 고정 안 함, 상태에 PoseSpeed 연결 필요)
        public void PlayHeldState(int stateHash, float holdTime)
        {
            CrossFade(stateHash, data.ActionCrossFade, true);
            poseHold.Hold(stateHash, holdTime);
        }

        // 액션 타임라인 재생 (애니메이션 속도 = 이벤트 시계 속도)
        public void PlayAction(int stateHash, float crossFadeDuration, float playbackSpeed)
        {
            if (hasActionSpeedParameter) animator.SetFloat(actionSpeedHash, playbackSpeed);
            CrossFade(stateHash, crossFadeDuration, true);
        }

        // 누락된 상태를 이름으로 보고 (릴리즈 빌드에서는 호출 제거)
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public void ValidateState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName) || animator.HasState(BaseLayerIndex, Animator.StringToHash(stateName))) return;

            Debug.LogError($"[{nameof(EnemyAnimator)}] 애니메이터에 '{stateName}' 상태가 없습니다. (Tools → ProjectFantasy → Create Enemy Animator)", this);
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

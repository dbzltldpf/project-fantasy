using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 적 애니메이터 상태 이름·파라미터·전환 시간 (적 종류 공용, Create Enemy Animator가 이 이름으로 상태 생성)
    [CreateAssetMenu(fileName = "EnemyAnimationData", menuName = "ProjectFantasy/Enemy/Enemy Animation Data")]
    public sealed class EnemyAnimationData : ScriptableObject
    {
        private const float MinAnimationSpeed = 0.01f;

        [Header("Parameters")]
        [Tooltip("공격 상태의 Speed Multiplier로 연결한 Float 파라미터 (ActionData 재생 속도)")]
        [SerializeField] private string actionSpeedParameter = "ActionSpeed";
        [Tooltip("조준 대기 상태의 Speed Multiplier로 연결한 Float 파라미터 (자세 고정)")]
        [SerializeField] private string poseSpeedParameter = "PoseSpeed";
        [Tooltip("걷기·달리기 상태의 Speed Multiplier로 연결한 Float 파라미터 (이동 속도 맞춤)")]
        [SerializeField] private string locomotionSpeedParameter = "LocomotionSpeed";

        [Header("States (애니메이터 상태 이름 = 클립 이름)")]
        [Tooltip("무기 대기 모션이 없을 때 기본 대기")]
        [SerializeField] private string defaultIdleState = "Idle_A";
        [SerializeField] private string walkState = "Walking_B";
        [SerializeField] private string runState = "Running_B";
        [SerializeField] private string hitState = "Hit_A";
        [SerializeField] private string groggyState = "Hit_B";
        [SerializeField] private string deathState = "Death_A";
        [Tooltip("스폰(리스폰) 시 등장 모션")]
        [SerializeField] private string spawnState = "Spawn_Ground";
        [Tooltip("방패 가드 유지")]
        [SerializeField] private string guardState = "Melee_Blocking";
        [Tooltip("가드로 막았을 때")]
        [SerializeField] private string blockHitState = "Melee_Block_Hit";

        [Header("Locomotion")]
        [Tooltip("수평 속도가 이 값(m/s) 미만이면 대기 모션")]
        [SerializeField, Min(0f)] private float idleSpeedThreshold = 0.1f;
        [Tooltip("걷기(0)~달리기(1) 속도 사이 어디서 달리기 모션으로 바꿀지 (이동 속도가 바뀌어도 자동으로 따라감)")]
        [SerializeField, Range(0f, 1f)] private float runThresholdBlend = 0.5f;
        [Tooltip("걷기 클립 재생 배율 1일 때 발이 미끄러지지 않는 이동 속도 (m/s)")]
        [SerializeField, Min(MinAnimationSpeed)] private float walkAnimationSpeed = 1.5f;
        [Tooltip("달리기 클립 재생 배율 1일 때 발이 미끄러지지 않는 이동 속도 (m/s)")]
        [SerializeField, Min(MinAnimationSpeed)] private float runAnimationSpeed = 4f;

        [Header("Transition (초)")]
        [SerializeField, Min(0f)] private float locomotionCrossFade = 0.15f;
        [SerializeField, Min(0f)] private float actionCrossFade = 0.1f;

        public string ActionSpeedParameter => actionSpeedParameter;
        public string PoseSpeedParameter => poseSpeedParameter;
        public string LocomotionSpeedParameter => locomotionSpeedParameter;
        public string DefaultIdleState => defaultIdleState;
        public string WalkState => walkState;
        public string RunState => runState;
        public string HitState => hitState;
        public string GroggyState => groggyState;
        public string DeathState => deathState;
        public string SpawnState => spawnState;
        public string GuardState => guardState;
        public string BlockHitState => blockHitState;
        public float IdleSpeedThreshold => idleSpeedThreshold;
        public float RunThresholdBlend => runThresholdBlend;
        public float LocomotionCrossFade => locomotionCrossFade;
        public float ActionCrossFade => actionCrossFade;
        public float WalkAnimationSpeed => walkAnimationSpeed;
        public float RunAnimationSpeed => runAnimationSpeed;

        // 애니메이터 생성 도구용 기본 상태 목록
        public string[] GetBaseStates() => new[] { defaultIdleState, walkState, runState, hitState, groggyState, deathState, spawnState, guardState, blockHitState };
    }
}

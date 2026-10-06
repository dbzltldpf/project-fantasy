using UnityEngine;

namespace ProjectFantasy.Player
{
    // 애니메이터 상태 이름·파라미터·전환 시간·이동 모션 분기 속도 (애니메이터 컨트롤러와 1:1)
    [CreateAssetMenu(fileName = "PlayerAnimationData", menuName = "ProjectFantasy/Player/Player Animation Data")]
    public sealed class PlayerAnimationData : ScriptableObject
    {
        [Header("Parameters")]
        [Tooltip("조준 대기 상태의 Speed Multiplier로 연결한 Float 파라미터 (자세 고정)")]
        [SerializeField] private string poseSpeedParameter = "PoseSpeed";
        [Tooltip("공격 상태의 Speed Multiplier로 연결한 Float 파라미터 (ActionData 재생 속도)")]
        [SerializeField] private string actionSpeedParameter = "ActionSpeed";

        [Header("States (애니메이터 상태 이름과 정확히 일치)")]
        [Tooltip("맨손 기본 대기 (무기별 대기는 WeaponData.idleStateName)")]
        [SerializeField] private string defaultIdleState = "Idle_A";
        [Tooltip("걷기 상태")]
        [SerializeField] private string walkState = "Walking_B";
        [Tooltip("달리기 상태")]
        [SerializeField] private string runState = "Running_B";
        [Tooltip("공중(점프·낙하) 상태")]
        [SerializeField] private string airState = "Jump_Idle";
        [Tooltip("피격 상태")]
        [SerializeField] private string hitState = "Hit_A";
        [Tooltip("사망 상태")]
        [SerializeField] private string deathState = "Death_A";
        [Tooltip("방패 가드 유지 상태")]
        [SerializeField] private string guardState = "Melee_Blocking";
        [Tooltip("가드로 막았을 때 상태")]
        [SerializeField] private string blockHitState = "Melee_Block_Hit";
        [Tooltip("줍기 상태")]
        [SerializeField] private string pickUpState = "PickUp";

        [Header("Aim Locomotion (조준 중 이동)")]
        [Tooltip("조준 중 뒤로 걷기")]
        [SerializeField] private string walkBackwardState = "Walking_Backwards";
        [Tooltip("조준 중 왼쪽 이동")]
        [SerializeField] private string strafeLeftState = "Running_Strafe_Left";
        [Tooltip("조준 중 오른쪽 이동")]
        [SerializeField] private string strafeRightState = "Running_Strafe_Right";

        [Header("Locomotion")]
        [Tooltip("수평 속도가 이 값(m/s) 미만이면 대기 모션")]
        [SerializeField, Min(0f)] private float idleSpeedThreshold = 0.1f;
        [Tooltip("수평 속도가 이 값(m/s) 이상이면 달리기 모션 (걷기 속도보다 크고 달리기 속도보다 작게)")]
        [SerializeField, Min(0f)] private float runSpeedThreshold = 3.5f;

        [Header("Transition (초)")]
        [Tooltip("대기·걷기·달리기·공중 전환 시간")]
        [SerializeField, Min(0f)] private float locomotionCrossFade = 0.15f;
        [Tooltip("피격·가드·줍기 등 동작 전환 시간 (공격은 ActionData 값 사용)")]
        [SerializeField, Min(0f)] private float actionCrossFade = 0.1f;

        public string PoseSpeedParameter => poseSpeedParameter;
        public string ActionSpeedParameter => actionSpeedParameter;
        public string DefaultIdleState => defaultIdleState;
        public string WalkState => walkState;
        public string RunState => runState;
        public string AirState => airState;
        public string HitState => hitState;
        public string DeathState => deathState;
        public string GuardState => guardState;
        public string BlockHitState => blockHitState;
        public string PickUpState => pickUpState;
        public string WalkBackwardState => walkBackwardState;
        public string StrafeLeftState => strafeLeftState;
        public string StrafeRightState => strafeRightState;
        public float IdleSpeedThreshold => idleSpeedThreshold;
        public float RunSpeedThreshold => runSpeedThreshold;
        public float LocomotionCrossFade => locomotionCrossFade;
        public float ActionCrossFade => actionCrossFade;
    }
}

using UnityEngine;

namespace ProjectFantasy.Player
{
    // 플레이어 이동/점프 튜닝 데이터
    [CreateAssetMenu(fileName = "PlayerMovementData", menuName = "ProjectFantasy/Player/Movement Data")]
    public sealed class PlayerMovementData : ScriptableObject
    {
        private const float JumpVelocityFactor = 2f;

        [Header("Speed")]
        [Tooltip("기본 이동 속도 (m/s, runSpeedThreshold보다 작게)")]
        [SerializeField, Min(0f)] private float walkSpeed = 2f;
        [Tooltip("달리기 버튼 시 속도 (m/s)")]
        [SerializeField, Min(0f)] private float runSpeed = 5f;
        [Tooltip("가드 중 이동 속도 (m/s)")]
        [SerializeField, Min(0f)] private float guardMoveSpeed = 1.2f;
        [Tooltip("조준 모드 이동 속도 (m/s)")]
        [SerializeField, Min(0f)] private float aimMoveSpeed = 1.5f;

        [Header("Acceleration")]
        [Tooltip("지상 가속 (m/s²)")]
        [SerializeField, Min(0f)] private float acceleration = 30f;
        [Tooltip("지상 감속 (m/s²)")]
        [SerializeField, Min(0f)] private float deceleration = 40f;
        [Tooltip("공중 가감속 (m/s², 작을수록 공중 제어 약함)")]
        [SerializeField, Min(0f)] private float airAcceleration = 8f;

        [Header("Rotation")]
        [Tooltip("회전 속도 (도/초)")]
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;

        [Header("Jump & Gravity")]
        [Tooltip("점프 높이 (m)")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
        [Tooltip("중력 가속도 (m/s²)")]
        [SerializeField, Min(0f)] private float gravity = 25f;
        [Tooltip("경사·계단에서 지면에 붙이는 하강 속도 (m/s)")]
        [SerializeField, Min(0f)] private float groundedStickForce = 2f;
        [Tooltip("최대 낙하 속도 (m/s)")]
        [SerializeField, Min(0f)] private float maxFallSpeed = 50f;
        [Tooltip("지면을 벗어난 뒤 점프·지상 판정 유지 시간 (초)")]
        [SerializeField, Min(0f)] private float coyoteTime = 0.15f;

        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public float GuardMoveSpeed => guardMoveSpeed;
        public float AimMoveSpeed => aimMoveSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirAcceleration => airAcceleration;
        public float RotationSpeed => rotationSpeed;
        public float Gravity => gravity;
        public float GroundedStickForce => groundedStickForce;
        public float MaxFallSpeed => maxFallSpeed;
        public float CoyoteTime => coyoteTime;

        // v = √(2gh)
        public float JumpVelocity => Mathf.Sqrt(JumpVelocityFactor * gravity * jumpHeight);
    }
}

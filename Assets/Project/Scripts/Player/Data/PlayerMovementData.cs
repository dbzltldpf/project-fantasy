using UnityEngine;

namespace ProjectFantasy.Player
{
    // 플레이어 이동/점프 튜닝 데이터
    [CreateAssetMenu(fileName = "PlayerMovementData", menuName = "ProjectFantasy/Player/Movement Data")]
    public sealed class PlayerMovementData : ScriptableObject
    {
        private const float JumpVelocityFactor = 2f;

        [Header("Speed")]
        [SerializeField, Min(0f)] private float walkSpeed = 2f;
        [SerializeField, Min(0f)] private float runSpeed = 5f;

        [Header("Acceleration")]
        [SerializeField, Min(0f)] private float acceleration = 30f;
        [SerializeField, Min(0f)] private float deceleration = 40f;
        [SerializeField, Min(0f)] private float airAcceleration = 8f;

        [Header("Rotation")]
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;

        [Header("Jump & Gravity")]
        [SerializeField, Min(0f)] private float jumpHeight = 1.2f;
        [SerializeField, Min(0f)] private float gravity = 25f;
        [SerializeField, Min(0f)] private float groundedStickForce = 2f;
        [SerializeField, Min(0f)] private float maxFallSpeed = 50f;
        [SerializeField, Min(0f)] private float coyoteTime = 0.15f;

        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
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

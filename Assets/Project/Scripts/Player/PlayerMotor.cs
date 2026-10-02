using UnityEngine;

namespace ProjectFantasy.Player
{
    // CharacterController 기반 이동/중력 처리 (프레임당 Move 1회)
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private PlayerMovementData movementData;

        private CharacterController characterController;
        private Transform cachedTransform;

        private Vector3 horizontalVelocity;
        private Vector3 targetHorizontalVelocity;
        private float velocityChangeRate;
        private float verticalVelocity;
        private float lastGroundedTime = float.NegativeInfinity;

        public PlayerMovementData Data => movementData;
        public bool IsGrounded { get; private set; }
        public bool IsRising => verticalVelocity > 0f;
        public float HorizontalSpeed => horizontalVelocity.magnitude;
        public Vector3 HorizontalVelocity => horizontalVelocity;

        // 코요테 타임을 넘겨 지면을 벗어난 상태
        public bool IsAirborne => Time.time - lastGroundedTime > movementData.CoyoteTime;
        public bool CanJump => !IsAirborne;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            cachedTransform = transform;
        }

        public void SetTargetVelocity(Vector3 targetVelocity, float changeRate)
        {
            targetHorizontalVelocity = targetVelocity;
            velocityChangeRate = changeRate;
        }

        public void SetVelocityImmediate(Vector3 velocity)
        {
            horizontalVelocity = velocity;
            targetHorizontalVelocity = velocity;
        }

        public void Jump()
        {
            verticalVelocity = movementData.JumpVelocity;
            IsGrounded = false;
            lastGroundedTime = float.NegativeInfinity;
        }

        public void RotateTowards(Vector3 direction, float deltaTime)
        {
            if (!TryGetFlatRotation(direction, out Quaternion targetRotation)) return;

            cachedTransform.rotation = Quaternion.RotateTowards(
                cachedTransform.rotation, targetRotation, movementData.RotationSpeed * deltaTime);
        }

        public void SnapRotation(Vector3 direction)
        {
            if (TryGetFlatRotation(direction, out Quaternion targetRotation))
            {
                cachedTransform.rotation = targetRotation;
            }
        }

        // 상태 Tick 이후 PlayerController가 호출
        public void Tick(float deltaTime)
        {
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetHorizontalVelocity, velocityChangeRate * deltaTime);
            ApplyGravity(deltaTime);

            Vector3 motion = horizontalVelocity;
            motion.y = verticalVelocity;
            CollisionFlags collisionFlags = characterController.Move(motion * deltaTime);

            IsGrounded = (collisionFlags & CollisionFlags.Below) != 0;
            if (IsGrounded) lastGroundedTime = Time.time;
            if ((collisionFlags & CollisionFlags.Above) != 0 && IsRising) verticalVelocity = 0f;
        }

        private void ApplyGravity(float deltaTime)
        {
            if (IsGrounded && !IsRising)
            {
                verticalVelocity = -movementData.GroundedStickForce;
                return;
            }

            verticalVelocity = Mathf.Max(verticalVelocity - movementData.Gravity * deltaTime, -movementData.MaxFallSpeed);
        }

        private static bool TryGetFlatRotation(Vector3 direction, out Quaternion rotation)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                rotation = Quaternion.identity;
                return false;
            }

            rotation = Quaternion.LookRotation(direction);
            return true;
        }
    }
}

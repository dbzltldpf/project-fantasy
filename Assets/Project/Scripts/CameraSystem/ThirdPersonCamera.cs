using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFantasy.CameraSystem
{
    // 타깃을 따라가는 3인칭 오빗 카메라 (벽 충돌 시 당김)
    [DisallowMultipleComponent]
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        private const float FullRotationDegrees = 360f;
        private const float RollDegrees = 0f;

        [SerializeField] private Transform target;
        [SerializeField] private CameraData cameraData;
        [SerializeField] private InputActionReference lookAction;

        private Transform cachedTransform;
        private Vector3 focusPosition;
        private Vector3 focusVelocity;
        private float yaw;
        private float pitch;
        private float currentDistance;

        private void Awake()
        {
            cachedTransform = transform;
            yaw = cachedTransform.eulerAngles.y;
            pitch = cameraData.DefaultPitch;
            currentDistance = cameraData.Distance;
            if (target != null) focusPosition = GetTargetFocus();
        }

        private void OnEnable()
        {
            lookAction.action.Enable();
            if (cameraData.LockCursor) SetCursorLocked(true);
        }

        private void OnDisable()
        {
            if (cameraData.LockCursor) SetCursorLocked(false);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float deltaTime = Time.deltaTime;
            UpdateRotation(deltaTime);
            focusPosition = Vector3.SmoothDamp(focusPosition, GetTargetFocus(), ref focusVelocity, cameraData.FollowSmoothTime);
            UpdatePosition(deltaTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null) focusPosition = GetTargetFocus();
        }

        private void UpdateRotation(float deltaTime)
        {
            Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

            // 마우스 델타는 프레임 누적값, 스틱은 초당 속도로 처리
            bool isGamepad = lookAction.action.activeControl?.device is Gamepad;
            Vector2 lookDelta = isGamepad
                ? lookInput * (cameraData.GamepadSensitivity * deltaTime)
                : lookInput * cameraData.MouseSensitivity;

            yaw = Mathf.Repeat(yaw + lookDelta.x, FullRotationDegrees);
            pitch += cameraData.InvertY ? lookDelta.y : -lookDelta.y;
            pitch = Mathf.Clamp(pitch, cameraData.MinPitch, cameraData.MaxPitch);
        }

        private void UpdatePosition(float deltaTime)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, RollDegrees);
            Vector3 backward = rotation * Vector3.back;
            float targetDistance = ResolveCollisionDistance(backward);

            // 벽에 가까워질 땐 즉시, 멀어질 땐 부드럽게 복귀
            currentDistance = targetDistance < currentDistance
                ? targetDistance
                : Mathf.MoveTowards(currentDistance, targetDistance, cameraData.DistanceRecoverSpeed * deltaTime);

            cachedTransform.SetPositionAndRotation(focusPosition + backward * currentDistance, rotation);
        }

        private float ResolveCollisionDistance(Vector3 backward)
        {
            bool isBlocked = Physics.SphereCast(focusPosition, cameraData.CollisionRadius, backward, out RaycastHit hit,
                cameraData.Distance, cameraData.CollisionLayers, QueryTriggerInteraction.Ignore);

            return isBlocked ? Mathf.Max(cameraData.MinDistance, hit.distance) : cameraData.Distance;
        }

        private Vector3 GetTargetFocus() => target.position + Vector3.up * cameraData.TargetHeight;

        private static void SetCursorLocked(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }
    }
}

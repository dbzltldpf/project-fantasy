using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFantasy.CameraSystem
{
    // 타깃을 따라가는 3인칭 오빗 카메라 (벽 충돌 시 당김, 조준 프로필 블렌드)
    [DisallowMultipleComponent]
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        private const float FullRotationDegrees = 360f;
        private const float RollDegrees = 0f;
        private const float FlatPitchDegrees = 0f;
        private const float DefaultBlend = 0f;
        private const float AimBlend = 1f;

        [SerializeField] private Transform target;
        [SerializeField] private CameraData cameraData;
        [Tooltip("조준 시 숄더뷰 프로필 (비우면 조준 전환 없음)")]
        [SerializeField] private CameraData aimCameraData;
        [SerializeField, Min(0f)] private float aimBlendSpeed = 6f;
        [SerializeField] private InputActionReference lookAction;

        private Transform cachedTransform;
        private Camera cachedCamera;
        private Vector3 focusPosition;
        private Vector3 focusVelocity;
        private float yaw;
        private float pitch;
        private float currentDistance;
        private float aimBlend;
        private float targetAimBlend;

        private void Awake()
        {
            cachedTransform = transform;
            cachedCamera = GetComponent<Camera>();
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
            aimBlend = Mathf.MoveTowards(aimBlend, targetAimBlend, aimBlendSpeed * deltaTime);

            UpdateRotation(deltaTime);
            focusPosition = Vector3.SmoothDamp(focusPosition, GetTargetFocus(), ref focusVelocity, cameraData.FollowSmoothTime);
            UpdatePosition(deltaTime);
            if (cachedCamera != null) cachedCamera.fieldOfView = BlendProfile(data => data.FieldOfView);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null) focusPosition = GetTargetFocus();
        }

        public void SetAiming(bool isAiming) => targetAimBlend = isAiming && aimCameraData != null ? AimBlend : DefaultBlend;

        private void UpdateRotation(float deltaTime)
        {
            Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

            // 마우스 델타는 프레임 누적값, 스틱은 초당 속도로 처리
            bool isGamepad = lookAction.action.activeControl?.device is Gamepad;
            Vector2 lookDelta = isGamepad
                ? lookInput * (BlendProfile(data => data.GamepadSensitivity) * deltaTime)
                : lookInput * BlendProfile(data => data.MouseSensitivity);

            yaw = Mathf.Repeat(yaw + lookDelta.x, FullRotationDegrees);
            pitch += cameraData.InvertY ? lookDelta.y : -lookDelta.y;
            pitch = Mathf.Clamp(pitch, cameraData.MinPitch, cameraData.MaxPitch);
        }

        private void UpdatePosition(float deltaTime)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, RollDegrees);
            Vector3 backward = rotation * Vector3.back;
            float desiredDistance = BlendProfile(data => data.Distance);
            float targetDistance = ResolveCollisionDistance(backward, desiredDistance);

            // 벽에 가까워질 땐 즉시, 멀어질 땐 부드럽게 복귀
            currentDistance = targetDistance < currentDistance
                ? targetDistance
                : Mathf.MoveTowards(currentDistance, targetDistance, cameraData.DistanceRecoverSpeed * deltaTime);

            cachedTransform.SetPositionAndRotation(focusPosition + backward * currentDistance, rotation);
        }

        private float ResolveCollisionDistance(Vector3 backward, float desiredDistance)
        {
            bool isBlocked = Physics.SphereCast(focusPosition, cameraData.CollisionRadius, backward, out RaycastHit hit,
                desiredDistance, cameraData.CollisionLayers, QueryTriggerInteraction.Ignore);

            return isBlocked ? Mathf.Max(cameraData.MinDistance, hit.distance) : desiredDistance;
        }

        // 타깃 높이 + 어깨 오프셋 (yaw 기준 오른쪽)
        private Vector3 GetTargetFocus()
        {
            Vector3 shoulderRight = Quaternion.Euler(FlatPitchDegrees, yaw, RollDegrees) * Vector3.right;
            return target.position
                + Vector3.up * BlendProfile(data => data.TargetHeight)
                + shoulderRight * BlendProfile(data => data.ShoulderOffset);
        }

        // 기본/조준 프로필 값을 aimBlend 비율로 보간
        private float BlendProfile(Func<CameraData, float> selector)
        {
            float baseValue = selector(cameraData);
            return aimCameraData != null ? Mathf.Lerp(baseValue, selector(aimCameraData), aimBlend) : baseValue;
        }

        private static void SetCursorLocked(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
        }
    }
}

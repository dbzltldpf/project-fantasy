using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectFantasy.CameraSystem
{
    // 타깃을 따라가는 3인칭 오빗 카메라 (휠 줌, 벽 충돌 시 당김, 조준 프로필 블렌드)
    [DisallowMultipleComponent]
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        private const float FullRotationDegrees = 360f;
        private const float RollDegrees = 0f;
        private const float FlatPitchDegrees = 0f;
        private const float DefaultBlend = 0f;
        private const float AimBlend = 1f;
        private const float NoScroll = 0f;

        [Tooltip("따라갈 대상 (플레이어)")]
        [SerializeField] private Transform target;
        [Tooltip("기본 카메라 설정")]
        [SerializeField] private CameraData cameraData;
        [Tooltip("조준 시 숄더뷰 프로필 (비우면 조준 전환 없음)")]
        [SerializeField] private CameraData aimCameraData;
        [Tooltip("숄더뷰 전환 속도 (초당 비율, 6이면 약 0.17초)")]
        [SerializeField, Min(0f)] private float aimBlendSpeed = 6f;
        [Tooltip("시점 회전 입력 (Player/Look)")]
        [SerializeField] private InputActionReference lookAction;
        [Tooltip("줌 입력 (Player/Zoom, 마우스 휠)")]
        [SerializeField] private InputActionReference zoomAction;

        private Transform cachedTransform;
        private Camera cachedCamera;
        private Vector3 focusPosition;
        private Vector3 focusVelocity;
        private float yaw;
        private float pitch;
        private float collisionPullDistance;
        private float aimBlend;
        private float targetAimBlend;
        private float zoomDistance;
        private float targetZoomDistance;
        private bool isLookEnabled = true;

        private void Awake()
        {
            cachedTransform = transform;
            cachedCamera = GetComponent<Camera>();
            yaw = cachedTransform.eulerAngles.y;
            pitch = cameraData.DefaultPitch;
            zoomDistance = Mathf.Clamp(cameraData.Distance, cameraData.MinZoomDistance, cameraData.MaxZoomDistance);
            targetZoomDistance = zoomDistance;
            if (target != null) focusPosition = GetTargetFocus();
        }

        private void OnEnable()
        {
            lookAction.action.Enable();
            if (zoomAction != null) zoomAction.action.Enable();
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

            if (isLookEnabled)
            {
                UpdateRotation(deltaTime);
                UpdateZoom(deltaTime);
            }
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

        // 메뉴 등에서 시점 회전 정지 + 커서 표시
        public void SetLookEnabled(bool isEnabled)
        {
            isLookEnabled = isEnabled;
            if (cameraData.LockCursor) SetCursorLocked(isEnabled);
        }

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

        // 휠 위 = 당기기, 아래 = 밀기 (한 칸당 zoomStep, 부드럽게 따라감)
        private void UpdateZoom(float deltaTime)
        {
            if (zoomAction != null)
            {
                float scroll = zoomAction.action.ReadValue<float>();
                if (!Mathf.Approximately(scroll, NoScroll))
                {
                    targetZoomDistance = Mathf.Clamp(targetZoomDistance - Mathf.Sign(scroll) * cameraData.ZoomStep,
                        cameraData.MinZoomDistance, cameraData.MaxZoomDistance);
                }
            }

            zoomDistance = Mathf.MoveTowards(zoomDistance, targetZoomDistance, cameraData.ZoomSpeed * deltaTime);
        }

        private void UpdatePosition(float deltaTime)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, RollDegrees);
            Vector3 backward = rotation * Vector3.back;
            // 기본 프로필 거리는 휠 줌 값, 조준 프로필은 고정 거리
            float desiredDistance = aimCameraData != null ? Mathf.Lerp(zoomDistance, aimCameraData.Distance, aimBlend) : zoomDistance;
            float collisionPull = desiredDistance - ResolveCollisionDistance(backward, desiredDistance);

            // 벽에 밀린 양만 보간 (밀릴 땐 즉시, 벗어날 땐 부드럽게), 조준·줌 거리 변화는 그대로 따라감
            collisionPullDistance = collisionPull > collisionPullDistance
                ? collisionPull
                : Mathf.MoveTowards(collisionPullDistance, collisionPull, cameraData.DistanceRecoverSpeed * deltaTime);

            float distance = Mathf.Max(cameraData.MinDistance, desiredDistance - collisionPullDistance);
            cachedTransform.SetPositionAndRotation(focusPosition + backward * distance, rotation);
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

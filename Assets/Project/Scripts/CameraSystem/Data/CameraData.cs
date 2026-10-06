using UnityEngine;

namespace ProjectFantasy.CameraSystem
{
    // 3인칭 카메라 튜닝 데이터
    [CreateAssetMenu(fileName = "CameraData", menuName = "ProjectFantasy/Camera/Third Person Camera Data")]
    public sealed class CameraData : ScriptableObject
    {
        [Header("Sensitivity")]
        [Tooltip("마우스 감도 (프레임 이동량 배율, 0.05~0.2)")]
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.1f;
        [Tooltip("패드 감도 (초당 회전 각도, 120~240)")]
        [SerializeField, Min(0f)] private float gamepadSensitivity = 180f;
        [Tooltip("상하 회전 반전")]
        [SerializeField] private bool invertY;

        [Header("Pitch")]
        [Tooltip("시작 시 상하 각도 (도, 양수 = 내려다봄)")]
        [SerializeField] private float defaultPitch = 15f;
        [Tooltip("올려다볼 수 있는 한계 각도 (도, 음수)")]
        [SerializeField] private float minPitch = -30f;
        [Tooltip("내려다볼 수 있는 한계 각도 (도)")]
        [SerializeField] private float maxPitch = 70f;

        [Header("Follow")]
        [Tooltip("캐릭터와의 거리 (m)")]
        [SerializeField, Min(0f)] private float distance = 5f;
        [Tooltip("바라보는 지점 높이 (발 기준 m)")]
        [SerializeField, Min(0f)] private float targetHeight = 1.5f;
        [Tooltip("추적 지연 (초, 0이면 즉시)")]
        [SerializeField, Min(0f)] private float followSmoothTime = 0.05f;
        [Tooltip("카메라 오른쪽(+)/왼쪽(-) 어깨 오프셋")]
        [SerializeField] private float shoulderOffset;
        [Tooltip("시야각 (도)")]
        [SerializeField, Range(1f, 179f)] private float fieldOfView = 60f;

        [Header("Zoom (휠, 기본 프로필만)")]
        [Tooltip("휠로 당길 수 있는 최소 거리 (m)")]
        [SerializeField, Min(0f)] private float minZoomDistance = 3f;
        [Tooltip("휠로 밀 수 있는 최대 거리 (m)")]
        [SerializeField, Min(0f)] private float maxZoomDistance = 12f;
        [Tooltip("휠 한 칸당 거리 변화 (m)")]
        [SerializeField, Min(0f)] private float zoomStep = 1f;
        [Tooltip("목표 거리까지 따라가는 속도 (m/s)")]
        [SerializeField, Min(0f)] private float zoomSpeed = 15f;

        [Header("Collision")]
        [Tooltip("카메라가 막히는 레이어 (Player·Projectile 제외)")]
        [SerializeField] private LayerMask collisionLayers = Physics.DefaultRaycastLayers;
        [Tooltip("벽 충돌 검사 구체 반지름 (m)")]
        [SerializeField, Min(0f)] private float collisionRadius = 0.25f;
        [Tooltip("벽에 막혔을 때 최소 거리 (m)")]
        [SerializeField, Min(0f)] private float minDistance = 0.5f;
        [Tooltip("벽에서 벗어날 때 거리 복귀 속도 (m/s)")]
        [SerializeField, Min(0f)] private float distanceRecoverSpeed = 8f;

        [Header("Cursor")]
        [Tooltip("플레이 중 커서 숨김·고정 (메뉴 열면 해제)")]
        [SerializeField] private bool lockCursor = true;

        public float MouseSensitivity => mouseSensitivity;
        public float GamepadSensitivity => gamepadSensitivity;
        public bool InvertY => invertY;
        public float DefaultPitch => defaultPitch;
        public float MinPitch => minPitch;
        public float MaxPitch => maxPitch;
        public float Distance => distance;
        public float TargetHeight => targetHeight;
        public float FollowSmoothTime => followSmoothTime;
        public float ShoulderOffset => shoulderOffset;
        public float FieldOfView => fieldOfView;
        public float MinZoomDistance => minZoomDistance;
        public float MaxZoomDistance => Mathf.Max(minZoomDistance, maxZoomDistance);
        public float ZoomStep => zoomStep;
        public float ZoomSpeed => zoomSpeed;
        public LayerMask CollisionLayers => collisionLayers;
        public float CollisionRadius => collisionRadius;
        public float MinDistance => minDistance;
        public float DistanceRecoverSpeed => distanceRecoverSpeed;
        public bool LockCursor => lockCursor;
    }
}

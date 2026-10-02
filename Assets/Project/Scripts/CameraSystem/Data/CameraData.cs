using UnityEngine;

namespace ProjectFantasy.CameraSystem
{
    // 3인칭 카메라 튜닝 데이터
    [CreateAssetMenu(fileName = "CameraData", menuName = "ProjectFantasy/Camera/Third Person Camera Data")]
    public sealed class CameraData : ScriptableObject
    {
        [Header("Sensitivity")]
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.1f;
        [SerializeField, Min(0f)] private float gamepadSensitivity = 180f;
        [SerializeField] private bool invertY;

        [Header("Pitch")]
        [SerializeField] private float defaultPitch = 15f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 70f;

        [Header("Follow")]
        [SerializeField, Min(0f)] private float distance = 5f;
        [SerializeField, Min(0f)] private float targetHeight = 1.5f;
        [SerializeField, Min(0f)] private float followSmoothTime = 0.05f;
        [Tooltip("카메라 오른쪽(+)/왼쪽(-) 어깨 오프셋")]
        [SerializeField] private float shoulderOffset;
        [SerializeField, Range(1f, 179f)] private float fieldOfView = 60f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionLayers = Physics.DefaultRaycastLayers;
        [SerializeField, Min(0f)] private float collisionRadius = 0.25f;
        [SerializeField, Min(0f)] private float minDistance = 0.5f;
        [SerializeField, Min(0f)] private float distanceRecoverSpeed = 8f;

        [Header("Cursor")]
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
        public LayerMask CollisionLayers => collisionLayers;
        public float CollisionRadius => collisionRadius;
        public float MinDistance => minDistance;
        public float DistanceRecoverSpeed => distanceRecoverSpeed;
        public bool LockCursor => lockCursor;
    }
}

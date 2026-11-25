using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 예상 비행 경로 표시 (점 버퍼 재사용, GC 없음)
    [RequireComponent(typeof(LineRenderer))]
    [DisallowMultipleComponent]
    public sealed class TrajectoryPreview : MonoBehaviour
    {
        private const float FrustumHeightFactor = 2f;
        private const float HalfAngleFactor = 0.5f;
        [Tooltip("화면 높이 대비 마커 크기 (0.02 = 화면 높이의 2%)")]
        [SerializeField, Range(0f, 1f)] private float screenSize = 0.02f;

        [Tooltip("경로선 최대 점 수")]
        [SerializeField, Min(2)] private int maxPoints = 64;
        [SerializeField] private Transform impactMarker;
        [Tooltip("마커가 바라볼 카메라 (비우면 Main Camera)")]
        [SerializeField] private Camera viewCamera;
        [Tooltip("마커를 표면에서 카메라 쪽으로 당기는 거리 (m, 마커 반지름 이상)")]
        [SerializeField, Min(0f)] private float surfaceOffset = 0.2f;
        private LineRenderer lineRenderer;
        private Vector3[] points;

        public Vector3[] Buffer => points;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            points = new Vector3[maxPoints];
            Hide();
            if (viewCamera == null) viewCamera = Camera.main;
        }

        public void Show(int pointCount)
        {
            lineRenderer.positionCount = pointCount;
            lineRenderer.SetPositions(points);
            lineRenderer.enabled = true;

            if (!impactMarker.gameObject.activeSelf) impactMarker.gameObject.SetActive(true);
            Vector3 impactPoint = points[pointCount - 1];
            Vector3 toCamera = (viewCamera.transform.position - impactPoint).normalized;
            impactMarker.SetPositionAndRotation(impactPoint + toCamera * surfaceOffset, viewCamera.transform.rotation);

            Vector3 markerPosition = impactMarker.position;
            float depth = Vector3.Dot(markerPosition - viewCamera.transform.position, viewCamera.transform.forward);
            float frustumHeight = FrustumHeightFactor * depth * Mathf.Tan(viewCamera.fieldOfView * HalfAngleFactor * Mathf.Deg2Rad);
            impactMarker.localScale = Vector3.one * (frustumHeight * screenSize);
        }

        public void Hide()
        {
            lineRenderer.enabled = false;
            if (impactMarker.gameObject.activeSelf) impactMarker.gameObject.SetActive(false);
        }
    }
}

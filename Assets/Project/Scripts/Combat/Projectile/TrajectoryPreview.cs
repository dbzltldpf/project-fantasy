using UnityEngine;

namespace ProjectFantasy.Combat
{
    // 예상 비행 경로 표시 (점 버퍼 재사용, GC 없음)
    [RequireComponent(typeof(LineRenderer))]
    [DisallowMultipleComponent]
    public sealed class TrajectoryPreview : MonoBehaviour
    {
        [Tooltip("경로선 최대 점 수")]
        [SerializeField, Min(2)] private int maxPoints = 64;

        private LineRenderer lineRenderer;
        private Vector3[] points;

        public Vector3[] Buffer => points;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            points = new Vector3[maxPoints];
            Hide();
        }

        public void Show(int pointCount)
        {
            lineRenderer.positionCount = pointCount;
            lineRenderer.SetPositions(points);
            lineRenderer.enabled = true;
        }

        public void Hide() => lineRenderer.enabled = false;
    }
}

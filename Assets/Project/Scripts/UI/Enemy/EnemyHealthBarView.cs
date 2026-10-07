using UnityEngine;

namespace ProjectFantasy.UI
{
    // 적 머리 위 월드 공간 체력바 표시 (언제 보일지는 EnemyHealthBarPresenter가 결정), 보이는 동안 카메라를 바라봄
    [DisallowMultipleComponent]
    public sealed class EnemyHealthBarView : MonoBehaviour
    {
        [Tooltip("켜고 끌 표시 영역")]
        [SerializeField] private GameObject root;
        [Tooltip("채움 마스크 (RectMask2D, 왼쪽 기준, 자식 Fill은 전체 길이 고정)")]
        [SerializeField] private RectTransform fillMask;

        private Transform cameraTransform;
        private float hideTime = float.PositiveInfinity;

        private void Awake()
        {
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        // 보이는 동안만 빌보드·예약 숨김 처리
        private void LateUpdate()
        {
            if (!root.activeSelf) return;

            if (Time.time >= hideTime)
            {
                HideImmediate();
                return;
            }

            if (cameraTransform != null) transform.rotation = cameraTransform.rotation;
        }

        public bool IsVisible => root.activeSelf;

        public void SetRatio(float ratio) => BarFill.SetMaskedRatio(fillMask, ratio);

        // 표시 (예약된 숨김 취소)
        public void Show()
        {
            hideTime = float.PositiveInfinity;
            root.SetActive(true);
        }

        public void HideAfter(float delay)
        {
            if (root.activeSelf) hideTime = Time.time + delay;
        }

        public void HideImmediate()
        {
            hideTime = float.PositiveInfinity;
            root.SetActive(false);
        }
    }
}

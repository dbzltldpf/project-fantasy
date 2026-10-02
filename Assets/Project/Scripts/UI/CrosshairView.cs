using UnityEngine;

namespace ProjectFantasy.UI
{
    // 화면 중앙 조준점 표시/숨김 (조준 모드에서 PlayerAimPresenter가 표시)
    [DisallowMultipleComponent]
    public sealed class CrosshairView : MonoBehaviour
    {
        [SerializeField] private GameObject crosshairRoot;

        private void Awake()
        {
            if (crosshairRoot == null) crosshairRoot = gameObject;
            SetVisible(false);
        }

        public void SetVisible(bool isVisible)
        {
            if (crosshairRoot.activeSelf != isVisible) crosshairRoot.SetActive(isVisible);
        }
    }
}

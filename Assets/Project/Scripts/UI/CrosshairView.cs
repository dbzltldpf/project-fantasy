using UnityEngine;
using UnityEngine.UI;

namespace ProjectFantasy.UI
{
    // 화면 중앙 조준점: 평소엔 점, 조준 모드엔 조준점 (PlayerAimPresenter가 전환)
    [DisallowMultipleComponent]
    public sealed class CrosshairView : MonoBehaviour
    {
        private const float FullRatio = 1f;
        [SerializeField] private GameObject aimRoot;
        [SerializeField] private GameObject defaultRoot;
        [SerializeField] private Image drawGauge;
        [SerializeField] private Color drawingColor, fullColor;

        private void Awake()
        {
            SetAiming(false);
        }

        public void SetAiming(bool isAiming)
        {
            defaultRoot.SetActive(!isAiming);
            aimRoot.SetActive(isAiming);
        }
        public void SetDraw(bool isVisible, float ratio)
        {
            if (drawGauge.gameObject.activeSelf != isVisible) drawGauge.gameObject.SetActive(isVisible);
            drawGauge.fillAmount = ratio;
            drawGauge.color = ratio >= FullRatio ? fullColor : drawingColor;
        }
    }
}

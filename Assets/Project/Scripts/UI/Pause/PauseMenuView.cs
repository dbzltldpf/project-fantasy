using UnityEngine;
namespace ProjectFantasy.UI
{
    // 일시정지 메뉴 표시, 테스트 패널 표시 여부 (일시정지 로직은 모름)   
    [DisallowMultipleComponent]
    public sealed class PauseMenuView : MonoBehaviour
    {
        [Tooltip("일시정지 패널 루트 (켜고 끄는 대상)")]
        [SerializeField] private GameObject root;
        [Tooltip("테스트 버튼 패널")]
        [SerializeField] private GameObject devPanel;
        [Tooltip("테스트 패널 표시")]
        [SerializeField] private bool showDevPanel = true;

        public bool IsOpen => root.activeSelf;
        private void Awake()
        {
            root.SetActive(false);
            devPanel.SetActive(showDevPanel);
        }
        public void Show()
        {
            root.SetActive(true);
        }
        public void Hide()
        {
            root.SetActive(false);
        }
    }
}

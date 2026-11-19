using System;
using UnityEngine;
using UnityEngine.UI;
namespace ProjectFantasy.UI
{
    // 타이틀 화면 표시, 시작·종료 버튼 클릭 알림 (게임 흐름은 모름)
    [DisallowMultipleComponent]
    public sealed class TitleView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        public event Action StartClicked;
        public event Action QuitClicked;
        private void Awake()
        {
            startButton.onClick.AddListener(() => StartClicked?.Invoke());
            quitButton.onClick.AddListener(() => QuitClicked?.Invoke());
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

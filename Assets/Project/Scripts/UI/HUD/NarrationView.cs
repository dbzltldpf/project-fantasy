using TMPro;
using UnityEngine;
namespace ProjectFantasy.UI
{
    // 내레이션 자막 표시 (재생 순서·시간은 모름)
    [DisallowMultipleComponent]
    public sealed class NarrationView : MonoBehaviour
    {
        [Tooltip("자막 패널 루트 (켜고 끄는 대상)")]
        [SerializeField] private GameObject root;
        [Tooltip("자막 문구 텍스트")]
        [SerializeField] private TMP_Text label;
        private void Awake()
        {
            root.SetActive(false);
        }
        public void Show(string line)
        {
            root.SetActive(true);
            label.text = line;
        }
        public void Hide()
        {
            root.SetActive(false);
        }

    }
}

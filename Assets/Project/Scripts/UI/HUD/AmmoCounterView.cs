using TMPro;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // 원거리 무기 장착 중 화살 수와 석궁 장전 상태 표시
    [DisallowMultipleComponent]
    public sealed class AmmoCounterView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text countLabel;
        [Tooltip("석궁 장전 표시 (활은 숨김)")]
        [SerializeField] private GameObject loadedMark;

        private void Awake()
        {
            if (root == null) root = gameObject;
            Hide();
        }

        public void Show(int count, bool showLoaded, bool isLoaded)
        {
            root.SetActive(true);
            countLabel.SetText("{0}", count);
            if (loadedMark != null) loadedMark.SetActive(showLoaded && isLoaded);
        }

        public void Hide() => root.SetActive(false);
    }
}

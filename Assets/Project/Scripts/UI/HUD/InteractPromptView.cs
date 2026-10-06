using TMPro;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // 줍기 안내 ("E 줍기 — 한손검")
    [DisallowMultipleComponent]
    public sealed class InteractPromptView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;
        [Tooltip("{0} = 아이템 이름, {1} = 수량")]
        [SerializeField] private string format = "E 줍기 — {0} x{1}";

        private void Awake()
        {
            if (root == null) root = gameObject;
            Hide();
        }

        public void Show(string itemName, int count)
        {
            root.SetActive(true);
            label.text = string.Format(format, itemName, count);
        }

        public void Hide() => root.SetActive(false);
    }
}

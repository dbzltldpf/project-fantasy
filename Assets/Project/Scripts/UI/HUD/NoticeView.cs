using ProjectFantasy.Items;
using TMPro;
using UnityEngine;

namespace ProjectFantasy.UI
{
    // 짧은 안내 문구 (일정 시간 후 자동 숨김)
    [DisallowMultipleComponent]
    public sealed class NoticeView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text label;
        [SerializeField, Min(0f)] private float displayDuration = 1.5f;

        [Header("Messages")]
        [SerializeField] private string inventoryFullMessage = "가방이 가득 찼습니다";
        [SerializeField] private string cannotUseNowMessage = "지금은 사용할 수 없습니다";
        [SerializeField] private string offHandIncompatibleMessage = "현재 무기와 함께 장착할 수 없습니다";
        [SerializeField] private string offHandDisabledMessage = "보조 장비가 비활성화되었습니다";

        private float hideTime;

        private void Awake()
        {
            if (root == null) root = gameObject;
            root.SetActive(false);
        }

        private void Update()
        {
            if (root.activeSelf && Time.time >= hideTime) root.SetActive(false);
        }

        public void Show(ItemNotice notice)
        {
            label.text = GetMessage(notice);
            root.SetActive(true);
            hideTime = Time.time + displayDuration;
        }

        private string GetMessage(ItemNotice notice)
        {
            switch (notice)
            {
                case ItemNotice.InventoryFull: return inventoryFullMessage;
                case ItemNotice.OffHandIncompatible: return offHandIncompatibleMessage;
                case ItemNotice.OffHandDisabled: return offHandDisabledMessage;
                default: return cannotUseNowMessage;
            }
        }
    }
}

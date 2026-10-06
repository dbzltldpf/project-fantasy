using ProjectFantasy.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectFantasy.UI
{
    // 드래그 중 포인터를 따라가는 아이템 표시 (레이캐스트를 막지 않도록 raycastTarget 끔)
    [DisallowMultipleComponent]
    public sealed class ItemDragGhost : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameLabel;

        private void Awake()
        {
            if (root == null) root = (RectTransform)transform;
            Hide();
        }

        // 칸의 드래그 이벤트에 연결 (가방·퀵슬롯 공용)
        public void Register(ItemSlotView slot)
        {
            slot.DragBegan += HandleDragBegan;
            slot.Dragging += HandleDragging;
            slot.DragEnded += HandleDragEnded;
        }

        public void Show(ItemData item, Vector2 screenPosition)
        {
            bool hasIcon = item.Icon != null;
            icon.enabled = hasIcon;
            if (hasIcon) icon.sprite = item.Icon;
            if (nameLabel != null) nameLabel.text = hasIcon ? string.Empty : item.DisplayName;

            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            Move(screenPosition);
        }

        // Screen Space Overlay 캔버스 기준
        public void Move(Vector2 screenPosition) => root.position = screenPosition;

        public void Hide() => root.gameObject.SetActive(false);

        private void HandleDragBegan(ItemSlotView slot, PointerEventData eventData) => Show(slot.Item, eventData.position);
        private void HandleDragging(ItemSlotView _, PointerEventData eventData) => Move(eventData.position);
        private void HandleDragEnded(ItemSlotView _, PointerEventData __) => Hide();
    }
}

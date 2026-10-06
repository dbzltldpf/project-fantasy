using System;
using ProjectFantasy.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjectFantasy.UI
{
    // 아이템 칸 하나 (아이콘 또는 이름, 수량, 장착·선택 표시), 포인터 입력을 이벤트로 전달
    [DisallowMultipleComponent]
    public sealed class ItemSlotView : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        private const int DoubleClickCount = 2;
        private const float VisibleAlpha = 1f;

        [SerializeField] private Image icon;
        [Tooltip("아이콘이 없는 아이템은 이름 표시")]
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text countLabel;
        [SerializeField] private TMP_Text keyLabel;
        [SerializeField] private GameObject equippedMark;
        [SerializeField] private GameObject selectedFrame;
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("소모품을 다 쓴 퀵슬롯 등 비활성 표시 알파")]
        [SerializeField, Range(0f, 1f)] private float dimmedAlpha = 0.4f;

        public int Index { get; private set; }
        public ItemData Item { get; private set; }
        public object Owner { get; private set; }

        public event Action<ItemSlotView> Clicked;
        public event Action<ItemSlotView> DoubleClicked;
        public event Action<ItemSlotView, Vector2> RightClicked;
        public event Action<ItemSlotView, PointerEventData> DragBegan;
        public event Action<ItemSlotView, PointerEventData> Dragging;
        public event Action<ItemSlotView, PointerEventData> DragEnded;
        // (드롭 받은 칸, 끌고 온 칸)
        public event Action<ItemSlotView, ItemSlotView> Dropped;

        public void Initialize(object owner, int index)
        {
            Owner = owner;
            Index = index;
        }

        // 수량은 중첩 가능한 아이템만 표시
        public void SetItem(ItemData item, int count)
        {
            Item = item;
            bool hasItem = item != null;
            bool hasIcon = hasItem && item.Icon != null;

            icon.enabled = hasIcon;
            if (hasIcon) icon.sprite = item.Icon;

            if (nameLabel != null) nameLabel.text = hasItem && !hasIcon ? item.DisplayName : string.Empty;
            if (countLabel != null)
            {
                bool showCount = hasItem && item.IsStackable;
                countLabel.enabled = showCount;
                if (showCount) countLabel.SetText("{0}", count);
            }
        }

        // 등급 색 (아이콘이 없어 이름을 보여줄 때)
        public void SetNameColor(Color color)
        {
            if (nameLabel != null) nameLabel.color = color;
        }

        public void SetKeyLabel(string key)
        {
            if (keyLabel != null) keyLabel.text = key;
        }

        public void SetEquipped(bool isEquipped) => SetActive(equippedMark, isEquipped);
        public void SetSelected(bool isSelected) => SetActive(selectedFrame, isSelected);

        public void SetDimmed(bool isDimmed)
        {
            if (canvasGroup != null) canvasGroup.alpha = isDimmed ? dimmedAlpha : VisibleAlpha;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                RightClicked?.Invoke(this, eventData.position);
                return;
            }

            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (eventData.clickCount == DoubleClickCount) DoubleClicked?.Invoke(this);
            else Clicked?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Item != null && eventData.button == PointerEventData.InputButton.Left) DragBegan?.Invoke(this, eventData);
        }

        public void OnDrag(PointerEventData eventData) => Dragging?.Invoke(this, eventData);
        public void OnEndDrag(PointerEventData eventData) => DragEnded?.Invoke(this, eventData);

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;

            ItemSlotView source = eventData.pointerDrag.GetComponent<ItemSlotView>();
            if (source != null && source != this && source.Item != null) Dropped?.Invoke(this, source);
        }

        private static void SetActive(GameObject target, bool isActive)
        {
            if (target != null && target.activeSelf != isActive) target.SetActive(isActive);
        }
    }
}

using System.Collections.Generic;
using ProjectFantasy.InventorySystem;
using ProjectFantasy.Items;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectFantasy.UI
{
    // 하단 퀵슬롯 바: 등록 개체·소모품 수량·장착 표시, 드롭으로 등록, 칸끼리 이동, 바깥으로 끌면 해제, 클릭으로 사용
    [DisallowMultipleComponent]
    public sealed class QuickSlotBarView : MonoBehaviour
    {
        private const int EmptyCount = 0;
        private const int EquipmentCount = 1;
        private const int KeyNumberOffset = 1;

        [SerializeField] private RectTransform slotContainer;
        [SerializeField] private ItemSlotView slotPrefab;
        [SerializeField] private ItemDragGhost dragGhost;
        [Tooltip("등급이 없는 아이템 이름 색")]
        [SerializeField] private Color defaultNameColor = Color.white;

        private readonly List<ItemSlotView> slotViews = new List<ItemSlotView>();
        private QuickSlots quickSlots;
        private Inventory inventory;
        private IItemActions itemActions;

        private void OnDestroy()
        {
            if (quickSlots != null) quickSlots.Changed -= Refresh;
            if (inventory != null) inventory.Changed -= Refresh;
            if (itemActions != null) itemActions.EquipmentChanged -= Refresh;
        }

        // 플레이어 쪽 Presenter가 시작 시 1회 연결
        public void Bind(QuickSlots targetQuickSlots, Inventory targetInventory, IItemActions actions)
        {
            quickSlots = targetQuickSlots;
            inventory = targetInventory;
            itemActions = actions;

            quickSlots.Changed += Refresh;
            inventory.Changed += Refresh;
            itemActions.EquipmentChanged += Refresh;

            CreateSlotViews();
            Refresh();
        }

        private void CreateSlotViews()
        {
            for (int i = 0; i < quickSlots.Count; i++)
            {
                ItemSlotView view = Instantiate(slotPrefab, slotContainer);
                view.Initialize(this, i);
                view.SetKeyLabel((i + KeyNumberOffset).ToString());
                view.Clicked += HandleClicked;
                view.Dropped += HandleDropped;
                view.DragEnded += HandleDragEnded;
                dragGhost.Register(view);
                slotViews.Add(view);
            }
        }

        private void Refresh()
        {
            for (int i = 0; i < slotViews.Count; i++)
            {
                QuickSlotEntry entry = quickSlots.Get(i);
                ItemSlotView view = slotViews[i];
                bool isInstance = entry.Instance != null;
                int count = isInstance ? EquipmentCount : inventory.GetCount(entry.Item);

                view.SetItem(entry.Item, count);
                view.SetNameColor(isInstance && entry.Instance.Grade != null ? entry.Instance.Grade.Color : defaultNameColor);
                view.SetEquipped(itemActions.IsEquipped(entry.Instance));
                view.SetDimmed(!entry.IsEmpty && count <= EmptyCount);
            }
        }

        private void HandleClicked(ItemSlotView view) => itemActions.TryActivateQuickSlot(view.Index);

        // 퀵슬롯끼리는 교환, 가방 칸(번호 = 가방 인덱스)은 등록 (같은 개체·종류는 이동)
        private void HandleDropped(ItemSlotView target, ItemSlotView source)
        {
            if (source.Owner == (object)this) quickSlots.Swap(source.Index, target.Index);
            else if (source.Index >= 0 && source.Index < inventory.Capacity) quickSlots.Assign(target.Index, inventory.GetSlot(source.Index));
        }

        // 다른 칸이 아닌 곳에 놓으면 해제
        private void HandleDragEnded(ItemSlotView view, PointerEventData eventData)
        {
            GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
            if (dropTarget == null || dropTarget.GetComponentInParent<ItemSlotView>() == null) quickSlots.Clear(view.Index);
        }
    }
}

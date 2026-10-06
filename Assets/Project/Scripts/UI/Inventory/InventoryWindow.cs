using System;
using System.Collections.Generic;
using ProjectFantasy.InventorySystem;
using ProjectFantasy.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFantasy.UI
{
    // 가방 창: 필터, 장비 칸, 상세(등급·능력치·장착 대비 차이), 더블클릭 장착·사용, 우클릭 메뉴, 칸 교환 (실시간)
    [DisallowMultipleComponent]
    public sealed class InventoryWindow : MonoBehaviour
    {
        private const int NoSelection = -1;
        private const int EquipmentSlotIndex = -1;
        private const int EquipmentCount = 1;

        public enum Filter
        {
            All,
            Weapon,
            OffHand,
            Ammo,
            Consumable
        }

        [Serializable]
        private struct FilterButton
        {
            [SerializeField] private Button button;
            [SerializeField] private Filter filter;

            public Button Button => button;
            public Filter Filter => filter;
        }

        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform slotContainer;
        [SerializeField] private ItemSlotView slotPrefab;
        [SerializeField] private FilterButton[] filterButtons = Array.Empty<FilterButton>();
        [Tooltip("등급이 없는 아이템 이름 색")]
        [SerializeField] private Color defaultNameColor = Color.white;

        [Header("Equipment")]
        [SerializeField] private ItemSlotView weaponSlot;
        [SerializeField] private ItemSlotView offHandSlot;

        [Header("Detail")]
        [SerializeField] private TMP_Text detailName;
        [SerializeField] private TMP_Text detailStats;
        [SerializeField] private TMP_Text detailDescription;
        [SerializeField] private ItemStatFormatter statFormatter = new ItemStatFormatter();

        [Header("Shared")]
        [SerializeField] private ItemContextMenu contextMenu;
        [SerializeField] private ItemDragGhost dragGhost;

        private readonly List<ItemSlotView> slotViews = new List<ItemSlotView>();
        private Inventory inventory;
        private IItemActions itemActions;
        private Filter currentFilter = Filter.All;
        private int selectedIndex = NoSelection;
        private int menuTargetIndex = NoSelection;

        public bool IsOpen => root.activeSelf;

        public event Action<bool> OpenChanged;

        private void Awake()
        {
            foreach (FilterButton filterButton in filterButtons)
            {
                Filter filter = filterButton.Filter;
                filterButton.Button.onClick.AddListener(() => SetFilter(filter));
            }

            InitializeEquipmentSlot(weaponSlot);
            InitializeEquipmentSlot(offHandSlot);
            contextMenu.ActionChosen += HandleMenuAction;
            root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (inventory != null) inventory.Changed -= Refresh;
            if (itemActions != null) itemActions.EquipmentChanged -= Refresh;
            contextMenu.ActionChosen -= HandleMenuAction;
        }

        // 플레이어 쪽 Presenter가 시작 시 1회 연결
        public void Bind(Inventory targetInventory, IItemActions actions)
        {
            inventory = targetInventory;
            itemActions = actions;
            inventory.Changed += Refresh;
            itemActions.EquipmentChanged += Refresh;

            CreateSlotViews();
            Refresh();
        }

        public void Toggle() => SetOpen(!IsOpen);

        public void SetOpen(bool isOpen)
        {
            if (IsOpen == isOpen) return;

            root.SetActive(isOpen);
            if (!isOpen)
            {
                contextMenu.Hide();
                dragGhost.Hide();
            }
            else
            {
                Refresh();
            }

            OpenChanged?.Invoke(isOpen);
        }

        private void CreateSlotViews()
        {
            for (int i = 0; i < inventory.Capacity; i++)
            {
                ItemSlotView view = Instantiate(slotPrefab, slotContainer);
                view.Initialize(this, i);
                view.Clicked += HandleSlotClicked;
                view.DoubleClicked += HandleSlotDoubleClicked;
                view.RightClicked += HandleSlotRightClicked;
                view.Dropped += HandleSlotDropped;
                dragGhost.Register(view);
                slotViews.Add(view);
            }
        }

        private void InitializeEquipmentSlot(ItemSlotView slot)
        {
            slot.Initialize(this, EquipmentSlotIndex);
            slot.DoubleClicked += HandleEquipmentDoubleClicked;
        }

        private void Refresh()
        {
            if (inventory == null || !IsOpen) return;

            for (int i = 0; i < slotViews.Count; i++)
            {
                ItemStack stack = inventory.GetSlot(i);
                ItemSlotView view = slotViews[i];
                bool isVisible = MatchesFilter(stack);

                view.gameObject.SetActive(isVisible);
                if (!isVisible) continue;

                view.SetItem(stack.Item, stack.Count);
                view.SetNameColor(GetNameColor(stack.Instance));
                view.SetEquipped(itemActions.IsEquipped(stack.Instance));
                view.SetSelected(i == selectedIndex);
            }

            RefreshEquipmentSlot(weaponSlot, itemActions.EquippedWeapon, true);
            RefreshEquipmentSlot(offHandSlot, itemActions.EquippedOffHand, itemActions.IsOffHandActive);
            RefreshDetail();
        }

        // 무기와 맞지 않아 비활성인 보조 장비는 흐리게
        private void RefreshEquipmentSlot(ItemSlotView slot, ItemInstance instance, bool isActive)
        {
            slot.SetItem(instance?.Data, EquipmentCount);
            slot.SetNameColor(GetNameColor(instance));
            slot.SetDimmed(instance != null && !isActive);
        }

        // 전체 필터만 빈 칸 표시
        private bool MatchesFilter(ItemStack stack)
        {
            if (currentFilter == Filter.All) return true;
            if (stack.IsEmpty) return false;

            switch (currentFilter)
            {
                case Filter.Weapon: return stack.Item.Category == ItemCategory.Weapon;
                case Filter.OffHand: return stack.Item.Category == ItemCategory.OffHand;
                case Filter.Ammo: return stack.Item.Category == ItemCategory.Ammo;
                case Filter.Consumable: return stack.Item.Category == ItemCategory.Consumable;
                default: return true;
            }
        }

        private void RefreshDetail()
        {
            ItemStack stack = selectedIndex != NoSelection ? inventory.GetSlot(selectedIndex) : ItemStack.Empty;
            if (stack.IsEmpty)
            {
                detailName.text = string.Empty;
                detailStats.text = string.Empty;
                detailDescription.text = string.Empty;
                return;
            }

            ItemInstance instance = stack.Instance;
            string itemName = stack.Item.DisplayName;
            detailName.text = instance?.Grade != null
                ? ItemStatFormatter.ToColorTag($"{itemName}  [{instance.Grade.DisplayName}]", instance.Grade.Color)
                : itemName;
            detailStats.text = instance != null ? statFormatter.Format(instance, GetComparisonStats(instance)) : string.Empty;
            detailDescription.text = stack.Item.Description;
        }

        // 같은 부위 장착 장비 대비 (자신이 장착 중이면 비교 없음, 비어 있으면 0 기준)
        private ItemStats? GetComparisonStats(ItemInstance instance)
        {
            if (itemActions.IsEquipped(instance)) return null;

            ItemInstance equipped = instance.Data.Category == ItemCategory.OffHand ? itemActions.EquippedOffHand
                : instance.Data.Category == ItemCategory.Weapon ? itemActions.EquippedWeapon
                : null;
            return equipped != null ? equipped.Stats : ItemStats.Zero;
        }

        private Color GetNameColor(ItemInstance instance) => instance?.Grade != null ? instance.Grade.Color : defaultNameColor;

        private void SetFilter(Filter filter)
        {
            currentFilter = filter;
            Refresh();
        }

        private void HandleSlotClicked(ItemSlotView view)
        {
            selectedIndex = view.Item != null ? view.Index : NoSelection;
            Refresh();
        }

        // 장착 중인 개체면 해제, 아니면 장착·사용
        private void HandleSlotDoubleClicked(ItemSlotView view)
        {
            ItemInstance instance = inventory.GetSlot(view.Index).Instance;
            if (itemActions.IsEquipped(instance)) itemActions.Unequip(instance);
            else itemActions.TryActivateSlot(view.Index);
        }

        private void HandleEquipmentDoubleClicked(ItemSlotView view)
        {
            ItemInstance instance = view == weaponSlot ? itemActions.EquippedWeapon : itemActions.EquippedOffHand;
            itemActions.Unequip(instance);
        }

        private void HandleSlotRightClicked(ItemSlotView view, Vector2 screenPosition)
        {
            ItemStack stack = inventory.GetSlot(view.Index);
            if (stack.IsEmpty) return;

            menuTargetIndex = view.Index;
            bool isEquipped = itemActions.IsEquipped(stack.Instance);
            bool canEquip = stack.Instance != null && !isEquipped;
            contextMenu.Show(screenPosition, canEquip, isEquipped, stack.Item.Category == ItemCategory.Consumable);
        }

        private void HandleMenuAction(ItemContextMenu.MenuAction action)
        {
            if (menuTargetIndex == NoSelection) return;

            int index = menuTargetIndex;
            menuTargetIndex = NoSelection;
            if (inventory.GetSlot(index).IsEmpty) return;

            switch (action)
            {
                case ItemContextMenu.MenuAction.Equip:
                case ItemContextMenu.MenuAction.Use:
                    itemActions.TryActivateSlot(index);
                    break;
                case ItemContextMenu.MenuAction.Unequip:
                    itemActions.Unequip(inventory.GetSlot(index).Instance);
                    break;
                case ItemContextMenu.MenuAction.Drop:
                    itemActions.Drop(index);
                    break;
            }
        }

        // 가방 칸끼리만 교환 (퀵슬롯·장비 칸에서 끌어온 경우 무시)
        private void HandleSlotDropped(ItemSlotView target, ItemSlotView source)
        {
            if (source.Owner != (object)this || source.Index == EquipmentSlotIndex) return;

            inventory.Swap(source.Index, target.Index);
            if (selectedIndex == source.Index) selectedIndex = target.Index;
            Refresh();
        }
    }
}

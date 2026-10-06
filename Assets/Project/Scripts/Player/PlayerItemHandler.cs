using System;
using ProjectFantasy.InventorySystem;
using ProjectFantasy.Items;
using ProjectFantasy.Weapon;
using UnityEngine;

namespace ProjectFantasy.Player
{
    // UI·퀵슬롯의 아이템 요청 창구: 개체 단위 장착·해제·사용·버리기·줍기 (장비 변경과 사용은 이동 상태에서만)
    [RequireComponent(typeof(Inventory), typeof(QuickSlots), typeof(PlayerInteractor))]
    [DisallowMultipleComponent]
    public sealed class PlayerItemHandler : MonoBehaviour, IItemActions
    {
        private const int EmptyCount = 0;

        [Header("Drop")]
        [Tooltip("버린 아이템을 담을 필드 아이템 프리팹")]
        [SerializeField] private WorldItem worldItemPrefab;
        [Tooltip("버릴 때 플레이어 앞쪽 거리 (m)")]
        [SerializeField, Min(0f)] private float dropDistance = 1f;
        [Tooltip("버릴 때 높이 (m)")]
        [SerializeField, Min(0f)] private float dropHeight = 0.3f;

        [Header("Pick Up")]
        [Tooltip("줍기 시작부터 가방에 들어가는 시간")]
        [SerializeField, Min(0f)] private float pickUpGrabTime = 0.4f;
        [Tooltip("줍기 모션 전체 시간 (초)")]
        [SerializeField, Min(0f)] private float pickUpDuration = 0.9f;

        private PlayerController controller;
        private PlayerLoadout loadout;

        public Inventory Inventory { get; private set; }
        public QuickSlots QuickSlots { get; private set; }
        public PlayerInteractor Interactor { get; private set; }
        public float PickUpGrabTime => pickUpGrabTime;
        public float PickUpDuration => pickUpDuration;

        public ItemInstance EquippedWeapon => loadout.WeaponInstance;
        public ItemInstance EquippedOffHand => loadout.OffHandInstance;
        public bool IsOffHandActive => loadout.IsOffHandActive;

        private bool CanAct => controller.IsInLocomotion;

        public event Action<ItemNotice> Noticed;
        public event Action EquipmentChanged;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            loadout = GetComponent<PlayerLoadout>();
            Inventory = GetComponent<Inventory>();
            QuickSlots = GetComponent<QuickSlots>();
            Interactor = GetComponent<PlayerInteractor>();
        }

        private void OnEnable()
        {
            loadout.WeaponChanged += HandleWeaponChanged;
            loadout.OffHandChanged += HandleOffHandChanged;
            loadout.OffHandActiveChanged += HandleOffHandActiveChanged;
        }

        private void OnDisable()
        {
            loadout.WeaponChanged -= HandleWeaponChanged;
            loadout.OffHandChanged -= HandleOffHandChanged;
            loadout.OffHandActiveChanged -= HandleOffHandActiveChanged;
        }

        public bool IsEquipped(ItemInstance instance) => loadout.IsEquipped(instance);

        // 퀵슬롯 번호: 장비 개체는 장착, 소모품은 사용
        public bool TryActivateQuickSlot(int quickSlotIndex)
        {
            if (!QuickSlots.IsValid(quickSlotIndex)) return false;

            QuickSlotEntry entry = QuickSlots.Get(quickSlotIndex);
            if (entry.Instance != null) return IsEquipped(entry.Instance) || TryEquip(entry.Instance);
            return entry.Item is ConsumableData consumable && TryUse(consumable);
        }

        // 가방 칸: 장비 개체는 장착, 소모품은 사용
        public bool TryActivateSlot(int slotIndex)
        {
            ItemStack stack = Inventory.GetSlot(slotIndex);
            if (stack.IsEmpty) return false;
            if (stack.Instance != null) return TryEquip(stack.Instance);
            return stack.Item is ConsumableData consumable && TryUse(consumable);
        }

        public bool TryEquip(ItemInstance instance)
        {
            if (instance == null || IsEquipped(instance) || !Inventory.Contains(instance)) return false;

            if (instance.Data is OffHandData offHand && !loadout.CanEquipOffHand(offHand))
            {
                Noticed?.Invoke(ItemNotice.OffHandIncompatible);
                return false;
            }

            if (!EnsureCanAct()) return false;

            if (instance.Data is WeaponData) loadout.EquipWeapon(instance);
            else if (instance.Data is OffHandData) loadout.EquipOffHand(instance);
            else return false;

            return true;
        }

        public void Unequip(ItemInstance instance)
        {
            if (!IsEquipped(instance) || !EnsureCanAct()) return;

            loadout.Unequip(instance);
        }

        public bool TryUse(ConsumableData consumable)
        {
            if (!Inventory.Contains(consumable) || !EnsureCanAct()) return false;

            controller.StartUseItem(consumable);
            return true;
        }

        // 칸 전체를 플레이어 앞에 버림 (장착 중인 개체는 이동 상태에서만)
        public void Drop(int slotIndex)
        {
            ItemStack stack = Inventory.GetSlot(slotIndex);
            if (stack.IsEmpty) return;
            if (IsEquipped(stack.Instance) && !EnsureCanAct()) return;

            ItemStack removed = Inventory.RemoveAt(slotIndex, stack.Count);
            SpawnWorldItem(removed);
        }

        // 가방에 자리가 없으면 안내만 하고 모션 생략
        public bool TryStartPickUp()
        {
            WorldItem target = Interactor.Target;
            if (target == null) return false;

            if (!Inventory.CanAdd(target.Item))
            {
                Noticed?.Invoke(ItemNotice.InventoryFull);
                return false;
            }

            controller.StartPickUp(target);
            return true;
        }

        // 줍기 모션의 획득 시점에 호출 (개체는 그대로, 겹치는 아이템은 들어간 만큼)
        public void CompletePickUp(WorldItem target)
        {
            if (target == null) return;

            int remaining;
            if (target.Instance != null) remaining = Inventory.TryAdd(target.Instance) ? EmptyCount : target.Count;
            else Inventory.TryAdd(target.Item, target.Count, out remaining);

            if (remaining > EmptyCount) Noticed?.Invoke(ItemNotice.InventoryFull);

            target.SetRemaining(remaining);
            Interactor.Scan();
        }

        private bool EnsureCanAct()
        {
            if (CanAct) return true;

            Noticed?.Invoke(ItemNotice.CannotUseNow);
            return false;
        }

        private void SpawnWorldItem(in ItemStack stack)
        {
            if (worldItemPrefab == null || stack.IsEmpty) return;

            Transform owner = transform;
            Vector3 position = owner.position + owner.forward * dropDistance + Vector3.up * dropHeight;
            WorldItem worldItem = Instantiate(worldItemPrefab, position, owner.rotation);

            if (stack.Instance != null) worldItem.Initialize(stack.Instance);
            else worldItem.Initialize(stack.Item, stack.Count);
        }

        private void HandleWeaponChanged(WeaponData _) => EquipmentChanged?.Invoke();
        private void HandleOffHandChanged(OffHandData _) => EquipmentChanged?.Invoke();

        private void HandleOffHandActiveChanged(bool isActive)
        {
            if (!isActive) Noticed?.Invoke(ItemNotice.OffHandDisabled);
            EquipmentChanged?.Invoke();
        }
    }
}

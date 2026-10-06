using System;
using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.InventorySystem
{
    // 퀵슬롯 등록 (가방에서 사라진 장비 개체는 자동 해제, 소모품은 0개여도 유지)
    [RequireComponent(typeof(Inventory))]
    [DisallowMultipleComponent]
    public sealed class QuickSlots : MonoBehaviour
    {
        public const int NoSlot = -1;
        private const int FirstSlot = 0;

        private Inventory inventory;
        private QuickSlotEntry[] entries = Array.Empty<QuickSlotEntry>();

        public int Count => entries.Length;

        public event Action Changed;

        private void Awake()
        {
            inventory = GetComponent<Inventory>();
            entries = new QuickSlotEntry[inventory.Data.QuickSlotCount];
        }

        private void OnEnable() => inventory.Changed += RemoveMissingInstances;
        private void OnDisable() => inventory.Changed -= RemoveMissingInstances;

        // 가방 개체가 만들어진 뒤(Awake 이후) 시작 등록
        private void Start()
        {
            int startingCount = Mathf.Min(entries.Length, inventory.Data.StartingQuickSlots.Count);
            for (int i = 0; i < startingCount; i++)
            {
                entries[i] = ResolveStarting(inventory.Data.StartingQuickSlots[i]);
            }
            Changed?.Invoke();
        }

        public QuickSlotEntry Get(int index) => IsValid(index) ? entries[index] : QuickSlotEntry.Empty;
        public bool IsValid(int index) => index >= FirstSlot && index < entries.Length;

        // 가방 칸 내용 등록 (같은 개체·종류가 다른 칸에 있으면 이동)
        public bool Assign(int index, in ItemStack stack)
        {
            QuickSlotEntry entry = QuickSlotEntry.From(stack);
            if (!IsValid(index) || entry.IsEmpty) return false;

            int previousIndex = IndexOf(entry);
            if (previousIndex != NoSlot) entries[previousIndex] = QuickSlotEntry.Empty;

            entries[index] = entry;
            Changed?.Invoke();
            return true;
        }

        public void Swap(int from, int to)
        {
            if (!IsValid(from) || !IsValid(to) || from == to) return;

            (entries[from], entries[to]) = (entries[to], entries[from]);
            Changed?.Invoke();
        }

        public void Clear(int index)
        {
            if (!IsValid(index) || entries[index].IsEmpty) return;

            entries[index] = QuickSlotEntry.Empty;
            Changed?.Invoke();
        }

        private int IndexOf(in QuickSlotEntry entry)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Matches(entry)) return i;
            }
            return NoSlot;
        }

        // 장비는 아직 등록되지 않은 같은 종류의 첫 개체
        private QuickSlotEntry ResolveStarting(ItemData item)
        {
            if (item == null || !item.CanQuickSlot) return QuickSlotEntry.Empty;
            if (item.IsStackable) return QuickSlotEntry.FromStackable(item);

            return QuickSlotEntry.FromInstance(inventory.FindFirstInstance(item, IsRegistered));
        }

        private bool IsRegistered(ItemInstance instance)
        {
            foreach (QuickSlotEntry entry in entries)
            {
                if (entry.Instance == instance) return true;
            }
            return false;
        }

        private void RemoveMissingInstances()
        {
            bool isChanged = false;
            for (int i = 0; i < entries.Length; i++)
            {
                ItemInstance instance = entries[i].Instance;
                if (instance == null || inventory.Contains(instance)) continue;

                entries[i] = QuickSlotEntry.Empty;
                isChanged = true;
            }

            if (isChanged) Changed?.Invoke();
        }
    }
}

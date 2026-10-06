using System;
using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.InventorySystem
{
    // 고정 칸 가방: 겹치는 아이템은 중첩, 장비는 칸마다 개체 1개 (변경 시 Changed 1회)
    [DisallowMultipleComponent]
    public sealed class Inventory : MonoBehaviour
    {
        public const int NoSlot = -1;
        private const int EmptyCount = 0;

        [Tooltip("가방 칸 수·퀵슬롯 수·시작 아이템")]
        [SerializeField] private InventoryData data;

        private ItemStack[] slots = Array.Empty<ItemStack>();

        public InventoryData Data => data;
        public int Capacity => slots.Length;

        public event Action Changed;

        private void Awake()
        {
            slots = new ItemStack[data.Capacity];
            foreach (InventoryData.StartingItem starting in data.StartingItems)
            {
                if (starting.Item != null) AddInternal(starting.Item, starting.Count);
            }
        }

        public ItemStack GetSlot(int index) => slots[index];

        public int GetCount(ItemData item)
        {
            if (item == null) return EmptyCount;

            int total = EmptyCount;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Item == item) total += slots[i].Count;
            }
            return total;
        }

        public bool Contains(ItemData item) => GetCount(item) > EmptyCount;
        public bool Contains(ItemInstance instance) => IndexOf(instance) != NoSlot;

        public int IndexOf(ItemInstance instance)
        {
            if (instance == null) return NoSlot;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Instance == instance) return i;
            }
            return NoSlot;
        }

        // 해당 종류의 첫 개체 (시작 장착·퀵슬롯 연결용), exclude와 같은 개체는 건너뜀
        public ItemInstance FindFirstInstance(ItemData item, Predicate<ItemInstance> exclude = null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                ItemInstance instance = slots[i].Instance;
                if (instance != null && instance.Data == item && (exclude == null || !exclude(instance))) return instance;
            }
            return null;
        }

        // 1개라도 들어갈 자리가 있는지
        public bool CanAdd(ItemData item)
        {
            if (item == null) return false;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty) return true;
                if (item.IsStackable && slots[i].Item == item && slots[i].SpaceLeft > EmptyCount) return true;
            }
            return false;
        }

        // 종류 + 수량으로 추가 (장비는 1개마다 새 개체), 넣지 못한 수량은 remaining
        public bool TryAdd(ItemData item, int count, out int remaining)
        {
            remaining = count;
            if (item == null || count <= EmptyCount) return false;

            remaining = AddInternal(item, count);
            if (remaining == count) return false;

            Changed?.Invoke();
            return true;
        }

        // 기존 개체를 그대로 추가 (버렸다 다시 줍기)
        public bool TryAdd(ItemInstance instance)
        {
            int emptySlot = FindEmptySlot();
            if (instance == null || emptySlot == NoSlot) return false;

            slots[emptySlot] = new ItemStack(instance);
            Changed?.Invoke();
            return true;
        }

        // 부족하면 아무것도 소모하지 않음 (뒤쪽 칸부터 소모)
        public bool TryConsume(ItemData item, int count)
        {
            if (count <= EmptyCount || GetCount(item) < count) return false;

            int left = count;
            for (int i = slots.Length - 1; i >= 0 && left > EmptyCount; i--)
            {
                if (slots[i].Item != item) continue;

                int taken = Mathf.Min(left, slots[i].Count);
                slots[i] = slots[i].WithCount(slots[i].Count - taken);
                left -= taken;
            }

            Changed?.Invoke();
            return true;
        }

        // 꺼낸 칸 내용 반환 (장비는 개체 포함)
        public ItemStack RemoveAt(int index, int count)
        {
            ItemStack stack = slots[index];
            if (stack.IsEmpty || count <= EmptyCount) return ItemStack.Empty;

            if (stack.Instance != null)
            {
                slots[index] = ItemStack.Empty;
                Changed?.Invoke();
                return stack;
            }

            int removed = Mathf.Min(count, stack.Count);
            slots[index] = stack.WithCount(stack.Count - removed);
            Changed?.Invoke();
            return new ItemStack(stack.Item, removed);
        }

        public void Swap(int from, int to)
        {
            if (from == to) return;

            (slots[from], slots[to]) = (slots[to], slots[from]);
            Changed?.Invoke();
        }

        private int AddInternal(ItemData item, int count)
        {
            int left = item.IsStackable ? FillExistingStacks(item, count) : count;

            while (left > EmptyCount)
            {
                int emptySlot = FindEmptySlot();
                if (emptySlot == NoSlot) break;

                if (item.IsStackable)
                {
                    int amount = Mathf.Min(left, item.MaxStack);
                    slots[emptySlot] = new ItemStack(item, amount);
                    left -= amount;
                }
                else
                {
                    slots[emptySlot] = new ItemStack(item.CreateInstance());
                    left--;
                }
            }
            return left;
        }

        private int FillExistingStacks(ItemData item, int count)
        {
            int left = count;
            for (int i = 0; i < slots.Length && left > EmptyCount; i++)
            {
                if (slots[i].Item != item || slots[i].SpaceLeft <= EmptyCount) continue;

                int amount = Mathf.Min(left, slots[i].SpaceLeft);
                slots[i] = slots[i].WithCount(slots[i].Count + amount);
                left -= amount;
            }
            return left;
        }

        private int FindEmptySlot()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty) return i;
            }
            return NoSlot;
        }
    }
}

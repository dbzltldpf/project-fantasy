using System;
using System.Collections.Generic;
using ProjectFantasy.Items;
using UnityEngine;

namespace ProjectFantasy.InventorySystem
{
    // 가방 칸 수, 퀵슬롯 수, 시작 아이템
    [CreateAssetMenu(fileName = "InventoryData", menuName = "ProjectFantasy/Inventory/Inventory Data")]
    public sealed class InventoryData : ScriptableObject
    {
        [Serializable]
        public struct StartingItem
        {
            [Tooltip("지급할 아이템")]
            [SerializeField] private ItemData item;
            [Tooltip("수량 (장비는 개수만큼 개체 생성)")]
            [SerializeField, Min(1)] private int count;
            [Tooltip("장비 티어 (0 = T0, 겹치는 아이템은 무시)")]
            [SerializeField, Min(ItemData.MinTier)] private int tier;

            public ItemData Item => item;
            public int Count => count;
            public int Tier => Mathf.Max(ItemData.MinTier, tier);
        }

        [Tooltip("가방 칸 수")]
        [SerializeField, Min(1)] private int capacity = 30;
        [Tooltip("키보드 1~8 바인딩 수와 맞출 것")]
        [SerializeField, Min(1)] private int quickSlotCount = 8;
        [Tooltip("시작 시 가방에 넣을 아이템")]
        [SerializeField] private StartingItem[] startingItems = Array.Empty<StartingItem>();
        [Tooltip("퀵슬롯 순서대로 등록 (무기·소모품만, 가방에 있어야 함)")]
        [SerializeField] private ItemData[] startingQuickSlots = Array.Empty<ItemData>();

        public int Capacity => capacity;
        public int QuickSlotCount => quickSlotCount;
        public IReadOnlyList<StartingItem> StartingItems => startingItems;
        public IReadOnlyList<ItemData> StartingQuickSlots => startingQuickSlots;
    }
}

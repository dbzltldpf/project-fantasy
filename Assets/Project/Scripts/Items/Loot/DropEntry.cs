using System;
using UnityEngine;

namespace ProjectFantasy.Items
{
    // 드랍 표 한 줄: 아이템, 확률, 수량 범위, 장비 티어 범위
    [Serializable]
    public sealed class DropEntry
    {
        private const int MinDropCount = 1;

        [SerializeField] private ItemData item;
        [Tooltip("드랍 확률 (0~1, 줄마다 독립 판정)")]
        [SerializeField, Range(0f, 1f)] private float chance = 0.5f;
        [Tooltip("수량 범위 (장비는 개수만큼 개체 생성)")]
        [SerializeField, Min(MinDropCount)] private int minCount = MinDropCount;
        [SerializeField, Min(MinDropCount)] private int maxCount = MinDropCount;
        [Tooltip("장비 티어 범위 (겹치는 아이템은 무시)")]
        [SerializeField, Min(ItemData.MinTier)] private int minTier = ItemData.MinTier;
        [SerializeField, Min(ItemData.MinTier)] private int maxTier = ItemData.MinTier;

        public ItemData Item => item;
        public float Chance => chance;
        // 인스펙터에서 배열 칸을 늘리면 새 줄이 0으로 채워지므로 최솟값 보장
        public int MinCount => Mathf.Max(MinDropCount, minCount);
        public int MaxCount => Mathf.Max(MinCount, maxCount);
        public int MinTier => Mathf.Max(ItemData.MinTier, minTier);
        public int MaxTier => Mathf.Max(MinTier, maxTier);
    }
}

using System;
using UnityEngine;

namespace ProjectFantasy.Items
{
    // 능력치 랜덤 범위 (최솟값~최댓값 포함)
    [Serializable]
    public struct StatRange
    {
        private const int InclusiveOffset = 1;

        [Tooltip("최솟값")]
        [SerializeField, Min(0)] private int min;
        [Tooltip("최댓값 (포함)")]
        [SerializeField, Min(0)] private int max;

        public int Min => min;
        public int Max => Mathf.Max(min, max);

        public StatRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        // 범위 균등 랜덤 × 등급 배율 (반올림)
        public int Roll(float multiplier)
        {
            int baseValue = UnityEngine.Random.Range(Min, Max + InclusiveOffset);
            return Mathf.RoundToInt(baseValue * multiplier);
        }
    }
}

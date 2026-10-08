using System;
using UnityEngine;

namespace ProjectFantasy.Utils
{
    // 실수 랜덤 범위 (인스펙터에 Min / Max 한 줄로 표시)
    [Serializable]
    public struct FloatRange
    {
        [SerializeField, Min(0f)] private float min;
        [SerializeField, Min(0f)] private float max;

        public float Min => min;
        public float Max => Mathf.Max(min, max);

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Random() => UnityEngine.Random.Range(Min, Max);
    }
}

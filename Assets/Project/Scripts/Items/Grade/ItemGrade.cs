using System;
using UnityEngine;

namespace ProjectFantasy.Items
{
    // 장비 등급 (이름, 표시 색, 뽑힐 가중치, 능력치 배율)
    [Serializable]
    public sealed class ItemGrade
    {
        [Tooltip("등급 이름 (상세 패널 표시)")]
        [SerializeField] private string displayName;
        [Tooltip("이름 표시 색")]
        [SerializeField] private Color color = Color.white;
        [Tooltip("뽑힐 상대 가중치 (0이면 나오지 않음)")]
        [SerializeField, Min(0)] private int weight;
        [Tooltip("능력치 배율 (범위에서 굴린 값 × 배율)")]
        [SerializeField, Min(0f)] private float statMultiplier = 1f;

        public string DisplayName => displayName;
        public Color Color => color;
        public int Weight => weight;
        public float StatMultiplier => statMultiplier;

        public ItemGrade(string displayName, Color color, int weight, float statMultiplier)
        {
            this.displayName = displayName;
            this.color = color;
            this.weight = weight;
            this.statMultiplier = statMultiplier;
        }
    }
}

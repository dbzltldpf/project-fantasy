using System;
using System.Collections.Generic;
using ProjectFantasy.Utils;
using UnityEngine;

namespace ProjectFantasy.Items
{
    // 음식·회복약 등 사용 아이템 (사용 모션, 효과 발동 시점, 효과 목록)
    [CreateAssetMenu(fileName = "ConsumableData", menuName = "ProjectFantasy/Items/Consumable Data")]
    public sealed class ConsumableData : ItemData
    {
        private const int ConsumableMaxStack = 20;

        [Header("Use")]
        [Tooltip("사용 모션 애니메이터 상태")]
        [SerializeField] private string useStateName = "Use_Item";
        [Tooltip("사용 시작부터 효과가 적용되는 시간 (그 전에 피격되면 소모 안 됨)")]
        [SerializeField, Min(0f)] private float effectTime = 0.6f;
        [Tooltip("사용 동작 전체 시간")]
        [SerializeField, Min(0f)] private float duration = 1.2f;
        [Tooltip("사용 중 이동 속도 (m/s)")]
        [SerializeField, Min(0f)] private float moveSpeed = 1f;

        [Header("Effects")]
        [SerializeReference, SubclassSelector] private List<ConsumableEffect> effects = new List<ConsumableEffect>();

        [NonSerialized] private int useStateHash;
        [NonSerialized] private bool isHashCached;

        public override ItemCategory Category => ItemCategory.Consumable;
        public override bool CanQuickSlot => true;
        protected override int DefaultMaxStack => ConsumableMaxStack;

        public string UseStateName => useStateName;
        public float EffectTime => effectTime;
        public float Duration => duration;
        public float MoveSpeed => moveSpeed;

        public int UseStateHash
        {
            get
            {
                if (!isHashCached)
                {
                    useStateHash = Animator.StringToHash(useStateName);
                    isHashCached = true;
                }
                return useStateHash;
            }
        }

        public void ApplyEffects(GameObject user)
        {
            foreach (ConsumableEffect effect in effects)
            {
                effect?.Apply(user);
            }
        }

        private void OnValidate()
        {
            isHashCached = false;
            if (effectTime > duration) Debug.LogWarning($"[{name}] effectTime > duration (효과 적용 전에 사용이 끝남)", this);
        }
    }
}

using UnityEngine;

namespace ProjectFantasy.Items
{
    // 가방에 들어가는 모든 아이템 공통 데이터 (이름, 아이콘, 중첩, 필드 모델)
    public abstract class ItemData : ScriptableObject
    {
        // 티어는 0부터 (T0 = 가장 낮은 티어)
        public const int MinTier = 0;
        private const int SingleStack = 1;

        [Tooltip("게임 내 표시 이름 (비우면 에셋 이름)")]
        [SerializeField] private string displayName;
        [Tooltip("슬롯 아이콘 (비우면 슬롯에 이름 표시)")]
        [SerializeField] private Sprite icon;
        [Tooltip("인벤토리 상세 패널 설명")]
        [SerializeField, TextArea] private string description;
        [Tooltip("한 칸에 겹칠 수 있는 최대 수량 (1이면 칸당 1개)")]
        [SerializeField, Min(SingleStack)] private int maxStack = SingleStack;
        [Tooltip("필드에 놓였을 때 모델 (비우면 장비 모델 사용)")]
        [SerializeField] private GameObject worldModelPrefab;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public Sprite Icon => icon;
        public string Description => description;
        public int MaxStack => maxStack;
        public bool IsStackable => maxStack > SingleStack;

        public virtual GameObject WorldModel => worldModelPrefab;
        // 생성된 모델에 티어 외형(머티리얼·색조) 적용 (장비만, 기본은 원본 그대로)
        public virtual void ApplyModelVisual(GameObject model, int tier) { }
        public abstract ItemCategory Category { get; }

        // 퀵슬롯 등록 가능 여부 (무기·소모품)
        public virtual bool CanQuickSlot => false;

        // 표시할 능력치 (장비만)
        public virtual bool HasStat(StatType stat) => false;

        // 겹치지 않는 아이템 1개를 개체로 생성 (장비는 티어 배율·등급·능력치를 이때 굴림, 시작 지급은 최하 등급)
        public virtual ItemInstance CreateInstance(int tier = MinTier, bool useLowestGrade = false)
        {
            return new ItemInstance(this, MinTier, null, null, ItemStats.Zero);
        }

        // 에셋 생성 시 종류별 기본 중첩 수
        protected virtual int DefaultMaxStack => SingleStack;

        protected virtual void Reset() => maxStack = DefaultMaxStack;
    }
}

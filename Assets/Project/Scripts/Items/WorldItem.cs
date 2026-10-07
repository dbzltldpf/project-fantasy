using UnityEngine;

namespace ProjectFantasy.Items
{
    // 필드에 놓인 아이템 (Interactable 레이어 트리거 콜라이더와 함께 사용), 장비는 개체(티어·등급·능력치)를 보관
    [DisallowMultipleComponent]
    public sealed class WorldItem : MonoBehaviour
    {
        private const int EmptyCount = 0;
        private const int SingleCount = 1;

        [Tooltip("놓을 아이템")]
        [SerializeField] private ItemData item;
        [Tooltip("수량 (장비는 1개로 고정)")]
        [SerializeField, Min(1)] private int count = 1;
        [Tooltip("장비 티어 (씬 배치 시 이 티어로 개체 생성, 겹치는 아이템은 무시)")]
        [SerializeField, Min(ItemData.MinTier)] private int tier = ItemData.MinTier;
        [Tooltip("모델을 붙일 위치 (비우면 자신)")]
        [SerializeField] private Transform modelRoot;

        private GameObject model;

        public ItemData Item => item;
        public int Count => count;
        // 겹치지 않는 아이템의 개체 (씬 배치 장비는 시작 시 지정 티어로 등급·능력치를 굴림)
        public ItemInstance Instance { get; private set; }

        private void Start()
        {
            if (Instance == null && item != null && !item.IsStackable)
            {
                Instance = item.CreateInstance(tier);
                count = SingleCount;
            }
            if (model == null) RefreshModel();
        }

        // 겹치는 아이템 버리기
        public void Initialize(ItemData itemData, int itemCount)
        {
            item = itemData;
            count = itemCount;
            Instance = null;
            RefreshModel();
        }

        // 장비 개체 버리기 (다시 주우면 같은 개체)
        public void Initialize(ItemInstance instance)
        {
            item = instance.Data;
            count = SingleCount;
            Instance = instance;
            RefreshModel();
        }

        // 가방에 다 들어가지 못한 수량만 남김
        public void SetRemaining(int remaining)
        {
            if (remaining <= EmptyCount)
            {
                // 파괴는 프레임 끝이므로 즉시 비활성화해 탐색 대상에서 제외
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
            count = remaining;
        }

        private void RefreshModel()
        {
            if (model != null) Destroy(model);
            if (item == null || item.WorldModel == null) return;

            model = Instantiate(item.WorldModel, modelRoot != null ? modelRoot : transform, false);
            item.ApplyModelVisual(model, Instance != null ? Instance.Tier : tier);
        }
    }
}

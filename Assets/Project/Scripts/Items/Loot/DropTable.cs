using System.Collections.Generic;
using UnityEngine;

namespace ProjectFantasy.Items
{
    // 처치 시 드랍 표 (줄마다 독립 확률, 장비는 티어 범위 안에서 개체 생성)
    [CreateAssetMenu(fileName = "DropTable", menuName = "ProjectFantasy/Items/Drop Table")]
    public sealed class DropTable : ScriptableObject
    {
        // Random.Range(int)는 최댓값 제외라 포함하도록 더함
        private const int InclusiveMaxOffset = 1;

        [SerializeField] private DropEntry[] entries = System.Array.Empty<DropEntry>();

        // 결과를 버퍼에 추가 (호출 측 리스트 재사용으로 할당 최소화)
        public void Roll(List<LootDrop> results)
        {
            foreach (DropEntry entry in entries)
            {
                if (entry.Item == null || Random.value >= entry.Chance) continue;

                int count = Random.Range(entry.MinCount, entry.MaxCount + InclusiveMaxOffset);
                if (entry.Item.IsStackable)
                {
                    results.Add(new LootDrop(entry.Item, count));
                    continue;
                }

                for (int i = 0; i < count; i++)
                {
                    int tier = Random.Range(entry.MinTier, entry.MaxTier + InclusiveMaxOffset);
                    results.Add(new LootDrop(entry.Item.CreateInstance(tier)));
                }
            }
        }
    }
}

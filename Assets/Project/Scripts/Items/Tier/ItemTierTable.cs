using UnityEngine;

namespace ProjectFantasy.Items
{
    // 티어 목록 (순서대로 T0, T1 …) + 공용 아우라 프리팹·아웃라인 머티리얼, 장비 데이터가 공용으로 참조 (해금 숙련 레벨은 MasteryData)
    [CreateAssetMenu(fileName = "ItemTierTable", menuName = "ProjectFantasy/Items/Item Tier Table")]
    public sealed class ItemTierTable : ScriptableObject
    {
        private const int LastIndexOffset = 1;

        [Tooltip("티어 목록 (첫 줄 = T0)")]
        [SerializeField] private ItemTier[] tiers =
        {
            new ItemTier("낡은", 1f),
            new ItemTier("철", 1.3f, new Color(2f, 2f, 2f, 0.9f), 10f, new Color(1.5f, 1.5f, 1.5f), 0.004f),
            new ItemTier("강철", 1.6f, new Color(1f, 1.8f, 3f, 0.95f), 15f, new Color(0.8f, 1.5f, 2.5f), 0.006f),
            new ItemTier("미스릴", 2f, new Color(2f, 1f, 4f, 1f), 25f, new Color(1.6f, 0.8f, 3.2f), 0.008f),
            new ItemTier("용의", 2.5f, new Color(6f, 2.4f, 0.5f, 1f), 40f, new Color(5f, 2f, 0.4f), 0.01f),
        };
        [Tooltip("불씨 아우라 파티클 프리팹 (Tools → ProjectFantasy → Create Weapon Aura Prefab로 생성)")]
        [SerializeField] private ParticleSystem auraPrefab;
        [Tooltip("테두리 발광 머티리얼 (ProjectFantasy/WeaponOutline 셰이더, 같은 메뉴로 생성)")]
        [SerializeField] private Material outlineMaterial;

        public ParticleSystem AuraPrefab => auraPrefab;
        public Material OutlineMaterial => outlineMaterial;

        public int Clamp(int tier) => Mathf.Clamp(tier, ItemData.MinTier, ItemData.MinTier + Mathf.Max(0, tiers.Length - LastIndexOffset));

        // 범위 밖이면 가장 가까운 티어
        public ItemTier Get(int tier) => tiers.Length > 0 ? tiers[Clamp(tier) - ItemData.MinTier] : null;
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectFantasy.Items
{
    // 모델의 각 메시에 테두리 발광 복제본 부착 (메시 공유, 티어별 머티리얼 복사본은 1회 생성 후 공유)
    public static class TierOutline
    {
        private const string OutlineObjectName = "TierOutline";

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int WidthId = Shader.PropertyToID("_Width");

        private static readonly Dictionary<(Material, ItemTier), Material> MaterialCache = new Dictionary<(Material, ItemTier), Material>();
        private static readonly List<MeshFilter> FilterBuffer = new List<MeshFilter>();

        // 플레이 모드 재진입(도메인 리로드 끔) 시 이전 세션 캐시 제거
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => MaterialCache.Clear();

        public static void Attach(GameObject model, Material source, ItemTier tier)
        {
            if (source == null || tier == null || !tier.UseOutline) return;

            Material material = GetOrCreateMaterial(source, tier);

            // 부착 중 새로 생기는 복제본이 다시 탐색되지 않도록 먼저 수집
            model.GetComponentsInChildren(true, FilterBuffer);
            foreach (MeshFilter filter in FilterBuffer)
            {
                if (filter.sharedMesh == null || !filter.TryGetComponent(out MeshRenderer _)) continue;

                GameObject outline = new GameObject(OutlineObjectName, typeof(MeshFilter), typeof(MeshRenderer));
                outline.layer = filter.gameObject.layer;
                outline.transform.SetParent(filter.transform, false);
                outline.GetComponent<MeshFilter>().sharedMesh = filter.sharedMesh;

                MeshRenderer outlineRenderer = outline.GetComponent<MeshRenderer>();
                outlineRenderer.sharedMaterial = material;
                outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
                outlineRenderer.receiveShadows = false;
            }
            FilterBuffer.Clear();
        }

        private static Material GetOrCreateMaterial(Material source, ItemTier tier)
        {
            if (MaterialCache.TryGetValue((source, tier), out Material material)) return material;

            material = new Material(source) { name = $"{source.name} ({tier.Prefix})", hideFlags = HideFlags.DontSave };
            material.SetColor(BaseColorId, tier.OutlineColor);
            material.SetFloat(WidthId, tier.OutlineWidth);

            MaterialCache.Add((source, tier), material);
            return material;
        }
    }
}

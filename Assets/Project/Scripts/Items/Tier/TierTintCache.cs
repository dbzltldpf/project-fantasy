using System.Collections.Generic;
using UnityEngine;

namespace ProjectFantasy.Items
{
    // 원본 머티리얼 + 티어 조합별 색조 복사본을 1회만 생성해 공유 (무기마다 텍스처가 달라도 티어표 한 벌로 적용)
    public static class TierTintCache
    {
        private const string EmissionKeyword = "_EMISSION";

        // URP Lit 우선, Built-in Standard 대체
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int LegacySmoothnessId = Shader.PropertyToID("_Glossiness");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private static readonly Dictionary<(Material, ItemTier), Material> Cache = new Dictionary<(Material, ItemTier), Material>();

        // 플레이 모드 재진입(도메인 리로드 끔) 시 이전 세션 캐시 제거
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => Cache.Clear();

        public static void Apply(GameObject model, ItemTier tier)
        {
            if (tier == null || !tier.ApplyTint) return;

            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null) materials[i] = GetOrCreate(materials[i], tier);
                }
                renderer.sharedMaterials = materials;
            }
        }

        private static Material GetOrCreate(Material source, ItemTier tier)
        {
            if (Cache.TryGetValue((source, tier), out Material tinted)) return tinted;

            tinted = new Material(source) { name = $"{source.name} ({tier.Prefix})", hideFlags = HideFlags.DontSave };
            SetColor(tinted, BaseColorId, LegacyColorId, tier.BaseColor);
            if (tinted.HasProperty(MetallicId)) tinted.SetFloat(MetallicId, tier.Metallic);
            SetFloat(tinted, SmoothnessId, LegacySmoothnessId, tier.Smoothness);

            if (tier.UseEmission && tinted.HasProperty(EmissionColorId))
            {
                tinted.EnableKeyword(EmissionKeyword);
                tinted.SetColor(EmissionColorId, tier.EmissionColor);
            }

            Cache.Add((source, tier), tinted);
            return tinted;
        }

        private static void SetColor(Material material, int primaryId, int fallbackId, Color color)
        {
            if (material.HasProperty(primaryId)) material.SetColor(primaryId, color);
            else if (material.HasProperty(fallbackId)) material.SetColor(fallbackId, color);
        }

        private static void SetFloat(Material material, int primaryId, int fallbackId, float value)
        {
            if (material.HasProperty(primaryId)) material.SetFloat(primaryId, value);
            else if (material.HasProperty(fallbackId)) material.SetFloat(fallbackId, value);
        }
    }
}

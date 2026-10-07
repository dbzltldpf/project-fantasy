using System.Collections.Generic;
using UnityEngine;

namespace ProjectFantasy.Items
{
    // 모델에 티어 아우라 부착: 가장 큰 렌더러의 로컬 경계 상자에서 입자 방출 (메시 Read/Write 불필요, 무기 모양 자동 대응)
    // 파티클 정점 색은 8비트라 1 초과(HDR)가 잘림 → HDR 색은 티어별 머티리얼 복사본의 Base Color로 전달해 Bloom 반영
    public static class TierAura
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

        private static readonly Dictionary<(Material, ItemTier), Material> MaterialCache = new Dictionary<(Material, ItemTier), Material>();

        // 플레이 모드 재진입(도메인 리로드 끔) 시 이전 세션 캐시 제거
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache() => MaterialCache.Clear();

        public static void Attach(GameObject model, ParticleSystem prefab, ItemTier tier)
        {
            if (prefab == null || tier == null || !tier.UseAura) return;

            Renderer target = FindLargestRenderer(model);
            if (target == null) return;

            ParticleSystem aura = Object.Instantiate(prefab, target.transform, false);
            Bounds bounds = target.localBounds;

            // 루트(불씨)와 자식 층(글로우 등) 모두 무기 경계 상자·티어 색 적용
            foreach (ParticleSystem layer in aura.GetComponentsInChildren<ParticleSystem>(true))
            {
                ApplyTier(layer, bounds, tier);
            }

            // 방출량은 루트(불씨)만 티어 값, 자식 층은 프리팹 값 유지
            ParticleSystem.EmissionModule emission = aura.emission;
            emission.rateOverTime = tier.AuraRate;

            aura.Play(true);
        }

        private static void ApplyTier(ParticleSystem layer, Bounds bounds, ItemTier tier)
        {
            ParticleSystem.ShapeModule shape = layer.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.position = bounds.center;
            shape.scale = bounds.size;

            // 색·밝기(HDR)는 머티리얼, 입자 정점 색은 투명도만
            ParticleSystem.MainModule main = layer.main;
            Color startColor = Color.white;
            startColor.a = tier.AuraColor.a;
            main.startColor = startColor;

            ParticleSystemRenderer layerRenderer = layer.GetComponent<ParticleSystemRenderer>();
            layerRenderer.sharedMaterial = GetOrCreateMaterial(layerRenderer.sharedMaterial, tier);
        }

        private static Material GetOrCreateMaterial(Material source, ItemTier tier)
        {
            if (source == null) return null;
            if (MaterialCache.TryGetValue((source, tier), out Material material)) return material;

            material = new Material(source) { name = $"{source.name} ({tier.Prefix})", hideFlags = HideFlags.DontSave };
            Color hdrColor = tier.AuraColor;
            hdrColor.a = source.HasProperty(BaseColorId) ? source.GetColor(BaseColorId).a : hdrColor.a;

            if (material.HasProperty(BaseColorId)) material.SetColor(BaseColorId, hdrColor);
            else if (material.HasProperty(LegacyColorId)) material.SetColor(LegacyColorId, hdrColor);

            MaterialCache.Add((source, tier), material);
            return material;
        }

        private static Renderer FindLargestRenderer(GameObject model)
        {
            Renderer largest = null;
            float largestVolume = 0f;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is ParticleSystemRenderer) continue;

                Vector3 size = renderer.localBounds.size;
                float volume = size.x * size.y * size.z;
                if (largest != null && volume <= largestVolume) continue;

                largest = renderer;
                largestVolume = volume;
            }
            return largest;
        }
    }
}

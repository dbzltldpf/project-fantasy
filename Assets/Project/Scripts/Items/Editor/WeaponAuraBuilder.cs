using ProjectFantasy.Items;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectFantasy.ItemsEditor
{
    // 티어 이펙트 에셋 생성: 불씨 아우라 프리팹·가산 파티클 머티리얼·테두리 발광 머티리얼, 비어 있는 티어표에 자동 연결 (이미 있으면 덮어쓰지 않음)
    public static class WeaponAuraBuilder
    {
        private const string MenuPath = "Tools/ProjectFantasy/Create Weapon Tier Effects";
        private const string MaterialFolder = "Assets/Project/Materials/Effects";
        private const string MaterialPath = MaterialFolder + "/WeaponAura.mat";
        private const string OutlineMaterialPath = MaterialFolder + "/WeaponOutline.mat";
        private const string OutlineShaderName = "ProjectFantasy/WeaponOutline";
        private const string PrefabFolder = "Assets/Project/Prefabs/Effects";
        private const string PrefabPath = PrefabFolder + "/WeaponAura.prefab";
        private const string ParticleShaderName = "Universal Render Pipeline/Particles/Unlit";
        private const string ParticleTextureName = "Default-Particle.psd";
        private const string TierTableFilter = "t:" + nameof(ItemTierTable);
        private const string AuraPrefabField = "auraPrefab";
        private const string OutlineMaterialField = "outlineMaterial";
        private const char PathSeparator = '/';

        // URP 파티클 셰이더 속성
        private const string SurfaceProperty = "_Surface";
        private const string BlendProperty = "_Blend";
        private const string SrcBlendProperty = "_SrcBlend";
        private const string DstBlendProperty = "_DstBlend";
        private const string ZWriteProperty = "_ZWrite";
        private const string BaseMapProperty = "_BaseMap";
        private const string TransparentKeyword = "_SURFACE_TYPE_TRANSPARENT";
        private const float TransparentSurface = 1f;
        private const float AdditiveBlend = 2f;
        private const float ZWriteOff = 0f;

        // 투명도·크기 커브 키
        private const float StartTime = 0f;
        private const float EndTime = 1f;
        private const float Opaque = 1f;
        private const float Transparent = 0f;

        // 층별 파티클 설정 (색·불씨 방출량은 티어표가 덮어씀)
        private readonly struct LayerSettings
        {
            public readonly string Name;
            public readonly Vector2 Lifetime;
            public readonly Vector2 Speed;
            public readonly Vector2 Size;
            public readonly float Gravity;
            public readonly float Rate;
            public readonly int MaxParticles;
            public readonly float PeakAlpha;
            public readonly float FadeIn;
            public readonly float FadeOutStart;
            public readonly float EndSizeScale;
            public readonly ParticleSystemSimulationSpace Space;

            public LayerSettings(string name, Vector2 lifetime, Vector2 speed, Vector2 size, float gravity, float rate, int maxParticles,
                float peakAlpha, float fadeIn, float fadeOutStart, float endSizeScale, ParticleSystemSimulationSpace space)
            {
                Name = name;
                Lifetime = lifetime;
                Speed = speed;
                Size = size;
                Gravity = gravity;
                Rate = rate;
                MaxParticles = maxParticles;
                PeakAlpha = peakAlpha;
                FadeIn = fadeIn;
                FadeOutStart = fadeOutStart;
                EndSizeScale = endSizeScale;
                Space = space;
            }
        }

        // 불씨: 작고 밝은 입자, 월드 공간이라 휘두르면 꼬리
        private static readonly LayerSettings EmberLayer = new LayerSettings("WeaponAura",
            new Vector2(0.5f, 1f), new Vector2(0.02f, 0.12f), new Vector2(0.08f, 0.18f), -0.08f, 15f, 150,
            1f, 0.2f, 0.6f, 0.3f, ParticleSystemSimulationSpace.World);

        [MenuItem(MenuPath)]
        private static void Create()
        {
            Material material = LoadOrCreateMaterial();
            ParticleSystem prefab = LoadOrCreatePrefab(material);
            Material outlineMaterial = LoadOrCreateOutlineMaterial();
            if (outlineMaterial == null) return;

            int linkedCount = LinkToEmptyTierTables(prefab, outlineMaterial);

            Selection.activeObject = prefab.gameObject;
            Debug.Log($"[{nameof(WeaponAuraBuilder)}] 불씨 아우라: {PrefabPath}, 테두리: {OutlineMaterialPath}, 티어표 {linkedCount}개에 연결");
        }

        private static Material LoadOrCreateMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (existing != null) return existing;

            Material material = new Material(Shader.Find(ParticleShaderName));
            material.SetFloat(SurfaceProperty, TransparentSurface);
            material.SetFloat(BlendProperty, AdditiveBlend);
            material.SetFloat(SrcBlendProperty, (float)BlendMode.SrcAlpha);
            material.SetFloat(DstBlendProperty, (float)BlendMode.One);
            material.SetFloat(ZWriteProperty, ZWriteOff);
            material.EnableKeyword(TransparentKeyword);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetTexture(BaseMapProperty, AssetDatabase.GetBuiltinExtraResource<Texture2D>(ParticleTextureName));

            EnsureFolder(MaterialFolder);
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        private static ParticleSystem LoadOrCreatePrefab(Material material)
        {
            ParticleSystem existing = AssetDatabase.LoadAssetAtPath<ParticleSystem>(PrefabPath);
            if (existing != null) return existing;

            ParticleSystem ember = CreateLayer(EmberLayer, material, null);

            EnsureFolder(PrefabFolder);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(ember.gameObject, PrefabPath);
            Object.DestroyImmediate(ember.gameObject);
            return prefab.GetComponent<ParticleSystem>();
        }

        private static Material LoadOrCreateOutlineMaterial()
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterialPath);
            if (existing != null) return existing;

            Shader shader = Shader.Find(OutlineShaderName);
            if (shader == null)
            {
                Debug.LogError($"[{nameof(WeaponAuraBuilder)}] '{OutlineShaderName}' 셰이더를 찾지 못했습니다 (Assets/Project/Shaders/WeaponOutline.shader 컴파일 확인).");
                return null;
            }

            Material material = new Material(shader);
            EnsureFolder(MaterialFolder);
            AssetDatabase.CreateAsset(material, OutlineMaterialPath);
            return material;
        }

        private static ParticleSystem CreateLayer(in LayerSettings settings, Material material, Transform parent)
        {
            GameObject layerObject = new GameObject(settings.Name, typeof(ParticleSystem));
            if (parent != null) layerObject.transform.SetParent(parent, false);

            ParticleSystem particles = layerObject.GetComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(settings.Lifetime.x, settings.Lifetime.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(settings.Speed.x, settings.Speed.y);
            main.startSize = new ParticleSystem.MinMaxCurve(settings.Size.x, settings.Size.y);
            main.gravityModifier = settings.Gravity;
            main.simulationSpace = settings.Space;
            main.maxParticles = settings.MaxParticles;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = settings.Rate;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;

            // 서서히 나타났다 사라짐
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, StartTime), new GradientColorKey(Color.white, EndTime) },
                new[]
                {
                    new GradientAlphaKey(Transparent, StartTime), new GradientAlphaKey(settings.PeakAlpha, settings.FadeIn),
                    new GradientAlphaKey(settings.PeakAlpha, settings.FadeOutStart), new GradientAlphaKey(Transparent, EndTime)
                });
            colorOverLifetime.color = fade;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(Opaque, AnimationCurve.Linear(StartTime, Opaque, EndTime, settings.EndSizeScale));

            ParticleSystemRenderer particleRenderer = layerObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            particleRenderer.sharedMaterial = material;
            return particles;
        }

        private static int LinkToEmptyTierTables(ParticleSystem prefab, Material outlineMaterial)
        {
            int linkedCount = 0;
            foreach (string guid in AssetDatabase.FindAssets(TierTableFilter))
            {
                ItemTierTable table = AssetDatabase.LoadAssetAtPath<ItemTierTable>(AssetDatabase.GUIDToAssetPath(guid));
                SerializedObject tableObject = new SerializedObject(table);
                bool isLinked = LinkIfEmpty(tableObject.FindProperty(AuraPrefabField), prefab);
                isLinked |= LinkIfEmpty(tableObject.FindProperty(OutlineMaterialField), outlineMaterial);
                if (!isLinked) continue;

                tableObject.ApplyModifiedPropertiesWithoutUndo();
                linkedCount++;
            }

            AssetDatabase.SaveAssets();
            return linkedCount;
        }

        private static bool LinkIfEmpty(SerializedProperty property, Object value)
        {
            if (property.objectReferenceValue != null) return false;

            property.objectReferenceValue = value;
            return true;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            int slash = path.LastIndexOf(PathSeparator);
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectFantasy.CombatEditor
{
    // 이펙트 프리팹의 Variant를 만들어 파티클·라이트 색의 색조(Hue)만 목표 색으로 변경 (원본·서드파티 수정 없음)
    // 채도·밝기·HDR 강도·알파는 유지, 흰색·회색은 색조가 없어 그대로
    public sealed class EffectRecolorWindow : EditorWindow
    {
        private const string MenuPath = "Tools/ProjectFantasy/Recolor Effect";
        private const string WindowTitle = "Recolor Effect";
        private const string DefaultSuffix = "Red";
        private const string PrefabExtension = ".prefab";
        private const string NameSeparator = " ";

        private GameObject source;
        private Color targetColor = Color.red;
        private string suffix = DefaultSuffix;

        [MenuItem(MenuPath)]
        private static void Open() => GetWindow<EffectRecolorWindow>(WindowTitle);

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("원본 이펙트의 Prefab Variant를 같은 폴더에 만들고 파티클(시작 색·수명/속도별 색·트레일)과 Light 색의 색조만 바꿉니다.", MessageType.Info);

            source = (GameObject)EditorGUILayout.ObjectField("Source", source, typeof(GameObject), false);
            targetColor = EditorGUILayout.ColorField(new GUIContent("Target Color", "이 색의 색조만 사용 (밝기·채도는 원본 유지)"), targetColor, true, false, false);
            suffix = EditorGUILayout.TextField(new GUIContent("Suffix", "새 프리팹 이름 = 원본 이름 + 공백 + Suffix"), suffix);

            bool isValid = source != null && PrefabUtility.IsPartOfPrefabAsset(source) && !string.IsNullOrWhiteSpace(suffix);
            using (new EditorGUI.DisabledScope(!isValid))
            {
                if (GUILayout.Button("Create Variant")) CreateVariant();
            }
        }

        private void CreateVariant()
        {
            string sourcePath = AssetDatabase.GetAssetPath(source);
            string folder = Path.GetDirectoryName(sourcePath);
            string targetPath = $"{folder}/{source.name}{NameSeparator}{suffix.Trim()}{PrefabExtension}".Replace('\\', '/');

            if (AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) != null
                && !EditorUtility.DisplayDialog(WindowTitle, $"'{targetPath}'가 이미 있습니다. 덮어쓸까요?", "덮어쓰기", "취소"))
            {
                return;
            }

            Color.RGBToHSV(targetColor, out float targetHue, out _, out _);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            try
            {
                int particleCount = RecolorParticles(instance, targetHue);
                int lightCount = RecolorLights(instance, targetHue);

                // 프리팹 인스턴스를 새 경로로 저장하면 원본의 Variant가 됨
                GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, targetPath);
                EditorGUIUtility.PingObject(variant);
                Debug.Log($"[{nameof(EffectRecolorWindow)}] '{targetPath}' 생성 (파티클 {particleCount}개, 라이트 {lightCount}개 색 변경)", variant);
            }
            finally
            {
                DestroyImmediate(instance);
            }
        }

        private static int RecolorParticles(GameObject root, float hue)
        {
            ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem system in systems)
            {
                ParticleSystem.MainModule main = system.main;
                main.startColor = Shift(main.startColor, hue);

                ParticleSystem.ColorOverLifetimeModule overLifetime = system.colorOverLifetime;
                overLifetime.color = Shift(overLifetime.color, hue);

                ParticleSystem.ColorBySpeedModule bySpeed = system.colorBySpeed;
                bySpeed.color = Shift(bySpeed.color, hue);

                ParticleSystem.TrailModule trails = system.trails;
                trails.colorOverLifetime = Shift(trails.colorOverLifetime, hue);
                trails.colorOverTrail = Shift(trails.colorOverTrail, hue);

                PrefabUtility.RecordPrefabInstancePropertyModifications(system);
            }
            return systems.Length;
        }

        private static int RecolorLights(GameObject root, float hue)
        {
            Light[] lights = root.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                light.color = Shift(light.color, hue);
                PrefabUtility.RecordPrefabInstancePropertyModifications(light);
            }
            return lights.Length;
        }

        // 모든 지정 방식(단색·두 색·그라디언트·두 그라디언트·랜덤) 처리
        private static ParticleSystem.MinMaxGradient Shift(ParticleSystem.MinMaxGradient gradient, float hue)
        {
            ParticleSystem.MinMaxGradient result;
            switch (gradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    result = new ParticleSystem.MinMaxGradient(Shift(gradient.color, hue));
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    result = new ParticleSystem.MinMaxGradient(Shift(gradient.colorMin, hue), Shift(gradient.colorMax, hue));
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    result = new ParticleSystem.MinMaxGradient(Shift(gradient.gradientMin, hue), Shift(gradient.gradientMax, hue));
                    break;
                default:
                    result = new ParticleSystem.MinMaxGradient(Shift(gradient.gradient, hue));
                    break;
            }

            result.mode = gradient.mode;
            return result;
        }

        private static Gradient Shift(Gradient gradient, float hue)
        {
            if (gradient == null) return null;

            GradientColorKey[] colorKeys = gradient.colorKeys;
            for (int i = 0; i < colorKeys.Length; i++)
            {
                colorKeys[i].color = Shift(colorKeys[i].color, hue);
            }

            Gradient result = new Gradient { mode = gradient.mode };
            result.SetKeys(colorKeys, gradient.alphaKeys);
            return result;
        }

        // 색조만 교체 (HDR 밝기 유지)
        private static Color Shift(Color color, float hue)
        {
            Color.RGBToHSV(color, out _, out float saturation, out float value);
            Color shifted = Color.HSVToRGB(hue, saturation, value, true);
            shifted.a = color.a;
            return shifted;
        }
    }
}

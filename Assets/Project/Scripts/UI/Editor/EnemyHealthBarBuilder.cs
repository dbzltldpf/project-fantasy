using ProjectFantasy.Enemy;
using ProjectFantasy.UI;
using UnityEditor;
using UnityEngine;
using static ProjectFantasy.UIEditor.UIBuilderUtility;

namespace ProjectFantasy.UIEditor
{
    // 적 머리 위 체력바 프리팹 생성 (월드 공간 캔버스 + Kenney 3조각 바 + 표시 규칙 Presenter), 적 프리팹 자식으로 배치해 사용
    public static class EnemyHealthBarBuilder
    {
        private const string MenuPath = "Tools/ProjectFantasy/Create Enemy Health Bar Prefab";
        private const string Title = "Create Enemy Health Bar";
        private const string PrefabPath = "Assets/Project/Prefabs/UI/EnemyHealthBar.prefab";
        private const string FillSpritePrefix = "barRed";

        private const float BarWidth = 120f;
        private const float BarHeight = 14f;
        private const float CapWidth = 6f;
        private const float WorldScale = 0.01f;
        private const float HeadHeight = 2.3f;

        [MenuItem(MenuPath)]
        private static void Create()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null
                && !EditorUtility.DisplayDialog(Title, $"'{PrefabPath}'가 이미 있습니다. 덮어쓸까요? (적 프리팹에 넣은 체력바는 다시 확인 필요)", "덮어쓰기", "취소"))
            {
                return;
            }

            RectTransform canvasRect = CreateRect("EnemyHealthBar", null);
            canvasRect.sizeDelta = new Vector2(BarWidth, BarHeight);
            canvasRect.localScale = Vector3.one * WorldScale;
            canvasRect.localPosition = Vector3.up * HeadHeight;
            Canvas canvas = canvasRect.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform root = CreateRect("Root", canvasRect);
            Stretch(root);
            RectTransform fillMask = CreateMaskedBar(root, FillSpritePrefix, CapWidth, BarWidth, out _);

            EnemyHealthBarView view = canvasRect.gameObject.AddComponent<EnemyHealthBarView>();
            Assign(view, "root", root.gameObject);
            Assign(view, "fillMask", fillMask);
            canvasRect.gameObject.AddComponent<EnemyHealthBarPresenter>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(canvasRect.gameObject, PrefabPath);
            Object.DestroyImmediate(canvasRect.gameObject);
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"[{nameof(EnemyHealthBarBuilder)}] 생성 완료: {PrefabPath} (적 프리팹 자식으로 넣고 높이 조절)");
        }
    }
}

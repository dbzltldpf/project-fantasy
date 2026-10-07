using ProjectFantasy.Player;
using ProjectFantasy.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static ProjectFantasy.UIEditor.UIBuilderUtility;

namespace ProjectFantasy.UIEditor
{
    // 숙련도 창(K)을 씬에 생성하고 PlayerMenuPresenter에 연결 (Create Inventory UI 이후 실행, 줄 프리팹은 없을 때만 생성)
    public static class MasteryUIBuilder
    {
        private const string MenuPath = "Tools/ProjectFantasy/Create Mastery UI";
        private const string UndoName = "Create Mastery UI";
        private const string RootName = "Mastery UI";
        private const string InventoryRootName = "Inventory UI";
        private const string PrefabFolder = "Assets/Project/Prefabs/UI";
        private const string RowPrefabPath = PrefabFolder + "/MasteryRow.prefab";
        private const string BarSpriteFolder = "Assets/ThirdParty/Kenney/UIPack";
        private const string BackSpritePrefix = "barBack";
        private const string FillSpritePrefix = "barGreen";
        private const string LeftCapSuffix = "_horizontalLeft.png";
        private const string MidSuffix = "_horizontalMid.png";
        private const string RightCapSuffix = "_horizontalRight.png";
        private const float CapWidth = 9f;

        private const float WindowWidth = 760f;
        private const float WindowHeight = 560f;
        private const float Margin = 20f;
        private const float HeaderHeight = 40f;
        private const float ColumnSpacing = 8f;
        private const float RowSpacing = 6f;
        private const float RowHeight = 34f;
        private const float NameWidth = 150f;
        private const float LevelWidth = 80f;
        private const float TierWidth = 60f;
        private const float BarWidth = 300f;
        private const float BarHeight = 20f;
        private const float BonusWidth = 90f;
        private const float TitleFontSize = 28f;
        private const float RowFontSize = 18f;
        private const float BarFontSize = 14f;

        private static readonly Color WindowColor = new Color(0.08f, 0.08f, 0.1f, 0.92f);
        private static readonly Vector2 LeftMiddle = new Vector2(0f, 0.5f);
        private static readonly Vector2 RightMiddle = new Vector2(1f, 0.5f);
        private static readonly Color TierColor = new Color(1f, 0.85f, 0.2f);

        [MenuItem(MenuPath)]
        private static void Create()
        {
            PlayerMenuPresenter presenter = Object.FindFirstObjectByType<PlayerMenuPresenter>();
            if (presenter == null)
            {
                EditorUtility.DisplayDialog(UndoName, "먼저 Tools → ProjectFantasy → Create Inventory UI 를 실행하세요 (PlayerMenuPresenter 필요).", "확인");
                return;
            }

            Canvas canvas = FindOrCreateCanvas(UndoName);
            presenter = UIPresenterSetup.Gather(canvas, UndoName).GetComponent<PlayerMenuPresenter>();
            if (!ReplaceExistingRoot(canvas.transform, RootName, UndoName)) return;

            MasteryRowView rowPrefab = LoadOrCreateRowPrefab();

            RectTransform root = CreateRect(RootName, canvas.transform);
            Stretch(root);
            MasteryWindow window = CreateWindow(root, rowPrefab);

            // 인벤토리 UI(드래그 고스트 포함)보다 아래에 그림
            Transform inventoryRoot = canvas.transform.Find(InventoryRootName);
            if (inventoryRoot != null) root.SetSiblingIndex(inventoryRoot.GetSiblingIndex());

            Assign(presenter, "masteryWindow", window);
            Undo.RegisterCreatedObjectUndo(root.gameObject, UndoName);
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
            Selection.activeGameObject = root.gameObject;
            Debug.Log($"[{nameof(MasteryUIBuilder)}] '{RootName}' 생성 및 PlayerMenuPresenter 연결 완료 (줄 프리팹: {RowPrefabPath})");
        }

        private static MasteryWindow CreateWindow(RectTransform parent, MasteryRowView rowPrefab)
        {
            // 컴포넌트는 항상 활성인 바깥 오브젝트, 열고 닫는 대상은 Root
            RectTransform holder = CreateRect("MasteryWindow", parent);
            Stretch(holder);

            RectTransform panel = CreateRect("Root", holder);
            SetAnchor(panel, Center, Center, Vector2.zero, new Vector2(WindowWidth, WindowHeight));
            AddImage(panel.gameObject, WindowColor, true);

            RectTransform title = CreateRect("Title", panel);
            SetTopLeft(title, Margin, Margin, WindowWidth - Margin - Margin, HeaderHeight);
            CreateText("Text", title, "숙련도", TitleFontSize, TextAlignmentOptions.Left);

            float rowsTop = Margin + HeaderHeight + Margin;
            RectTransform rows = CreateRect("Rows", panel);
            SetTopLeft(rows, Margin, rowsTop, WindowWidth - Margin - Margin, WindowHeight - rowsTop - Margin);
            VerticalLayoutGroup layout = rows.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = RowSpacing;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            MasteryWindow window = holder.gameObject.AddComponent<MasteryWindow>();
            Assign(window, "root", panel.gameObject);
            Assign(window, "rowContainer", rows);
            Assign(window, "rowPrefab", rowPrefab);

            // 편집 화면을 가리지 않도록 닫힌 상태로 생성
            panel.gameObject.SetActive(false);
            return window;
        }

        // 사용자가 꾸민 프리팹을 덮어쓰지 않도록 없을 때만 생성
        private static MasteryRowView LoadOrCreateRowPrefab()
        {
            MasteryRowView existing = AssetDatabase.LoadAssetAtPath<MasteryRowView>(RowPrefabPath);
            if (existing != null) return existing;

            RectTransform row = CreateRect("MasteryRow", null);
            row.sizeDelta = new Vector2(WindowWidth - Margin - Margin, RowHeight);
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = ColumnSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            TMP_Text nameLabel = CreateColumnText(row, "Name", NameWidth, TextAlignmentOptions.Left);
            TMP_Text levelLabel = CreateColumnText(row, "Level", LevelWidth, TextAlignmentOptions.Center);
            TMP_Text tierLabel = CreateColumnText(row, "Tier", TierWidth, TextAlignmentOptions.Center);
            tierLabel.color = TierColor;

            RectTransform bar = CreateRect("Experience", row);
            bar.sizeDelta = new Vector2(BarWidth, BarHeight);
            RectTransform back = CreateCappedBar(bar, "Back", BackSpritePrefix);
            Stretch(back);

            // 채움: 왼쪽 기준, 너비를 경험치 비율로 조절 (MasteryRowView)
            RectTransform fill = CreateCappedBar(bar, "Fill", FillSpritePrefix);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.up;
            fill.pivot = LeftMiddle;
            fill.anchoredPosition = Vector2.zero;
            fill.sizeDelta = new Vector2(BarWidth, 0f);
            TMP_Text experienceLabel = CreateText("Label", bar, string.Empty, BarFontSize, TextAlignmentOptions.Center);

            TMP_Text bonusLabel = CreateColumnText(row, "Bonus", BonusWidth, TextAlignmentOptions.Right);

            MasteryRowView view = row.gameObject.AddComponent<MasteryRowView>();
            Assign(view, "nameLabel", nameLabel);
            Assign(view, "levelLabel", levelLabel);
            Assign(view, "tierLabel", tierLabel);
            Assign(view, "experienceFill", fill);
            SerializedObject viewObject = new SerializedObject(view);
            viewObject.FindProperty("minFillWidth").floatValue = CapWidth + CapWidth;
            viewObject.ApplyModifiedPropertiesWithoutUndo();
            Assign(view, "experienceLabel", experienceLabel);
            Assign(view, "bonusLabel", bonusLabel);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(row.gameObject, RowPrefabPath);
            Object.DestroyImmediate(row.gameObject);
            return prefab.GetComponent<MasteryRowView>();
        }

        // Kenney 바: 왼쪽 끝(고정) | 가운데(늘어남) | 오른쪽 끝(고정)
        private static RectTransform CreateCappedBar(RectTransform parent, string name, string spritePrefix)
        {
            RectTransform container = CreateRect(name, parent);

            RectTransform left = CreateRect("Left", container);
            SetVerticalStretch(left, Vector2.zero, LeftMiddle, CapWidth);
            AddBarImage(left, spritePrefix + LeftCapSuffix);

            RectTransform middle = CreateRect("Mid", container);
            Stretch(middle);
            middle.offsetMin = new Vector2(CapWidth, 0f);
            middle.offsetMax = new Vector2(-CapWidth, 0f);
            AddBarImage(middle, spritePrefix + MidSuffix);

            RectTransform right = CreateRect("Right", container);
            SetVerticalStretch(right, Vector2.right, RightMiddle, CapWidth);
            AddBarImage(right, spritePrefix + RightCapSuffix);
            return container;
        }

        // 세로로 꽉 차고 가로는 고정 너비 (anchorX: 0 = 왼쪽, 1 = 오른쪽)
        private static void SetVerticalStretch(RectTransform rect, Vector2 anchorX, Vector2 pivot, float width)
        {
            rect.anchorMin = new Vector2(anchorX.x, 0f);
            rect.anchorMax = new Vector2(anchorX.x, 1f);
            rect.pivot = pivot;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, 0f);
        }

        private static void AddBarImage(RectTransform rect, string spriteFile)
        {
            Image image = AddImage(rect.gameObject, Color.white, false);
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{BarSpriteFolder}/{spriteFile}");
            if (image.sprite == null) Debug.LogWarning($"[{nameof(MasteryUIBuilder)}] 스프라이트를 찾지 못했습니다: {spriteFile}");
        }

        private static TMP_Text CreateColumnText(RectTransform row, string name, float width, TextAlignmentOptions alignment)
        {
            RectTransform column = CreateRect(name, row);
            column.sizeDelta = new Vector2(width, RowHeight);
            return CreateText("Text", column, string.Empty, RowFontSize, alignment);
        }
    }
}

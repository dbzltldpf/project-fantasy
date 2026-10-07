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
    // 플레이어 상태바(하단 중앙, 퀵슬롯 위)를 씬에 생성하고 PlayerHudPresenter에 연결 (Create Inventory UI 이후 실행)
    // 자원 바는 Status Bars 세로 레이아웃에 한 줄씩 쌓임 (지금은 체력, 자원 단계에서 스태미나·마나 추가)
    public static class PlayerStatusUIBuilder
    {
        private const string MenuPath = "Tools/ProjectFantasy/Create Player Status UI";
        private const string UndoName = "Create Player Status UI";
        private const string RootName = "Status UI";
        private const string InventoryRootName = "Inventory UI";
        private const string HealthSpritePrefix = "barRed";

        // 퀵슬롯 바(하단 20~100) 위, 줍기 안내(140) 아래
        private const float BottomY = 108f;
        private const float BarWidth = 480f;
        private const float BarHeight = 24f;
        private const float BarSpacing = 4f;
        private const float CapWidth = 9f;
        private const float ValueFontSize = 16f;

        [MenuItem(MenuPath)]
        private static void Create()
        {
            if (Object.FindFirstObjectByType<PlayerHudPresenter>() == null)
            {
                EditorUtility.DisplayDialog(UndoName, "먼저 Tools → ProjectFantasy → Create Inventory UI 를 실행하세요 (PlayerHudPresenter 필요).", "확인");
                return;
            }

            Canvas canvas = FindOrCreateCanvas(UndoName);
            PlayerHudPresenter presenter = UIPresenterSetup.Gather(canvas, UndoName).GetComponent<PlayerHudPresenter>();
            if (!ReplaceExistingRoot(canvas.transform, RootName, UndoName)) return;

            RectTransform root = CreateRect(RootName, canvas.transform);
            Stretch(root);

            RectTransform bars = CreateRect("Status Bars", root);
            SetAnchor(bars, BottomCenter, BottomCenter, new Vector2(0f, BottomY), new Vector2(BarWidth, BarHeight));
            VerticalLayoutGroup layout = bars.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = BarSpacing;
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = bars.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            StatusBarView healthBar = CreateStatusBar(bars, "HealthBar", HealthSpritePrefix);
            Assign(presenter, "healthBar", healthBar);

            // HUD라 인벤토리·숙련도 창보다 아래에 그림
            Transform inventoryRoot = canvas.transform.Find(InventoryRootName);
            if (inventoryRoot != null) root.SetSiblingIndex(inventoryRoot.GetSiblingIndex());

            Undo.RegisterCreatedObjectUndo(root.gameObject, UndoName);
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
            Selection.activeGameObject = root.gameObject;
            Debug.Log($"[{nameof(PlayerStatusUIBuilder)}] '{RootName}' 생성 및 PlayerHudPresenter 연결 완료");
        }

        // 한 줄: 어두운 배경 + 마스크 채움(깜빡임용 CanvasGroup) + 가운데 수치
        private static StatusBarView CreateStatusBar(RectTransform parent, string name, string spritePrefix)
        {
            RectTransform bar = CreateRect(name, parent);
            bar.sizeDelta = new Vector2(BarWidth, BarHeight);

            RectTransform fillMask = CreateMaskedBar(bar, spritePrefix, CapWidth, BarWidth, out RectTransform fill);
            CanvasGroup fillGroup = fill.gameObject.AddComponent<CanvasGroup>();
            fillGroup.blocksRaycasts = false;
            TMP_Text valueLabel = CreateText("Value", bar, string.Empty, ValueFontSize, TextAlignmentOptions.Center);

            StatusBarView view = bar.gameObject.AddComponent<StatusBarView>();
            Assign(view, "fillMask", fillMask);
            Assign(view, "fillGroup", fillGroup);
            Assign(view, "valueLabel", valueLabel);
            return view;
        }
    }
}

using ProjectFantasy.CameraSystem;
using ProjectFantasy.Player;
using ProjectFantasy.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ProjectFantasy.UIEditor
{
    // 인벤토리 창·퀵슬롯 바·HUD·Presenter 계층을 씬에 생성하고 참조 연결 (슬롯 프리팹은 없을 때만 생성)
    public static class InventoryUIBuilder
    {
        private const string MenuPath = "Tools/ProjectFantasy/Create Inventory UI";
        private const string UndoName = "Create Inventory UI";
        private const string RootName = "Inventory UI";
        private const string SlotPrefabFolder = "Assets/Project/Prefabs/UI";
        private const string SlotPrefabPath = SlotPrefabFolder + "/ItemSlot.prefab";

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float WidthHeightMatch = 0.5f;

        private const float SlotSize = 80f;
        private const float SlotSpacing = 6f;
        private const float SlotPadding = 6f;
        private const int GridColumns = 6;
        private const float WindowWidth = 920f;
        private const float WindowHeight = 600f;
        private const float WindowMargin = 20f;
        private const float HeaderHeight = 40f;
        private const float FilterButtonWidth = 96f;
        private const float RightPanelX = 580f;
        private const float EquipLabelHeight = 24f;
        private const float DetailTop = 200f;
        private const float DetailWidth = 320f;
        private const float DetailNameHeight = 36f;
        private const float DetailStatsHeight = 100f;
        private const float DetailDescriptionHeight = 120f;
        private const float MenuWidth = 160f;
        private const float MenuButtonHeight = 36f;
        private const float GhostSize = 64f;
        private const float HudBottomMargin = 20f;
        private const float AmmoWidth = 220f;
        private const float AmmoHeight = 50f;
        private const float PromptY = 140f;
        private const float NoticeY = 200f;
        private const float LabelWidth = 480f;
        private const float LabelHeight = 40f;
        private const float SmallLabelSize = 24f;

        private const float TitleFontSize = 28f;
        private const float LabelFontSize = 22f;
        private const float SmallFontSize = 16f;
        private const float SlotNameFontSize = 13f;

        private static readonly Color WindowColor = new Color(0.08f, 0.08f, 0.1f, 0.92f);
        private static readonly Color SlotColor = new Color(0.18f, 0.18f, 0.2f, 0.9f);
        private static readonly Color ButtonColor = new Color(0.25f, 0.25f, 0.3f, 1f);
        private static readonly Color SelectedColor = new Color(1f, 0.85f, 0.2f, 0.35f);
        private static readonly Color EquippedColor = new Color(1f, 0.85f, 0.2f);
        private static readonly Color TransparentColor = new Color(0f, 0f, 0f, 0f);
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);
        private static readonly Vector2 BottomRight = new Vector2(1f, 0f);

        [MenuItem(MenuPath)]
        private static void Create()
        {
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            ThirdPersonCamera thirdPersonCamera = Object.FindFirstObjectByType<ThirdPersonCamera>();
            if (player == null || thirdPersonCamera == null)
            {
                EditorUtility.DisplayDialog(UndoName, "씬에 PlayerController와 ThirdPersonCamera가 있어야 합니다.", "확인");
                return;
            }

            Canvas canvas = FindOrCreateCanvas();
            if (!ReplaceExistingRoot(canvas)) return;

            EnsureEventSystem();
            ItemSlotView slotPrefab = LoadOrCreateSlotPrefab();

            RectTransform root = CreateRect(RootName, canvas.transform);
            Stretch(root);

            // 생성 순서 = 그리기 순서 (HUD → 창 → 우클릭 메뉴 → 드래그 고스트)
            QuickSlotBarView quickSlotBar = CreateQuickSlotBar(root, slotPrefab);
            AmmoCounterView ammoCounter = CreateAmmoCounter(root);
            InteractPromptView prompt = CreateLabelView<InteractPromptView>(root, "InteractPrompt", PromptY);
            NoticeView notice = CreateLabelView<NoticeView>(root, "Notice", NoticeY);
            InventoryWindow window = CreateInventoryWindow(root, slotPrefab, out ItemContextMenu contextMenu);
            ItemDragGhost ghost = CreateDragGhost(root);

            Assign(quickSlotBar, "dragGhost", ghost);
            Assign(window, "contextMenu", contextMenu);
            Assign(window, "dragGhost", ghost);

            PlayerMenuPresenter menuPresenter = root.gameObject.AddComponent<PlayerMenuPresenter>();
            Assign(menuPresenter, "player", player);
            Assign(menuPresenter, "thirdPersonCamera", thirdPersonCamera);
            Assign(menuPresenter, "inventoryWindow", window);

            PlayerHudPresenter hudPresenter = root.gameObject.AddComponent<PlayerHudPresenter>();
            Assign(hudPresenter, "player", player);
            Assign(hudPresenter, "quickSlotBar", quickSlotBar);
            Assign(hudPresenter, "ammoCounter", ammoCounter);
            Assign(hudPresenter, "interactPrompt", prompt);
            Assign(hudPresenter, "notice", notice);

            root.SetAsLastSibling();
            Undo.RegisterCreatedObjectUndo(root.gameObject, UndoName);
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
            Selection.activeGameObject = root.gameObject;
            Debug.Log($"[{nameof(InventoryUIBuilder)}] '{RootName}' 생성 완료 (슬롯 프리팹: {SlotPrefabPath})");
        }

        #region Scene Setup

        private static Canvas FindOrCreateCanvas()
        {
            foreach (Canvas existing in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (existing.isRootCanvas && existing.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    if (existing.GetComponent<GraphicRaycaster>() == null) Undo.AddComponent<GraphicRaycaster>(existing.gameObject);
                    return existing;
                }
            }

            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, UndoName);
            canvasObject.layer = LayerMask.NameToLayer("UI");

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = WidthHeightMatch;
            return canvas;
        }

        // 이미 있으면 다시 만들지 확인
        private static bool ReplaceExistingRoot(Canvas canvas)
        {
            Transform existing = canvas.transform.Find(RootName);
            if (existing == null) return true;
            if (!EditorUtility.DisplayDialog(UndoName, $"'{RootName}'이 이미 있습니다. 삭제하고 다시 만들까요?", "다시 만들기", "취소")) return false;

            Undo.DestroyObjectImmediate(existing.gameObject);
            return true;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystemObject, UndoName);
                return;
            }

            if (eventSystem.GetComponent<StandaloneInputModule>() != null)
            {
                Debug.LogWarning($"[{nameof(InventoryUIBuilder)}] EventSystem에 StandaloneInputModule이 있습니다. Input System UI Input Module로 교체하세요.", eventSystem);
            }
        }

        #endregion

        #region Slot Prefab

        // 사용자가 꾸민 프리팹을 덮어쓰지 않도록 없을 때만 생성
        private static ItemSlotView LoadOrCreateSlotPrefab()
        {
            ItemSlotView existing = AssetDatabase.LoadAssetAtPath<ItemSlotView>(SlotPrefabPath);
            if (existing != null) return existing;

            RectTransform slot = CreateRect("ItemSlot", null);
            slot.sizeDelta = new Vector2(SlotSize, SlotSize);
            AddImage(slot.gameObject, SlotColor, true);
            CanvasGroup canvasGroup = slot.gameObject.AddComponent<CanvasGroup>();

            Image icon = AddImage(CreateStretched("Icon", slot, SlotPadding).gameObject, Color.white, false);
            icon.preserveAspect = true;
            TMP_Text nameLabel = CreateText("Name", CreateStretched("NameArea", slot, SlotPadding), string.Empty, SlotNameFontSize, TextAlignmentOptions.Center);
            TMP_Text countLabel = CreateText("Count", CreateStretched("CountArea", slot, SlotPadding), string.Empty, SmallFontSize, TextAlignmentOptions.BottomRight);
            TMP_Text keyLabel = CreateText("Key", CreateStretched("KeyArea", slot, SlotPadding), string.Empty, SmallFontSize, TextAlignmentOptions.TopLeft);

            TMP_Text equipped = CreateText("Equipped", CreateStretched("EquippedMark", slot, SlotPadding), "E", SmallFontSize, TextAlignmentOptions.TopRight);
            equipped.color = EquippedColor;
            GameObject equippedMark = equipped.transform.parent.gameObject;
            equippedMark.SetActive(false);

            GameObject selectedFrame = CreateStretched("SelectedFrame", slot, 0f).gameObject;
            AddImage(selectedFrame, SelectedColor, false);
            selectedFrame.SetActive(false);

            ItemSlotView view = slot.gameObject.AddComponent<ItemSlotView>();
            Assign(view, "icon", icon);
            Assign(view, "nameLabel", nameLabel);
            Assign(view, "countLabel", countLabel);
            Assign(view, "keyLabel", keyLabel);
            Assign(view, "equippedMark", equippedMark);
            Assign(view, "selectedFrame", selectedFrame);
            Assign(view, "canvasGroup", canvasGroup);

            if (!AssetDatabase.IsValidFolder(SlotPrefabFolder)) AssetDatabase.CreateFolder("Assets/Project/Prefabs", "UI");
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(slot.gameObject, SlotPrefabPath);
            Object.DestroyImmediate(slot.gameObject);
            return prefab.GetComponent<ItemSlotView>();
        }

        #endregion

        #region Views

        private static QuickSlotBarView CreateQuickSlotBar(RectTransform parent, ItemSlotView slotPrefab)
        {
            RectTransform bar = CreateRect("QuickSlotBar", parent);
            SetAnchor(bar, BottomCenter, BottomCenter, new Vector2(0f, HudBottomMargin), Vector2.zero);

            HorizontalLayoutGroup layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = SlotSpacing;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            ContentSizeFitter fitter = bar.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            QuickSlotBarView view = bar.gameObject.AddComponent<QuickSlotBarView>();
            Assign(view, "slotContainer", bar);
            Assign(view, "slotPrefab", slotPrefab);
            return view;
        }

        private static AmmoCounterView CreateAmmoCounter(RectTransform parent)
        {
            RectTransform counter = CreateRect("AmmoCounter", parent);
            SetAnchor(counter, BottomRight, BottomRight, new Vector2(-WindowMargin, HudBottomMargin), new Vector2(AmmoWidth, AmmoHeight));
            AddImage(counter.gameObject, WindowColor, false);

            TMP_Text count = CreateText("Count", CreateStretched("CountArea", counter, SlotPadding), "0", LabelFontSize, TextAlignmentOptions.Left);
            TMP_Text loaded = CreateText("Loaded", CreateStretched("LoadedMark", counter, SlotPadding), "장전", SmallFontSize, TextAlignmentOptions.Right);
            loaded.color = EquippedColor;

            AmmoCounterView view = counter.gameObject.AddComponent<AmmoCounterView>();
            Assign(view, "countLabel", count);
            Assign(view, "loadedMark", loaded.transform.parent.gameObject);
            return view;
        }

        // 화면 하단 중앙 문구 (줍기 안내, 알림)
        private static T CreateLabelView<T>(RectTransform parent, string name, float y) where T : Component
        {
            RectTransform rect = CreateRect(name, parent);
            SetAnchor(rect, BottomCenter, BottomCenter, new Vector2(0f, y), new Vector2(LabelWidth, LabelHeight));
            TMP_Text label = CreateText("Label", CreateStretched("LabelArea", rect, 0f), string.Empty, LabelFontSize, TextAlignmentOptions.Center);

            T view = rect.gameObject.AddComponent<T>();
            Assign(view, "label", label);
            return view;
        }

        private static InventoryWindow CreateInventoryWindow(RectTransform parent, ItemSlotView slotPrefab, out ItemContextMenu contextMenu)
        {
            // 컴포넌트는 항상 활성인 바깥 오브젝트, 열고 닫는 대상은 Root
            RectTransform holder = CreateRect("InventoryWindow", parent);
            Stretch(holder);

            RectTransform window = CreateRect("Root", holder);
            SetAnchor(window, Center, Center, Vector2.zero, new Vector2(WindowWidth, WindowHeight));
            AddImage(window.gameObject, WindowColor, true);

            RectTransform title = CreateRect("Title", window);
            SetTopLeft(title, WindowMargin, WindowMargin, WindowWidth, HeaderHeight);
            CreateText("Text", title, "인벤토리", TitleFontSize, TextAlignmentOptions.Left);

            InventoryWindow component = holder.gameObject.AddComponent<InventoryWindow>();
            SerializedObject windowObject = new SerializedObject(component);
            float filterTop = WindowMargin + HeaderHeight + SlotSpacing;
            CreateFilterButtons(window, windowObject, filterTop);

            float gridTop = filterTop + HeaderHeight + WindowMargin;
            RectTransform grid = CreateRect("Slots", window);
            float gridWidth = GridColumns * (SlotSize + SlotSpacing);
            SetTopLeft(grid, WindowMargin, gridTop, gridWidth, WindowHeight - gridTop - WindowMargin);
            GridLayoutGroup layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(SlotSize, SlotSize);
            layout.spacing = new Vector2(SlotSpacing, SlotSpacing);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = GridColumns;

            ItemSlotView weaponSlot = CreateEquipmentSlot(window, slotPrefab, "WeaponSlot", "무기", RightPanelX, gridTop);
            ItemSlotView offHandSlot = CreateEquipmentSlot(window, slotPrefab, "OffHandSlot", "보조", RightPanelX + SlotSize + WindowMargin, gridTop);

            RectTransform detailName = CreateRect("DetailName", window);
            SetTopLeft(detailName, RightPanelX, gridTop + DetailTop, DetailWidth, DetailNameHeight);
            TMP_Text nameText = CreateText("Text", detailName, string.Empty, LabelFontSize, TextAlignmentOptions.TopLeft);

            RectTransform detailStats = CreateRect("DetailStats", window);
            SetTopLeft(detailStats, RightPanelX, gridTop + DetailTop + DetailNameHeight, DetailWidth, DetailStatsHeight);
            TMP_Text statsText = CreateText("Text", detailStats, string.Empty, SmallFontSize, TextAlignmentOptions.TopLeft);

            RectTransform detailDescription = CreateRect("DetailDescription", window);
            SetTopLeft(detailDescription, RightPanelX, gridTop + DetailTop + DetailNameHeight + DetailStatsHeight, DetailWidth, DetailDescriptionHeight);
            TMP_Text descriptionText = CreateText("Text", detailDescription, string.Empty, SmallFontSize, TextAlignmentOptions.TopLeft);

            windowObject.Update();
            windowObject.FindProperty("root").objectReferenceValue = window.gameObject;
            windowObject.FindProperty("slotContainer").objectReferenceValue = grid;
            windowObject.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
            windowObject.FindProperty("weaponSlot").objectReferenceValue = weaponSlot;
            windowObject.FindProperty("offHandSlot").objectReferenceValue = offHandSlot;
            windowObject.FindProperty("detailName").objectReferenceValue = nameText;
            windowObject.FindProperty("detailStats").objectReferenceValue = statsText;
            windowObject.FindProperty("detailDescription").objectReferenceValue = descriptionText;
            windowObject.ApplyModifiedPropertiesWithoutUndo();

            // 편집 화면을 가리지 않도록 닫힌 상태로 생성 (플레이 시에도 닫힌 상태로 시작)
            window.gameObject.SetActive(false);
            contextMenu = CreateContextMenu(parent);
            return component;
        }

        private static void CreateFilterButtons(RectTransform window, SerializedObject windowObject, float top)
        {
            (string label, InventoryWindow.Filter filter)[] filters =
            {
                ("전체", InventoryWindow.Filter.All),
                ("무기", InventoryWindow.Filter.Weapon),
                ("장비", InventoryWindow.Filter.OffHand),
                ("화살", InventoryWindow.Filter.Ammo),
                ("소모품", InventoryWindow.Filter.Consumable),
            };

            SerializedProperty buttons = windowObject.FindProperty("filterButtons");
            buttons.arraySize = filters.Length;

            for (int i = 0; i < filters.Length; i++)
            {
                RectTransform rect = CreateRect($"Filter_{filters[i].filter}", window);
                SetTopLeft(rect, WindowMargin + i * (FilterButtonWidth + SlotSpacing), top, FilterButtonWidth, HeaderHeight);
                Button button = CreateButton(rect, filters[i].label);

                SerializedProperty element = buttons.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("button").objectReferenceValue = button;
                element.FindPropertyRelative("filter").enumValueIndex = (int)filters[i].filter;
            }

            windowObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ItemSlotView CreateEquipmentSlot(RectTransform window, ItemSlotView slotPrefab, string name, string label, float x, float top)
        {
            RectTransform caption = CreateRect($"{name}Label", window);
            SetTopLeft(caption, x, top, SlotSize, EquipLabelHeight);
            CreateText("Text", caption, label, SmallFontSize, TextAlignmentOptions.Center);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab.gameObject, window);
            instance.name = name;
            ItemSlotView slot = instance.GetComponent<ItemSlotView>();
            SetTopLeft((RectTransform)slot.transform, x, top + EquipLabelHeight, SlotSize, SlotSize);
            return slot;
        }

        private static ItemContextMenu CreateContextMenu(RectTransform parent)
        {
            RectTransform holder = CreateRect("ItemContextMenu", parent);
            Stretch(holder);

            RectTransform blocker = CreateRect("Root", holder);
            Stretch(blocker);
            AddImage(blocker.gameObject, TransparentColor, true);
            Button blockerButton = blocker.gameObject.AddComponent<Button>();
            blockerButton.transition = Selectable.Transition.None;

            RectTransform panel = CreateRect("Panel", blocker);
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.zero;
            panel.pivot = TopLeft;
            panel.sizeDelta = new Vector2(MenuWidth, 0f);
            AddImage(panel.gameObject, WindowColor, true);
            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ItemContextMenu menu = holder.gameObject.AddComponent<ItemContextMenu>();
            Assign(menu, "root", blocker.gameObject);
            Assign(menu, "panel", panel);
            Assign(menu, "outsideBlocker", blockerButton);
            Assign(menu, "equipButton", CreateMenuButton(panel, "Equip", "장착"));
            Assign(menu, "unequipButton", CreateMenuButton(panel, "Unequip", "해제"));
            Assign(menu, "useButton", CreateMenuButton(panel, "Use", "사용"));
            Assign(menu, "dropButton", CreateMenuButton(panel, "Drop", "버리기"));

            blocker.gameObject.SetActive(false);
            return menu;
        }

        private static Button CreateMenuButton(RectTransform panel, string name, string label)
        {
            RectTransform rect = CreateRect(name, panel);
            rect.sizeDelta = new Vector2(MenuWidth, MenuButtonHeight);
            return CreateButton(rect, label);
        }

        private static ItemDragGhost CreateDragGhost(RectTransform parent)
        {
            RectTransform ghost = CreateRect("ItemDragGhost", parent);
            SetAnchor(ghost, Vector2.zero, Center, Vector2.zero, new Vector2(GhostSize, GhostSize));
            Image icon = AddImage(ghost.gameObject, Color.white, false);
            icon.preserveAspect = true;
            TMP_Text name = CreateText("Name", CreateStretched("NameArea", ghost, 0f), string.Empty, SlotNameFontSize, TextAlignmentOptions.Center);

            ItemDragGhost view = ghost.gameObject.AddComponent<ItemDragGhost>();
            Assign(view, "root", ghost);
            Assign(view, "icon", icon);
            Assign(view, "nameLabel", name);
            return view;
        }

        #endregion

        #region Helpers

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.layer = LayerMask.NameToLayer("UI");
            RectTransform rect = (RectTransform)gameObject.transform;
            if (parent != null) rect.SetParent(parent, false);
            return rect;
        }

        private static RectTransform CreateStretched(string name, Transform parent, float padding)
        {
            RectTransform rect = CreateRect(name, parent);
            Stretch(rect);
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetAnchor(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        // 부모 왼쪽 위 기준 배치 (top은 아래로 증가)
        private static void SetTopLeft(RectTransform rect, float x, float top, float width, float height)
        {
            SetAnchor(rect, TopLeft, TopLeft, new Vector2(x, -top), new Vector2(width, height));
        }

        private static Image AddImage(GameObject target, Color color, bool raycastTarget)
        {
            Image image = target.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            RectTransform rect = CreateRect(name, parent);
            Stretch(rect);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.raycastTarget = false;
            return label;
        }

        private static Button CreateButton(RectTransform rect, string label)
        {
            AddImage(rect.gameObject, ButtonColor, true);
            Button button = rect.gameObject.AddComponent<Button>();
            CreateText("Label", rect, label, SmallFontSize, TextAlignmentOptions.Center);
            return button;
        }

        private static void Assign(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        #endregion
    }
}

using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ProjectFantasy.UIEditor
{
    // UI 빌더 공용: 캔버스·EventSystem 준비, RectTransform·이미지·텍스트·버튼 생성, 직렬화 참조 연결
    internal static class UIBuilderUtility
    {
        private const string UILayerName = "UI";
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float WidthHeightMatch = 0.5f;

        public static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        public static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        public static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);
        public static readonly Vector2 BottomRight = new Vector2(1f, 0f);

        #region Scene Setup

        // 기존 Screen Space Overlay 루트 캔버스 사용, 없으면 1920×1080 기준으로 생성
        public static Canvas FindOrCreateCanvas(string undoName)
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
            Undo.RegisterCreatedObjectUndo(canvasObject, undoName);
            canvasObject.layer = LayerMask.NameToLayer(UILayerName);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.matchWidthOrHeight = WidthHeightMatch;
            return canvas;
        }

        // 이미 있으면 다시 만들지 확인 (취소하면 false)
        public static bool ReplaceExistingRoot(Transform parent, string rootName, string undoName)
        {
            Transform existing = parent.Find(rootName);
            if (existing == null) return true;
            if (!EditorUtility.DisplayDialog(undoName, $"'{rootName}'이 이미 있습니다. 삭제하고 다시 만들까요?", "다시 만들기", "취소")) return false;

            Undo.DestroyObjectImmediate(existing.gameObject);
            return true;
        }

        public static void EnsureEventSystem(string undoName)
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystemObject, undoName);
                return;
            }

            if (eventSystem.GetComponent<StandaloneInputModule>() != null)
            {
                Debug.LogWarning($"[{undoName}] EventSystem에 StandaloneInputModule이 있습니다. Input System UI Input Module로 교체하세요.", eventSystem);
            }
        }

        #endregion

        #region Elements

        public static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.layer = LayerMask.NameToLayer(UILayerName);
            RectTransform rect = (RectTransform)gameObject.transform;
            if (parent != null) rect.SetParent(parent, false);
            return rect;
        }

        public static RectTransform CreateStretched(string name, Transform parent, float padding)
        {
            RectTransform rect = CreateRect(name, parent);
            Stretch(rect);
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
            return rect;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void SetAnchor(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        // 부모 왼쪽 위 기준 배치 (top은 아래로 증가)
        public static void SetTopLeft(RectTransform rect, float x, float top, float width, float height)
        {
            SetAnchor(rect, TopLeft, TopLeft, new Vector2(x, -top), new Vector2(width, height));
        }

        public static Image AddImage(GameObject target, Color color, bool raycastTarget)
        {
            Image image = target.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
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

        public static Button CreateButton(RectTransform rect, string label, Color color, float fontSize)
        {
            AddImage(rect.gameObject, color, true);
            Button button = rect.gameObject.AddComponent<Button>();
            CreateText("Label", rect, label, fontSize, TextAlignmentOptions.Center);
            return button;
        }

        public static void Assign(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        #endregion
    }
}

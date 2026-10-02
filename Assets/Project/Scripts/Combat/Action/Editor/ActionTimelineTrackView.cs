using System;
using ProjectFantasy.Combat;
using UnityEditor;
using UnityEngine;

namespace ProjectFantasy.CombatEditor
{
    // 눈금자·이벤트 트랙 그리기와 드래그 편집 (프레임 단위 스냅, 드래그 1회 = Undo 1회)
    internal sealed class ActionTimelineTrackView
    {
        public const int NoSelection = -1;

        private const float LabelWidth = 160f;
        private const float RulerHeight = 20f;
        private const float RowHeight = 22f;
        private const float BarPadding = 3f;
        private const float EdgeGrabWidth = 6f;
        private const float MarkerHalfWidth = 4f;
        private const float LineWidth = 2f;
        private const float OutlineWidth = 1f;
        private const float MinPixelsPerTick = 4f;
        private const float MinorTickHeight = 4f;
        private const float MajorTickHeight = 10f;
        private const float LabelIndent = 4f;
        private const int MajorTickInterval = 5;
        private const int MinFrameSpan = 1;
        private const int MinTotalFrames = 1;
        private const int RightMouseButton = 1;
        private const string LengthLabel = "Length";
        private const string WarningIconName = "console.warnicon.sml";

        private static readonly Color RulerColor = new Color(0.15f, 0.15f, 0.15f);
        private static readonly Color RowColor = new Color(0.21f, 0.21f, 0.21f);
        private static readonly Color AlternateRowColor = new Color(0.24f, 0.24f, 0.24f);
        private static readonly Color SelectedRowColor = new Color(0.25f, 0.4f, 0.6f, 0.6f);
        private static readonly Color TickColor = new Color(1f, 1f, 1f, 0.3f);
        private static readonly Color OutOfLengthColor = new Color(0f, 0f, 0f, 0.4f);
        private static readonly Color OutlineColor = Color.white;
        private static readonly Color TransitionColor = Color.white;
        private static readonly Color LengthColor = new Color(1f, 0.8f, 0.2f);
        private static readonly Color PlayheadColor = new Color(1f, 0.3f, 0.3f);

        private enum DragMode { None, Scrub, Move, ResizeStart, ResizeEnd, Transition, Length }

        private DragMode dragMode;
        private int dragOriginFrame;
        private int dragStartValue;
        private int dragEndValue;
        private int dragTransitionValue;
        private int undoGroup;
        private GUIContent warningIcon;

        public int SelectedIndex { get; set; } = NoSelection;

        // 컨텍스트 메뉴로 이벤트가 추가·삭제됐을 때 (창 다시 그리기용)
        public event Action Changed;

        public static float GetHeight(int eventCount) => RulerHeight + (eventCount + 1) * RowHeight;

        // 스크럽으로 바뀐 현재 프레임 반환
        public float Draw(Rect rect, ActionData action, SerializedObject actionObject, float currentFrame, int totalFrames)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            SerializedProperty events = actionObject.FindProperty(ActionEditorUtility.EventsPropertyName);
            SerializedProperty length = actionObject.FindProperty(ActionEditorUtility.LengthFramesPropertyName);
            TimelineLayout layout = new TimelineLayout(rect, Mathf.Max(MinTotalFrames, totalFrames));

            Event current = Event.current;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.Repaint:
                    DrawTracks(layout, action, events, length.intValue, currentFrame);
                    break;
                case EventType.MouseDown:
                    currentFrame = HandleMouseDown(current, controlId, layout, actionObject, events, length, currentFrame);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId) currentFrame = HandleDrag(current, layout, events, length, currentFrame);
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId) EndDrag(current);
                    break;
            }

            return currentFrame;
        }

        #region Drawing

        private void DrawTracks(TimelineLayout layout, ActionData action, SerializedProperty events, int lengthFrames, float currentFrame)
        {
            DrawRuler(layout);

            for (int i = 0; i < events.arraySize; i++)
            {
                SerializedProperty element = events.GetArrayElementAtIndex(i);
                DrawRowBackground(layout, i);
                DrawRowLabel(layout, i, $"{i}. {ActionEditorUtility.GetEventName(element)}", ActionEditorUtility.GetWarning(action, i));
                DrawEventBar(layout, i, element);
            }

            int lengthRow = events.arraySize;
            DrawRowBackground(layout, lengthRow);
            DrawRowLabel(layout, lengthRow, LengthLabel, null);

            // 액션 종료 이후 구간은 어둡게
            float lengthX = layout.FrameToX(lengthFrames);
            EditorGUI.DrawRect(new Rect(lengthX, layout.Ruler.yMax, layout.Timeline.xMax - lengthX, layout.TracksHeight), OutOfLengthColor);
            DrawVerticalLine(lengthX, layout.Ruler.yMax, layout.TracksHeight, LengthColor);
            EditorGUIUtility.AddCursorRect(GrabRect(lengthX, layout.RowRect(lengthRow)), MouseCursor.ResizeHorizontal);

            DrawVerticalLine(layout.FrameToX(currentFrame), layout.Ruler.y, layout.Ruler.height + layout.TracksHeight, PlayheadColor);
        }

        private static void DrawRuler(TimelineLayout layout)
        {
            Rect ruler = layout.Ruler;
            EditorGUI.DrawRect(new Rect(layout.Bounds.x, ruler.y, layout.Bounds.width, ruler.height), RulerColor);

            bool drawMinorTicks = layout.PixelsPerFrame >= MinPixelsPerTick;
            for (int frame = 0; frame <= layout.TotalFrames; frame++)
            {
                bool isMajor = frame % MajorTickInterval == 0;
                if (!isMajor && !drawMinorTicks) continue;

                float x = layout.FrameToX(frame);
                float tickHeight = isMajor ? MajorTickHeight : MinorTickHeight;
                EditorGUI.DrawRect(new Rect(x, ruler.yMax - tickHeight, OutlineWidth, tickHeight), TickColor);
                if (isMajor) GUI.Label(new Rect(x + OutlineWidth, ruler.y, ruler.width, ruler.height), frame.ToString(), EditorStyles.miniLabel);
            }
        }

        private void DrawRowBackground(TimelineLayout layout, int row)
        {
            Rect rowRect = layout.FullRowRect(row);
            EditorGUI.DrawRect(rowRect, row % 2 == 0 ? RowColor : AlternateRowColor);
            if (row == SelectedIndex) EditorGUI.DrawRect(rowRect, SelectedRowColor);
        }

        private void DrawRowLabel(TimelineLayout layout, int row, string text, string warning)
        {
            Rect labelRect = layout.LabelRect(row);
            labelRect.xMin += LabelIndent;
            GUI.Label(labelRect, text, EditorStyles.label);

            if (warning == null) return;

            warningIcon ??= EditorGUIUtility.IconContent(WarningIconName);
            float iconSize = labelRect.height;
            Rect iconRect = new Rect(labelRect.xMax - iconSize, labelRect.y, iconSize, iconSize);
            GUI.Label(iconRect, new GUIContent(warningIcon.image, warning));
        }

        private void DrawEventBar(TimelineLayout layout, int row, SerializedProperty element)
        {
            SerializedProperty start = element.FindPropertyRelative(ActionEditorUtility.StartFramePropertyName);
            SerializedProperty end = element.FindPropertyRelative(ActionEditorUtility.EndFramePropertyName);
            if (start == null || end == null) return;

            Rect rowRect = layout.RowRect(row);
            Rect bar = BarRect(layout, rowRect, start.intValue, end.intValue);
            Color color = ActionEventStyle.GetColor(element.managedReferenceValue?.GetType());

            EditorGUI.DrawRect(bar, color);
            if (row == SelectedIndex) DrawOutline(bar);

            EditorGUIUtility.AddCursorRect(GrabRect(bar.xMin, rowRect), MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(GrabRect(bar.xMax, rowRect), MouseCursor.ResizeHorizontal);

            SerializedProperty transition = element.FindPropertyRelative(ActionEditorUtility.TransitionFramePropertyName);
            if (transition == null) return;

            float x = layout.FrameToX(transition.intValue);
            EditorGUI.DrawRect(new Rect(x - MarkerHalfWidth, rowRect.y, MarkerHalfWidth * 2f, rowRect.height), TransitionColor);
            EditorGUIUtility.AddCursorRect(GrabRect(x, rowRect), MouseCursor.ResizeHorizontal);
        }

        private static void DrawOutline(Rect rect)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, OutlineWidth), OutlineColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - OutlineWidth, rect.width, OutlineWidth), OutlineColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, OutlineWidth, rect.height), OutlineColor);
            EditorGUI.DrawRect(new Rect(rect.xMax - OutlineWidth, rect.y, OutlineWidth, rect.height), OutlineColor);
        }

        private static void DrawVerticalLine(float x, float y, float height, Color color)
        {
            EditorGUI.DrawRect(new Rect(x - LineWidth * 0.5f, y, LineWidth, height), color);
        }

        #endregion

        #region Input

        private float HandleMouseDown(Event current, int controlId, TimelineLayout layout, SerializedObject actionObject,
            SerializedProperty events, SerializedProperty length, float currentFrame)
        {
            Vector2 mouse = current.mousePosition;
            if (!layout.Bounds.Contains(mouse)) return currentFrame;

            int frame = layout.XToFrame(mouse.x);

            if (layout.Ruler.Contains(mouse))
            {
                BeginDrag(current, controlId, DragMode.Scrub, frame);
                return frame;
            }

            int row = layout.GetRow(mouse.y);
            bool isEventRow = row >= 0 && row < events.arraySize;

            if (current.button == RightMouseButton)
            {
                ShowContextMenu(actionObject, isEventRow ? row : NoSelection, frame);
                current.Use();
                return currentFrame;
            }

            if (row == events.arraySize && IsNear(mouse.x, layout.FrameToX(length.intValue)))
            {
                BeginDrag(current, controlId, DragMode.Length, frame);
                return currentFrame;
            }

            SelectedIndex = isEventRow ? row : NoSelection;
            DragMode mode = isEventRow && mouse.x >= layout.Timeline.x ? PickEventDragMode(layout, events, row, mouse) : DragMode.None;

            if (mode != DragMode.None) BeginDrag(current, controlId, mode, frame);
            else current.Use();
            return currentFrame;
        }

        // 클릭 위치로 전이 마커 / 시작·끝 가장자리 / 막대 이동 판별, 드래그 기준값 저장
        private DragMode PickEventDragMode(TimelineLayout layout, SerializedProperty events, int row, Vector2 mouse)
        {
            SerializedProperty element = events.GetArrayElementAtIndex(row);
            SerializedProperty start = element.FindPropertyRelative(ActionEditorUtility.StartFramePropertyName);
            SerializedProperty end = element.FindPropertyRelative(ActionEditorUtility.EndFramePropertyName);
            SerializedProperty transition = element.FindPropertyRelative(ActionEditorUtility.TransitionFramePropertyName);
            if (start == null || end == null) return DragMode.None;

            dragStartValue = start.intValue;
            dragEndValue = end.intValue;
            dragTransitionValue = transition != null ? transition.intValue : 0;

            Rect bar = BarRect(layout, layout.RowRect(row), start.intValue, end.intValue);
            return transition != null && IsNear(mouse.x, layout.FrameToX(transition.intValue)) ? DragMode.Transition
                : IsNear(mouse.x, bar.xMin) ? DragMode.ResizeStart
                : IsNear(mouse.x, bar.xMax) ? DragMode.ResizeEnd
                : bar.Contains(mouse) ? DragMode.Move
                : DragMode.None;
        }

        private float HandleDrag(Event current, TimelineLayout layout, SerializedProperty events, SerializedProperty length, float currentFrame)
        {
            int frame = Mathf.Clamp(layout.XToFrame(current.mousePosition.x), 0, layout.TotalFrames);
            current.Use();

            if (dragMode == DragMode.Scrub) return frame;

            if (dragMode == DragMode.Length)
            {
                length.intValue = Mathf.Clamp(frame, MinFrameSpan, layout.TotalFrames);
                return currentFrame;
            }

            if (SelectedIndex < 0 || SelectedIndex >= events.arraySize) return currentFrame;

            SerializedProperty element = events.GetArrayElementAtIndex(SelectedIndex);
            SerializedProperty start = element.FindPropertyRelative(ActionEditorUtility.StartFramePropertyName);
            SerializedProperty end = element.FindPropertyRelative(ActionEditorUtility.EndFramePropertyName);
            SerializedProperty transition = element.FindPropertyRelative(ActionEditorUtility.TransitionFramePropertyName);

            switch (dragMode)
            {
                case DragMode.Move:
                    // 구간 길이 유지, 전이 프레임도 함께 이동
                    int span = dragEndValue - dragStartValue;
                    int newStart = Mathf.Clamp(dragStartValue + frame - dragOriginFrame, 0, Mathf.Max(0, layout.TotalFrames - span));
                    start.intValue = newStart;
                    end.intValue = newStart + span;
                    if (transition != null) transition.intValue = Mathf.Max(0, dragTransitionValue + newStart - dragStartValue);
                    break;
                case DragMode.ResizeStart:
                    start.intValue = Mathf.Clamp(frame, 0, end.intValue - MinFrameSpan);
                    break;
                case DragMode.ResizeEnd:
                    end.intValue = Mathf.Clamp(frame, start.intValue + MinFrameSpan, layout.TotalFrames);
                    break;
                case DragMode.Transition:
                    transition.intValue = frame;
                    break;
            }

            return currentFrame;
        }

        private void BeginDrag(Event current, int controlId, DragMode mode, int frame)
        {
            dragMode = mode;
            dragOriginFrame = frame;
            undoGroup = Undo.GetCurrentGroup();
            GUIUtility.hotControl = controlId;
            current.Use();
        }

        private void EndDrag(Event current)
        {
            dragMode = DragMode.None;
            GUIUtility.hotControl = 0;
            Undo.CollapseUndoOperations(undoGroup);
            current.Use();
        }

        private void ShowContextMenu(SerializedObject actionObject, int eventIndex, int frame)
        {
            GenericMenu menu = new GenericMenu();
            ActionEditorUtility.AppendAddItems(menu, "추가/", actionObject, Mathf.Max(0, frame), index =>
            {
                SelectedIndex = index;
                Changed?.Invoke();
            });

            if (eventIndex != NoSelection)
            {
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("삭제"), false, () =>
                {
                    ActionEditorUtility.RemoveEvent(actionObject, eventIndex);
                    SelectedIndex = NoSelection;
                    Changed?.Invoke();
                });
            }

            menu.ShowAsContext();
        }

        #endregion

        private static Rect BarRect(TimelineLayout layout, Rect rowRect, int startFrame, int endFrame)
        {
            float xMin = layout.FrameToX(startFrame);
            float xMax = Mathf.Max(layout.FrameToX(endFrame), xMin + LineWidth);
            return new Rect(xMin, rowRect.y + BarPadding, xMax - xMin, rowRect.height - BarPadding * 2f);
        }

        private static Rect GrabRect(float x, Rect rowRect) => new Rect(x - EdgeGrabWidth * 0.5f, rowRect.y, EdgeGrabWidth, rowRect.height);

        private static bool IsNear(float x, float targetX) => Mathf.Abs(x - targetX) <= EdgeGrabWidth * 0.5f;

        // 프레임 ↔ 픽셀 변환과 행 영역 계산
        private readonly struct TimelineLayout
        {
            public readonly Rect Bounds;
            public readonly Rect Ruler;
            public readonly Rect Timeline;
            public readonly int TotalFrames;
            public readonly float PixelsPerFrame;

            public float TracksHeight => Bounds.yMax - Ruler.yMax;

            public TimelineLayout(Rect bounds, int totalFrames)
            {
                Bounds = bounds;
                TotalFrames = totalFrames;
                Timeline = new Rect(bounds.x + LabelWidth, bounds.y, bounds.width - LabelWidth, bounds.height);
                Ruler = new Rect(Timeline.x, bounds.y, Timeline.width, RulerHeight);
                PixelsPerFrame = Timeline.width / totalFrames;
            }

            public float FrameToX(float frame) => Timeline.x + frame * PixelsPerFrame;
            public int XToFrame(float x) => Mathf.RoundToInt((x - Timeline.x) / PixelsPerFrame);
            public int GetRow(float y) => Mathf.FloorToInt((y - Ruler.yMax) / RowHeight);

            public Rect FullRowRect(int row) => new Rect(Bounds.x, Ruler.yMax + row * RowHeight, Bounds.width, RowHeight);
            public Rect RowRect(int row) => new Rect(Timeline.x, Ruler.yMax + row * RowHeight, Timeline.width, RowHeight);
            public Rect LabelRect(int row) => new Rect(Bounds.x, Ruler.yMax + row * RowHeight, LabelWidth, RowHeight);
        }
    }
}

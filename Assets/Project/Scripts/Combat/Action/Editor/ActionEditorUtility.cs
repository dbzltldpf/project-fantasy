using System;
using ProjectFantasy.Combat;
using UnityEditor;
using UnityEngine;

namespace ProjectFantasy.CombatEditor
{
    // ActionData 인스펙터·타임라인 창 공용 (직렬화 필드 이름, 이벤트 추가 메뉴, 필드 그리기)
    internal static class ActionEditorUtility
    {
        public const string EventsPropertyName = "events";
        public const string LengthFramesPropertyName = "lengthFrames";
        public const string StartFramePropertyName = "startFrame";
        public const string EndFramePropertyName = "endFrame";
        public const string TransitionFramePropertyName = "transitionFrame";

        private const string EmptyEventName = "(None)";
        private const int DefaultEventLength = 5;

        public static int GetClipFrameCount(ActionData action)
        {
            AnimationClip clip = action.Clip;
            return clip != null ? Mathf.RoundToInt(clip.length * clip.frameRate) : 0;
        }

        public static string GetEventName(SerializedProperty element)
        {
            object value = element.managedReferenceValue;
            return value != null ? value.GetType().Name : EmptyEventName;
        }

        // 이벤트 검증 경고 (문제 없으면 null)
        public static string GetWarning(ActionData action, int index)
        {
            if (index >= action.EventCount) return null;

            ActionEvent actionEvent = action.GetEvent(index);
            return actionEvent != null ? actionEvent.Validate(action.LengthFrames) : null;
        }

        // 관리 참조 요소의 하위 필드를 펼쳐 그림
        public static void DrawChildren(SerializedProperty element)
        {
            SerializedProperty child = element.Copy();
            SerializedProperty end = element.GetEndProperty();
            if (!child.NextVisible(true)) return;

            while (!SerializedProperty.EqualContents(child, end))
            {
                EditorGUILayout.PropertyField(child, true);
                if (!child.NextVisible(false)) break;
            }
        }

        // ActionEvent 파생 타입을 메뉴에 나열 (새 이벤트 클래스는 자동 노출)
        public static void AppendAddItems(GenericMenu menu, string pathPrefix, SerializedObject actionObject, int startFrame, Action<int> onAdded)
        {
            foreach (Type type in TypeCache.GetTypesDerivedFrom<ActionEvent>())
            {
                if (type.IsAbstract) continue;

                Type eventType = type;
                menu.AddItem(new GUIContent(pathPrefix + eventType.Name), false, () =>
                {
                    int index = AddEvent(actionObject, eventType, startFrame);
                    onAdded?.Invoke(index);
                });
            }
        }

        public static void RemoveEvent(SerializedObject actionObject, int index)
        {
            actionObject.Update();
            actionObject.FindProperty(EventsPropertyName).DeleteArrayElementAtIndex(index);
            actionObject.ApplyModifiedProperties();
        }

        private static int AddEvent(SerializedObject actionObject, Type eventType, int startFrame)
        {
            int undoGroup = Undo.GetCurrentGroup();

            actionObject.Update();
            SerializedProperty events = actionObject.FindProperty(EventsPropertyName);
            int index = events.arraySize;
            events.arraySize++;
            events.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(eventType);
            actionObject.ApplyModifiedProperties();

            // 인스턴스 할당 후 하위 필드가 생기므로 다시 갱신
            actionObject.Update();
            SerializedProperty element = actionObject.FindProperty(EventsPropertyName).GetArrayElementAtIndex(index);
            element.FindPropertyRelative(StartFramePropertyName).intValue = startFrame;
            element.FindPropertyRelative(EndFramePropertyName).intValue = startFrame + DefaultEventLength;
            actionObject.ApplyModifiedProperties();

            Undo.CollapseUndoOperations(undoGroup);
            return index;
        }
    }
}

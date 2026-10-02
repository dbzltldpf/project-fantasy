using ProjectFantasy.Combat;
using UnityEditor;
using UnityEngine;

namespace ProjectFantasy.CombatEditor
{
    // ActionData 인스펙터: 타임라인 창 열기, 다형 이벤트 목록 추가·삭제·검증 표시
    [CustomEditor(typeof(ActionData))]
    public sealed class ActionDataEditor : Editor
    {
        private const float RemoveButtonWidth = 50f;
        private const int NoIndex = -1;
        private const int DefaultStartFrame = 0;

        private SerializedProperty eventsProperty;

        private void OnEnable()
        {
            eventsProperty = serializedObject.FindProperty(ActionEditorUtility.EventsPropertyName);
        }

        public override void OnInspectorGUI()
        {
            ActionData action = (ActionData)target;
            if (GUILayout.Button("타임라인 열기")) ActionTimelineWindow.Open(action);

            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, ActionEditorUtility.EventsPropertyName);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

            int removeIndex = DrawEvents(action);
            if (removeIndex != NoIndex) eventsProperty.DeleteArrayElementAtIndex(removeIndex);

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("이벤트 추가"))
            {
                GenericMenu menu = new GenericMenu();
                ActionEditorUtility.AppendAddItems(menu, string.Empty, serializedObject, DefaultStartFrame, null);
                menu.ShowAsContext();
            }
        }

        // 삭제 요청된 인덱스 반환 (없으면 NoIndex)
        private int DrawEvents(ActionData action)
        {
            int removeIndex = NoIndex;

            for (int i = 0; i < eventsProperty.arraySize; i++)
            {
                SerializedProperty element = eventsProperty.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i}. {ActionEditorUtility.GetEventName(element)}", EditorStyles.boldLabel);
                if (GUILayout.Button("삭제", GUILayout.Width(RemoveButtonWidth))) removeIndex = i;
                EditorGUILayout.EndHorizontal();

                ActionEditorUtility.DrawChildren(element);

                string warning = ActionEditorUtility.GetWarning(action, i);
                if (warning != null) EditorGUILayout.HelpBox(warning, MessageType.Warning);

                EditorGUILayout.EndVertical();
            }

            return removeIndex;
        }
    }
}

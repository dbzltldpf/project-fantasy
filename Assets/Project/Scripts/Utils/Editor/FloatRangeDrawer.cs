using ProjectFantasy.Utils;
using UnityEditor;
using UnityEngine;

namespace ProjectFantasy.UtilsEditor
{
    // FloatRange를 "라벨 | Min [ ] Max [ ]" 한 줄로 표시
    [CustomPropertyDrawer(typeof(FloatRange))]
    public sealed class FloatRangeDrawer : PropertyDrawer
    {
        private const string MinField = "min";
        private const string MaxField = "max";
        private const float FieldLabelWidth = 30f;
        private const float Spacing = 4f;
        private const int FieldCount = 2;

        private static readonly GUIContent MinLabel = new GUIContent("Min");
        private static readonly GUIContent MaxLabel = new GUIContent("Max");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect content = EditorGUI.PrefixLabel(position, label);

            int previousIndent = EditorGUI.indentLevel;
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = FieldLabelWidth;

            float fieldWidth = (content.width - Spacing) / FieldCount;
            Rect minRect = new Rect(content.x, content.y, fieldWidth, content.height);
            Rect maxRect = new Rect(minRect.xMax + Spacing, content.y, fieldWidth, content.height);
            EditorGUI.PropertyField(minRect, property.FindPropertyRelative(MinField), MinLabel);
            EditorGUI.PropertyField(maxRect, property.FindPropertyRelative(MaxField), MaxLabel);

            EditorGUIUtility.labelWidth = previousLabelWidth;
            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }
    }
}

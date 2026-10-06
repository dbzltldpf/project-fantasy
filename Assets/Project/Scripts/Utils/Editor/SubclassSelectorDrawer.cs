using System;
using System.Collections.Generic;
using ProjectFantasy.Utils;
using UnityEditor;
using UnityEngine;

namespace ProjectFantasy.UtilsEditor
{
    // 관리 참조 필드: 상단 드롭다운으로 구현 타입 선택, 아래에 하위 필드 표시
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public sealed class SubclassSelectorDrawer : PropertyDrawer
    {
        private const string NoneLabel = "(None)";
        private const char TypeNameSeparator = ' ';
        private const char NamespaceSeparator = '.';
        private const int AssemblyAndTypeParts = 2;

        private static readonly Dictionary<Type, List<Type>> ConcreteTypeCache = new Dictionary<Type, List<Type>>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect buttonRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            buttonRect.xMin += EditorGUIUtility.labelWidth;

            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(GetCurrentTypeName(property)), FocusType.Keyboard))
            {
                ShowTypeMenu(property);
            }

            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }

        private static string GetCurrentTypeName(SerializedProperty property)
        {
            string fullTypeName = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(fullTypeName)) return NoneLabel;

            // "어셈블리 네임스페이스.타입" 형식에서 타입 이름만
            string typeName = fullTypeName.Substring(fullTypeName.IndexOf(TypeNameSeparator) + 1);
            return typeName.Substring(typeName.LastIndexOf(NamespaceSeparator) + 1);
        }

        private static void ShowTypeMenu(SerializedProperty property)
        {
            Type baseType = ResolveFieldType(property);
            GenericMenu menu = new GenericMenu();
            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;

            menu.AddItem(new GUIContent(NoneLabel), false, () => Assign(serializedObject, propertyPath, null));
            if (baseType != null)
            {
                foreach (Type type in GetConcreteTypes(baseType))
                {
                    Type selected = type;
                    menu.AddItem(new GUIContent(selected.Name), false, () => Assign(serializedObject, propertyPath, Activator.CreateInstance(selected)));
                }
            }
            menu.ShowAsContext();
        }

        private static void Assign(SerializedObject serializedObject, string propertyPath, object value)
        {
            serializedObject.Update();
            serializedObject.FindProperty(propertyPath).managedReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        // "어셈블리 네임스페이스.타입" 문자열에서 필드 선언 타입 해석
        private static Type ResolveFieldType(SerializedProperty property)
        {
            string[] parts = property.managedReferenceFieldTypename.Split(TypeNameSeparator);
            return parts.Length == AssemblyAndTypeParts ? Type.GetType($"{parts[1]}, {parts[0]}") : null;
        }

        private static List<Type> GetConcreteTypes(Type baseType)
        {
            if (ConcreteTypeCache.TryGetValue(baseType, out List<Type> types)) return types;

            types = new List<Type>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom(baseType))
            {
                if (!type.IsAbstract && !type.IsGenericType && !typeof(UnityEngine.Object).IsAssignableFrom(type)) types.Add(type);
            }
            ConcreteTypeCache.Add(baseType, types);
            return types;
        }
    }
}

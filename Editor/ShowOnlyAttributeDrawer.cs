using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VeryFS.Framework.Runtime.Utilities;

namespace VeryFS.Framework.Editor
{
    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
    public class ShowOnlyAttributeDrawer: PropertyDrawer
    {
        public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, includeChildren: true);
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return base.CreatePropertyGUI(property);
        }

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            
            //EditorGUI.BeginProperty(rect, label, prop);

            using (new EditorGUI.DisabledScope(disabled: true))
            {
                EditorGUI.PropertyField(rect, prop, label,true);
            }

            //EditorGUI.EndProperty();
            // string valueStr;
            //
            // switch (prop.propertyType)
            // {
            //     case SerializedPropertyType.Integer:
            //         valueStr = prop.intValue.ToString();
            //         break;
            //     case SerializedPropertyType.Boolean:
            //         valueStr = prop.boolValue.ToString();
            //         break;
            //     case SerializedPropertyType.Float:
            //         valueStr = prop.floatValue.ToString("0.00000");
            //         break;
            //     case SerializedPropertyType.String:
            //         valueStr = prop.stringValue;
            //         break;
            //     case SerializedPropertyType.Enum:
            //     {
            //         int idx = Mathf.Clamp(prop.enumValueIndex, 0, prop.enumDisplayNames.Length);
            //         valueStr = prop.enumDisplayNames[idx];
            //         break;
            //     }
            //     default:
            //         //valueStr = "(not supported) " + prop.propertyType;
            //         EditorGUI.PropertyField(position, prop, label,true);
            //         return;
            // }
            //
            // EditorGUI.LabelField(position,label.text, valueStr);
        }
    }
}
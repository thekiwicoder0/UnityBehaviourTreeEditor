using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace TheKiwiCoder
{
    public class EditorUtility : MonoBehaviour
    {
        public static void CreatePropertyInspector(VisualElement root, SerializedProperty property) {

            // Auto-expand the property
            property.isExpanded = true;

            // Property field
            PropertyField field = new PropertyField();
            field.BindProperty(property);
            root.Add(field);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace TheKiwiCoder {
    public class BlackboardView : VisualElement {

        [SerializeField]
        VisualTreeAsset m_ItemAsset;

        public new class UxmlFactory : UxmlFactory<BlackboardView, VisualElement.UxmlTraits> { }

        public BlackboardView() {

        }

        internal void Bind(SerializedProperty blackboard) {
            Clear();

            EditorUtility.CreatePropertyInspector(this, blackboard);
        }
    }
}
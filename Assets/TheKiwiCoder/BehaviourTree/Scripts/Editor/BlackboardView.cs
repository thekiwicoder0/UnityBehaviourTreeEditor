using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace TheKiwiCoder {
    public class BlackboardView : VisualElement {
        public new class UxmlFactory : UxmlFactory<BlackboardView, VisualElement.UxmlTraits> { }

        public BlackboardView() {

        }

        internal void Bind(BehaviourTree tree) {
            Clear();

            var blackboardProperty = new SerializedBehaviourTree(tree).Blackboard;
            EditorUtility.CreatePropertyInspector(this, blackboardProperty);
        }
    }
}
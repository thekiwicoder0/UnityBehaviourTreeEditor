using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace TheKiwiCoder {
    public class InspectorView : VisualElement {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

        public InspectorView() {

        }

        internal void UpdateSelection(BehaviourTree tree, NodeView nodeView) {
            Clear();

            if (nodeView == null) {
                return;
            }

            SerializedBehaviourTree serializedBehaviourTree = new SerializedBehaviourTree(tree);
            var nodeProperty = serializedBehaviourTree.FindNode(serializedBehaviourTree.Nodes, nodeView.node);
            if (nodeProperty == null) {
                return;
            }

            // Auto-expand the property
            nodeProperty.isExpanded = true;

            // Property field
            PropertyField field = new PropertyField();
            field.label = nodeProperty.managedReferenceValue.GetType().ToString();
            field.BindProperty(nodeProperty);
            Add(field);
            
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace TheKiwiCoder {

    [UxmlElement]
    public partial class InspectorView : VisualElement {

        public InspectorView() {

        }

        internal void UpdateSelection(SerializedBehaviourTree serializer, NodeView nodeView) {
            Clear();

            if (nodeView == null) {
                return;
            }

            var nodeProperty = serializer.FindNode(serializer.Nodes, nodeView.node);
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
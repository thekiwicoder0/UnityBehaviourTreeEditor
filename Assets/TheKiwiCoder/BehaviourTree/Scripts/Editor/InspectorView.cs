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
            var nodeProp = serializedBehaviourTree.FindNode(serializedBehaviourTree.Nodes, nodeView.node);
            if (nodeProp == null) {
                return;
            }

            //var root = new InspectorElement(nodeView.node);

            

            var root = new VisualElement();
            root.style.flexGrow = 1.0f;

            //PropertyField propField = new PropertyField(nodeProp);
            //propField.style.flexGrow = 1.0f;
            //propField.BindProperty(nodeProp);
            //root.Add(propField);

            var iter = nodeProp.GetEnumerator();

            while (iter.MoveNext()) {
               
                var property = iter.Current as SerializedProperty;
                var propertyField = new PropertyField();
                propertyField.BindProperty(iter.Current as SerializedProperty);

                root.Add(propertyField);
                    //if (prop.name == "m_Script") {
                    //field.SetEnabled(false);
                    //}

                
            }

            Add(root);
        }
    }
}
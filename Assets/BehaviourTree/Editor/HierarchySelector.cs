using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace TheKiwiCoder {
    public class HierarchySelector : MouseManipulator {

        protected override void RegisterCallbacksOnTarget() {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget() {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt) {
            if (!CanStopManipulation(evt))
                return; 
            
            var graphView = target as BehaviourTreeView;
            if (graphView == null)
                return;

            NodeView clickedElement = evt.target as NodeView;
            if (clickedElement == null) {
                var ve = evt.target as VisualElement;
                clickedElement = ve.GetFirstAncestorOfType<NodeView>();
                if (clickedElement == null)
                    return;
            }

            if (evt.ctrlKey) {
                SelectChildren(evt, graphView, clickedElement);
            }
        }

        void SelectChildren(MouseDownEvent evt, BehaviourTreeView graphView, NodeView clickedElement) {
            // Add children to selection so the root element can be moved
            BehaviourTree.Traverse(clickedElement.node, node => {
                var view = graphView.FindNodeView(node);
                graphView.AddToSelection(view);
            });
        }
    }
}
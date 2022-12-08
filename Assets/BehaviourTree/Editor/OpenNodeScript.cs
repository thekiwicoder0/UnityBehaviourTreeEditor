using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace TheKiwiCoder {
    public class  OpenNodeScript : MouseManipulator {

        double time;
        double doubleClickDuration = 0.3;

        public OpenNodeScript() {
            time = EditorApplication.timeSinceStartup;
        }

        protected override void RegisterCallbacksOnTarget() {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget() {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt) {
            if (!CanStopManipulation(evt))
                return; 

            NodeView clickedElement = evt.target as NodeView;
            if (clickedElement == null) {
                var ve = evt.target as VisualElement;
                clickedElement = ve.GetFirstAncestorOfType<NodeView>();
                if (clickedElement == null)
                    return;
            }

            double duration = EditorApplication.timeSinceStartup - time;
            if (duration < doubleClickDuration) {
                OpenScriptForNode(evt, clickedElement);
            }

            time = EditorApplication.timeSinceStartup;
        }

        void OpenScriptForNode(MouseDownEvent evt, NodeView clickedElement) {
            // Open script in the editor:
            var nodeName = clickedElement.node.GetType().Name;
            var assetGuids = AssetDatabase.FindAssets($"t:TextAsset {nodeName}");
            for (int i = 0; i < assetGuids.Length; ++i) {
                var path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                var filename = System.IO.Path.GetFileName(path);
                if (filename == $"{nodeName}.cs") {
                    var script = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    AssetDatabase.OpenAsset(script);
                    break;
                }
            }

            // Remove the node from selection to prevent dragging it around when returning to the editor.
            BehaviourTreeEditorWindow.Instance.treeView.RemoveFromSelection(clickedElement);
        }
    }
}
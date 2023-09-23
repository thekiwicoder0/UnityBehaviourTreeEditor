using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTreeBuilder
{
    public class DoubleClickNode : MouseManipulator
    {
        private readonly double doubleClickDuration = 0.3;

        private double time;

        public DoubleClickNode()
        {
            time = EditorApplication.timeSinceStartup;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!CanStopManipulation(evt))
                return;

            var clickedElement = evt.target as NodeView;
            if (clickedElement == null)
            {
                var ve = evt.target as VisualElement;
                clickedElement = ve.GetFirstAncestorOfType<NodeView>();
                if (clickedElement == null)
                    return;
            }

            var duration = EditorApplication.timeSinceStartup - time;
            if (duration < doubleClickDuration) OnDoubleClick(evt, clickedElement);

            time = EditorApplication.timeSinceStartup;
        }

        private void OpenScriptForNode(MouseDownEvent evt, NodeView clickedElement)
        {
            // Open script in the editor:
            var nodeName = clickedElement.node.GetType().Name;
            var assetGuids = AssetDatabase.FindAssets($"t:TextAsset {nodeName}");
            for (var i = 0; i < assetGuids.Length; ++i)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                var filename = Path.GetFileName(path);
                if (filename == $"{nodeName}.cs")
                {
                    var script = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    AssetDatabase.OpenAsset(script);
                    break;
                }
            }

            // Remove the node from selection to prevent dragging it around when returning to the editor.
            BehaviourTreeEditorWindow.Instance.treeView.RemoveFromSelection(clickedElement);
        }

        private void OpenSubtree(NodeView clickedElement)
        {
            BehaviourTreeEditorWindow.Instance.PushSubTreeView(clickedElement.node as SubTree);
        }

        private void OnDoubleClick(MouseDownEvent evt, NodeView clickedElement)
        {
            if (clickedElement.node is SubTree)
                OpenSubtree(clickedElement);
            else
                OpenScriptForNode(evt, clickedElement);
        }
    }
}
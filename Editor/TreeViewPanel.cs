using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TheKiwiCoder {

    public class TreeViewPanel : VisualElement {
        BehaviourTreeEditorWindow window;
        SerializedBehaviourTree serializer;
        ScrollView scroll;
        Dictionary<string, VisualElement> nodeRows = new Dictionary<string, VisualElement>();
        Dictionary<string, VisualElement> childContainers = new Dictionary<string, VisualElement>();
        Dictionary<string, Label> arrowLabels = new Dictionary<string, Label>();
        string selectedGuid;

        public TreeViewPanel(BehaviourTreeEditorWindow window) {
            this.window = window;
            AddToClassList("tree-view-panel");

            scroll = new ScrollView();
            scroll.AddToClassList("tree-view-scroll");
            Add(scroll);
        }

        public void Bind(SerializedBehaviourTree serializer) {
            this.serializer = serializer;
            nodeRows.Clear();
            childContainers.Clear();
            arrowLabels.Clear();
            scroll.Clear();
            if (serializer == null || serializer.tree == null) return;

            int rowIndex = 0;
            CreateNodeRecursive(serializer.tree.rootNode, scroll.contentContainer, 0, ref rowIndex);

            if (selectedGuid != null && nodeRows.TryGetValue(selectedGuid, out var row)) {
                row.AddToClassList("tree-view-selected");
            }
        }

        public void SelectNode(string guid) {
            if (selectedGuid != null && nodeRows.TryGetValue(selectedGuid, out var prev)) {
                prev.RemoveFromClassList("tree-view-selected");
            }
            selectedGuid = guid;
            if (guid != null && nodeRows.TryGetValue(guid, out var row)) {
                ExpandToNode(guid);
                row.AddToClassList("tree-view-selected");
            }
        }

        void ExpandToNode(string guid) {
            if (serializer?.tree == null) return;
            var path = new List<string>();
            if (!FindPath(serializer.tree.rootNode, guid, path)) return;
            // Expand every ancestor (all guids except the target itself)
            foreach (var ancestorGuid in path) {
                SetExpandedByGuid(ancestorGuid, true);
            }
        }

        bool FindPath(Node node, string targetGuid, List<string> path) {
            if (node == null) return false;
            if (node.guid == targetGuid) return true;
            path.Add(node.guid);
            foreach (var child in BehaviourTree.GetChildren(node)) {
                if (FindPath(child, targetGuid, path)) return true;
            }
            path.RemoveAt(path.Count - 1);
            return false;
        }

        void SetExpandedByGuid(string guid, bool expanded) {
            // Find node to update session state
            var node = FindNode(serializer.tree.rootNode, guid);
            if (node != null) SetExpanded(node, expanded);
            // Update arrow and container
            if (arrowLabels.TryGetValue(guid, out var arrow))
                arrow.text = expanded ? "▼" : "▶";
            if (childContainers.TryGetValue(guid, out var container))
                container.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        Node FindNode(Node node, string guid) {
            if (node == null) return null;
            if (node.guid == guid) return node;
            foreach (var child in BehaviourTree.GetChildren(node)) {
                var result = FindNode(child, guid);
                if (result != null) return result;
            }
            return null;
        }

        void CreateNodeRecursive(Node node, VisualElement parent, int depth, ref int rowIndex) {
            if (node == null) return;

            var children = BehaviourTree.GetChildren(node);
            bool hasChildren = children.Count > 0;
            bool expanded = IsExpanded(node);
            int currentRow = rowIndex++;

            var row = new VisualElement();
            row.AddToClassList("tree-view-row");
            row.AddToClassList(currentRow % 2 == 0 ? "tree-view-row-even" : "tree-view-row-odd");
            row.style.paddingLeft = 4 + depth * 14;

            var arrow = new Label(hasChildren ? (expanded ? "▼" : "▶") : "  ");
            arrow.AddToClassList("tree-view-arrow");
            row.Add(arrow);
            if (hasChildren) arrowLabels[node.guid] = arrow;

            var label = new Label(node.GetType().Name);
            label.AddToClassList("tree-view-label");

            var icon = GetNodeIcon(node);
            if (icon != null) {
                var iconEl = new VisualElement();
                iconEl.AddToClassList("tree-view-icon");
                iconEl.style.backgroundImage = new StyleBackground(icon);
                row.Add(iconEl);
            }

            row.Add(label);

            parent.Add(row);
            nodeRows[node.guid] = row;

            VisualElement childContainer = null;
            if (hasChildren) {
                childContainer = new VisualElement();
                childContainer.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                childContainers[node.guid] = childContainer;
                parent.Add(childContainer);

                arrow.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    bool nowExpanded = !IsExpanded(node);
                    SetExpanded(node, nowExpanded);
                    arrow.text = nowExpanded ? "▼" : "▶";
                    childContainer.style.display = nowExpanded ? DisplayStyle.Flex : DisplayStyle.None;
                });
            }

            row.RegisterCallback<ClickEvent>(evt => OnNodeClicked(node));

            if (childContainer != null) {
                foreach (var child in children) {
                    CreateNodeRecursive(child, childContainer, depth + 1, ref rowIndex);
                }
            }
        }

        void OnNodeClicked(Node node) {
            if (window == null || serializer == null) return;
            SelectNode(node.guid);
            var tv = window.CurrentTreeView;
            if (tv != null) {
                var nodeView = tv.FindNodeView(node.guid);
                if (nodeView != null) {
                    tv.SelectNode(nodeView);
                    window.InspectNode(serializer, nodeView);
                }
            }
        }

        Texture2D GetNodeIcon(Node node) {
            if (node is RootNode)      return Resources.Load<Texture2D>("node_root");
            if (node is Sequencer)     return Resources.Load<Texture2D>("node_sequencer");
            if (node is Selector)      return Resources.Load<Texture2D>("node_selector");
            if (node is Parallel)      return Resources.Load<Texture2D>("node_parallel");
            if (node is SubTree)       return Resources.Load<Texture2D>("node_subtree");
            if (node is ActionNode)    return Resources.Load<Texture2D>("node_action");
            return null;
        }

        bool IsExpanded(Node node) {
            if (window == null || window.expandedNodeGuids == null) return false;
            if (!window.expandedNodeGuids.TryGetValue(serializer.tree, out var set)) return false;
            return set.Contains(node.guid);
        }

        void SetExpanded(Node node, bool value) {
            if (window == null) return;
            if (!window.expandedNodeGuids.TryGetValue(serializer.tree, out var set)) {
                set = new HashSet<string>();
                window.expandedNodeGuids[serializer.tree] = set;
            }
            if (value) set.Add(node.guid);
            else set.Remove(node.guid);
        }
    }
}

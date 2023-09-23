using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTreeBuilder
{
    public class BehaviourTreeView : GraphView
    {
        // Node positions snap to 15 pixels
        public static int gridSnapSize = 15;

        private bool dontUpdateModel;

        public Action<NodeView> OnNodeSelected;

        private SerializedBehaviourTree serializer;

        public BehaviourTreeView()
        {
            Insert(0, new GridBackground());


            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new HierarchySelector());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Perform Copy
            serializeGraphElements = items =>
            {
                var copyPasteData = new CopyPasteData();
                copyPasteData.AddGraphElements(items);
                var data = JsonUtility.ToJson(copyPasteData);
                return data;
            };

            // Perform Paste
            unserializeAndPaste = (operationName, data) =>
            {
                serializer.BeginBatch();

                ClearSelection();

                var copyPasteData = JsonUtility.FromJson<CopyPasteData>(data);
                var oldToNewMapping = new Dictionary<string, string>();

                // Gather all nodes to copy
                var nodesToCopy = new List<NodeView>();
                foreach (var nodeGuid in copyPasteData.nodeGuids)
                {
                    var nodeView = FindNodeView(nodeGuid);
                    nodesToCopy.Add(nodeView);
                }

                // Gather all edges to create
                var edgesToCreate = new List<EdgeToCreate>();
                foreach (var nodeGuid in copyPasteData.nodeGuids)
                {
                    var nodeView = FindNodeView(nodeGuid);
                    var nodesParent = nodeView.NodeParent;
                    if (nodesToCopy.Contains(nodesParent))
                    {
                        var newEdge = new EdgeToCreate();
                        newEdge.parent = nodesParent;
                        newEdge.child = nodeView;
                        edgesToCreate.Add(newEdge);
                    }
                }

                // Copy all nodes
                foreach (var nodeView in nodesToCopy)
                {
                    var newNode = serializer.CreateNode(nodeView.node.GetType(),
                        nodeView.node.position + Vector2.one * 50);
                    var newNodeView = CreateNodeView(newNode);
                    AddToSelection(newNodeView);

                    // Map old to new guids so edges can be cloned.
                    oldToNewMapping[nodeView.node.guid] = newNode.guid;
                }

                // Copy all edges
                foreach (var edge in edgesToCreate)
                {
                    var oldParent = edge.parent;
                    var oldChild = edge.child;

                    // These should already have been created.
                    var newParent = FindNodeView(oldToNewMapping[oldParent.node.guid]);
                    var newChild = FindNodeView(oldToNewMapping[oldChild.node.guid]);

                    serializer.AddChild(newParent.node, newChild.node);
                    AddChild(newParent, newChild);
                }

                // Save changes
                serializer.EndBatch();
            };

            // Enable copy paste always?
            canPasteSerializedData = data => { return true; };

            viewTransformChanged += OnViewTransformChanged;
        }

        protected override bool canCopySelection => true;

        protected override bool canCutSelection => false; // Cut not supported right now

        protected override bool canPaste => true;

        protected override bool canDuplicateSelection => true;

        protected override bool canDeleteSelection => true;

        private void OnViewTransformChanged(GraphView graphView)
        {
            var position = contentViewContainer.transform.position;
            var scale = contentViewContainer.transform.scale;
            serializer.SetViewTransform(position, scale);
        }

        public NodeView FindNodeView(Node node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        public NodeView FindNodeView(string guid)
        {
            return GetNodeByGuid(guid) as NodeView;
        }

        public void ClearView()
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
        }

        public void PopulateView(SerializedBehaviourTree tree)
        {
            serializer = tree;

            ClearView();

            Debug.Assert(serializer.tree.rootNode != null);

            // Creates node view
            serializer.tree.nodes.ForEach(n => CreateNodeView(n));

            // Create edges
            serializer.tree.nodes.ForEach(n =>
            {
                var children = BehaviourTree.GetChildren(n);
                children.ForEach(c =>
                {
                    var parentView = FindNodeView(n);
                    var childView = FindNodeView(c);
                    Debug.Assert(parentView != null, "Invalid parent after deserialising");
                    Debug.Assert(childView != null,
                        $"Null child view after deserialising parent{parentView.node.GetType().Name}");
                    CreateEdgeView(parentView, childView);
                });
            });

            // Set view
            contentViewContainer.transform.position = serializer.tree.viewPosition;
            contentViewContainer.transform.scale = serializer.tree.viewScale;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (dontUpdateModel) return graphViewChange;

            var blockedDeletes = new List<GraphElement>();

            if (graphViewChange.elementsToRemove != null)
                graphViewChange.elementsToRemove.ForEach(elem =>
                {
                    var nodeView = elem as NodeView;
                    if (nodeView != null)
                    {
                        // The root node is not deletable
                        if (nodeView.node is not RootNode)
                        {
                            OnNodeSelected(null);
                            serializer.DeleteNode(nodeView.node);
                        }
                        else
                        {
                            blockedDeletes.Add(elem);
                        }
                    }

                    var edge = elem as Edge;
                    if (edge != null)
                    {
                        var parentView = edge.output.node as NodeView;
                        var childView = edge.input.node as NodeView;
                        serializer.RemoveChild(parentView.node, childView.node);
                    }
                });

            if (graphViewChange.edgesToCreate != null)
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    var parentView = edge.output.node as NodeView;
                    var childView = edge.input.node as NodeView;
                    serializer.AddChild(parentView.node, childView.node);
                });

            nodes.ForEach(n =>
            {
                var view = n as NodeView;
                // Need to rebind description labels as the serialized properties will be invalidated after removing from array
                view.SortChildren();
            });

            foreach (var elem in blockedDeletes) graphViewChange.elementsToRemove.Remove(elem);

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //base.BuildContextualMenu(evt); // Disable default cut/copy/paste context menu options.. who uses those anyway?

            CreateNodeWindow.Show(evt.mousePosition, null);
        }

        public NodeView CreateNode(Type type, Vector2 position, NodeView parentView)
        {
            serializer.BeginBatch();

            // Update model
            var node = serializer.CreateNode(type, position);
            if (parentView != null) serializer.AddChild(parentView.node, node);

            // Update View
            var nodeView = CreateNodeView(node);
            if (parentView != null) AddChild(parentView, nodeView);

            serializer.EndBatch();

            return nodeView;
        }

        public NodeView CreateNodeWithChild(Type type, Vector2 position, NodeView childView)
        {
            serializer.BeginBatch();

            // Update Model
            var node = serializer.CreateNode(type, position);

            // Delete the childs previous parent
            foreach (var connection in childView.input.connections)
            {
                var childParent = connection.output.node as NodeView;
                serializer.RemoveChild(childParent.node, childView.node);
            }

            // Add as child of new node.
            serializer.AddChild(node, childView.node);

            // Update View
            var nodeView = CreateNodeView(node);
            if (nodeView != null) AddChild(nodeView, childView);

            serializer.EndBatch();
            return nodeView;
        }

        private NodeView CreateNodeView(Node node)
        {
            var nodeView = new NodeView(node, BehaviourTreeEditorWindow.Instance.nodeXml);
            AddElement(nodeView);
            nodeView.OnNodeSelected = OnNodeSelected;
            return nodeView;
        }

        public void AddChild(NodeView parentView, NodeView childView)
        {
            // Delete Previous output connections
            if (parentView.output.capacity == Port.Capacity.Single) RemoveElements(parentView.output.connections);

            // Delete previous child's parent
            RemoveElements(childView.input.connections);

            CreateEdgeView(parentView, childView);
        }

        private void CreateEdgeView(NodeView parentView, NodeView childView)
        {
            var edge = parentView.output.ConnectTo<FlowingEdge>(childView.input);
            AddElement(edge);
        }

        public void RemoveElements(IEnumerable<GraphElement> elementsToRemove)
        {
            dontUpdateModel = true;
            DeleteElements(
                elementsToRemove); // Just need to delete the ui elements without causing a graphChangedEvent here.
            dontUpdateModel = false;
        }

        public void UpdateNodeStates()
        {
            nodes.ForEach(n =>
            {
                var view = n as NodeView;
                view.UpdateDescription();
                if (Application.isPlaying) view.UpdateState();
            });
        }

        public void SelectNode(NodeView nodeView)
        {
            ClearSelection();
            if (nodeView != null) AddToSelection(nodeView);
        }

        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits>
        {
        }

        [Serializable]
        private class CopyPasteData
        {
            public List<string> nodeGuids = new();

            public void AddGraphElements(IEnumerable<GraphElement> elementsToCopy)
            {
                foreach (var element in elementsToCopy)
                {
                    var nodeView = element as NodeView;
                    if (nodeView != null && nodeView.node is not RootNode) nodeGuids.Add(nodeView.node.guid);
                }
            }
        }

        private class EdgeToCreate
        {
            public NodeView child;
            public NodeView parent;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

namespace TheKiwiCoder {

    [UxmlElement]
    public partial class BehaviourTreeView : GraphView {
        public Action<NodeView> OnNodeSelected;

        protected override bool canCopySelection => true;

        protected override bool canCutSelection => false; // Cut not supported right now

        protected override bool canPaste => true;

        protected override bool canDuplicateSelection => true;

        protected override bool canDeleteSelection => true;

        public SerializedBehaviourTree serializer;

        bool dontUpdateModel = false;

        [Serializable]
        class CopyPasteData {

            public List<string> nodeGuids = new List<string>();

            public void AddGraphElements(IEnumerable<GraphElement> elementsToCopy) {
                foreach (var element in elementsToCopy) {
                    NodeView nodeView = element as NodeView;
                    if (nodeView != null && nodeView.node is not RootNode) {
                        nodeGuids.Add(nodeView.node.guid);
                    }
                }
            }
        }

        class EdgeToCreate {
            public NodeView parent;
            public NodeView child;
        };

        public BehaviourTreeView() {

            Insert(0, new GridBackground());


            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new HierarchySelector());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Perform Copy
            serializeGraphElements = (items) => {
                CopyPasteData copyPasteData = new CopyPasteData();
                copyPasteData.AddGraphElements(items);
                string data = JsonUtility.ToJson(copyPasteData);
                return data;
            };

            // Perform Paste
            unserializeAndPaste = (operationName, data) => {

                serializer.BeginBatch();

                ClearSelection();

                CopyPasteData copyPasteData = JsonUtility.FromJson<CopyPasteData>(data);
                Dictionary<string, string> oldToNewMapping = new Dictionary<string, string>();

                // Gather all nodes to copy
                List<NodeView> nodesToCopy = new List<NodeView>();
                foreach (var nodeGuid in copyPasteData.nodeGuids) {
                    NodeView nodeView = FindNodeView(nodeGuid);
                    nodesToCopy.Add(nodeView);
                }

                // Gather all edges to create
                List<EdgeToCreate> edgesToCreate = new List<EdgeToCreate>();
                foreach (var nodeGuid in copyPasteData.nodeGuids) {
                    NodeView nodeView = FindNodeView(nodeGuid);
                    if (nodeView != null) {
                        var nodesParent = nodeView.NodeParent;
                        if (nodesToCopy.Contains(nodesParent)) {
                            EdgeToCreate newEdge = new EdgeToCreate();
                            newEdge.parent = nodesParent;
                            newEdge.child = nodeView;
                            edgesToCreate.Add(newEdge);
                        }
                    }
                }

                // Copy all nodes
                foreach (var nodeView in nodesToCopy) {
                    Node newNode = serializer.CloneNode(nodeView.node, nodeView.node.position + Vector2.one * 50);
                    NodeView newNodeView = CreateNodeView(newNode);
                    AddToSelection(newNodeView);

                    // Map old to new guids so edges can be cloned.
                    oldToNewMapping[nodeView.node.guid] = newNode.guid;
                }

                // Copy all edges
                foreach (var edge in edgesToCreate) {
                    NodeView oldParent = edge.parent;
                    NodeView oldChild = edge.child;

                    // These should already have been created.
                    NodeView newParent = FindNodeView(oldToNewMapping[oldParent.node.guid]);
                    NodeView newChild = FindNodeView(oldToNewMapping[oldChild.node.guid]);

                    serializer.AddChild(newParent.node, newChild.node);
                    AddChild(newParent, newChild);
                }

                // Save changes
                serializer.EndBatch();
            };

            // Enable copy paste always?
            canPasteSerializedData = (data) => {
                return true;
            };

            viewTransformChanged += OnViewTransformChanged;
        }

        void OnViewTransformChanged(GraphView graphView) {
            Vector3 position = contentViewContainer.transform.position;
            Vector3 scale = contentViewContainer.transform.scale;
            serializer.SetViewTransform(position, scale);
        }

        public NodeView FindNodeView(Node node) {
            if (node == null) {
                return null;
            }
            return GetNodeByGuid(node.guid) as NodeView;
        }

        public NodeView FindNodeView(string guid) {
            return GetNodeByGuid(guid) as NodeView;
        }

        public void ClearView() {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
        }

        public void PopulateView(SerializedBehaviourTree tree) {
            serializer = tree;

            ClearView();

            Debug.Assert(serializer.tree.rootNode != null);

            // Creates node view
            serializer.tree.nodes.ForEach(n => CreateNodeView(n));

            // Create edges
            serializer.tree.nodes.ForEach(n => {
                var children = BehaviourTree.GetChildren(n);
                children.ForEach(c => {
                    NodeView parentView = FindNodeView(n);
                    NodeView childView = FindNodeView(c);
                    Debug.Assert(parentView != null, "Invalid parent after deserialising");
                    Debug.Assert(childView != null, $"Null child view after deserialising parent '{parentView.node.GetType().Name}'");
                    CreateEdgeView(parentView, childView);

                });
            });

            // Set view
            contentViewContainer.transform.position = serializer.tree.viewPosition;
            contentViewContainer.transform.scale = serializer.tree.viewScale;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {

            if (dontUpdateModel) {
                return graphViewChange;
            }

            List<GraphElement> blockedDeletes = new List<GraphElement>();

            if (graphViewChange.elementsToRemove != null) {
                graphViewChange.elementsToRemove.ForEach(elem => {
                    NodeView nodeView = elem as NodeView;
                    if (nodeView != null) {

                        // The root node is not deletable
                        if (nodeView.node is not RootNode) {
                            OnNodeSelected(null);
                            serializer.DeleteNode(nodeView.node);
                        } else {
                            blockedDeletes.Add(elem);
                        }
                    }

                    Edge edge = elem as Edge;
                    if (edge != null) {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;
                        serializer.RemoveChild(parentView.node, childView.node);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null) {
                graphViewChange.edgesToCreate.ForEach(edge => {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    serializer.AddChild(parentView.node, childView.node);
                });
            }

            nodes.ForEach((n) => {
                NodeView view = n as NodeView;
                // Need to rebind description labels as the serialized properties will be invalidated after removing from array
                view.SetupDataBinding();
                view.SortChildren();
            });

            foreach (var elem in blockedDeletes) {
                graphViewChange.elementsToRemove.Remove(elem);
            }

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            //base.BuildContextualMenu(evt); // Disable default cut/copy/paste context menu options.. who uses those anyway?

            CreateNodeWindow.Show(evt.mousePosition, null);
        }

        public NodeView CreateNode(System.Type type, Vector2 position, NodeView parentView) {

            serializer.BeginBatch();

            // Update model
            Node node = serializer.CreateNode(type, position);
            if (parentView != null) {
                serializer.AddChild(parentView.node, node);
            }

            // Update View
            NodeView nodeView = CreateNodeView(node);
            if (parentView != null) {
                AddChild(parentView, nodeView);
            }

            serializer.EndBatch();

            return nodeView;
        }

        public NodeView CreateNodeWithChild(System.Type type, Vector2 position, NodeView childView) {
            serializer.BeginBatch();

            // Update Model
            Node node = serializer.CreateNode(type, position);

            // Delete the childs previous parent
            foreach (var connection in childView.input.connections) {
                var childParent = connection.output.node as NodeView;
                serializer.RemoveChild(childParent.node, childView.node);
            }
            // Add as child of new node.
            serializer.AddChild(node, childView.node);

            // Update View
            NodeView nodeView = CreateNodeView(node);
            if (nodeView != null) {
                AddChild(nodeView, childView);
            }

            serializer.EndBatch();
            return nodeView;
        }

        public NodeView CreateNodeView(Node node) {
            NodeView nodeView = new NodeView(node, BehaviourTreeEditorWindow.Instance.nodeXml, this);
            AddElement(nodeView);
            return nodeView;
        }

        public void AddChild(NodeView parentView, NodeView childView) {

            // Delete Previous output connections
            if (parentView.output.capacity == Port.Capacity.Single) {
                RemoveElements(parentView.output.connections);
            }

            // Delete previous child's parent
            RemoveElements(childView.input.connections);

            CreateEdgeView(parentView, childView);
        }

        void CreateEdgeView(NodeView parentView, NodeView childView) {
            Edge edge = parentView.output.ConnectTo(childView.input);
            AddElement(edge);
        }

        public void RemoveElements(IEnumerable<GraphElement> elementsToRemove) {
            dontUpdateModel = true;
            DeleteElements(elementsToRemove); // Just need to delete the ui elements without causing a graphChangedEvent here.
            dontUpdateModel = false;
        }

        public void UpdateNodeStates() {
            if (serializer == null) {
                return;
            }

            if (serializer.tree == null) {
                return;
            }

            if (serializer.tree.treeContext == null) {
                return;
            }

            var tickResults = serializer.tree.treeContext.tickResults;
            if (tickResults != null) {
                nodes.ForEach(n => {
                    NodeView view = n as NodeView;
                    view.UpdateState(tickResults);
                });
            }
        }

        public void SelectNode(NodeView nodeView) {
            ClearSelection();
            if (nodeView != null) {
                AddToSelection(nodeView);
            }
        }
        public void SelectNode(Node node) {
            var nodeView = FindNodeView(node);
            SelectNode(nodeView);
        }

        public void InspectNode(Node node) {
            var nodeView = FindNodeView(node);
            OnNodeSelected(nodeView);
        }

        internal void DeleteNodeView(Node n) {
            var nodeView = FindNodeView(n);
            if (nodeView != null) {
                if (nodeView.input != null) {
                    RemoveElements(nodeView.input.connections);
                }
                if (nodeView.output != null) {
                    RemoveElements(nodeView.output.connections);
                }
                RemoveElement(nodeView);
            }
        }

        public void CreateSubTree(NodeView nodeView) {

            BehaviourTreeEditorWindow window = BehaviourTreeEditorWindow.Instance;


            InspectNode(null);

            BehaviourTree tree = EditorUtility.CreateNewTree();
            if (tree) {


                var subTreeRootParent = nodeView.NodeParent.node;
                var subTreeRoot = nodeView.node;

                // Create new Subtree asset
                {
                    SerializedBehaviourTree newTree = new SerializedBehaviourTree(tree);
                    newTree.BeginBatch();
                    newTree.CloneTree(subTreeRoot, tree.rootNode);
                    newTree.EndBatch();
                    window.NewTab(tree, false);
                }

                // Replace subtree with subtree node
                {
                    serializer.BeginBatch();

                    // Create New SubTree Node with the newly created asset
                    var subTreeNode = serializer.CreateNode<SubTree>(subTreeRoot.position) as SubTree;
                    serializer.SetNodeProperty(subTreeNode, nameof(SubTree.treeAsset), tree);

                    // Unparent subtree from it's parent
                    serializer.RemoveChild(subTreeRootParent, subTreeRoot);

                    // Parent Subtree node to previous parent
                    serializer.AddChild(subTreeRootParent, subTreeNode);

                    // Delete remaining subtree
                    serializer.DeleteTree(subTreeRoot);

                    serializer.EndBatch();

                    // Refresh view, focus new subtree node
                    PopulateView(serializer);
                    InspectNode(subTreeNode);
                    SelectNode(subTreeNode);
                }
            }
        }
    }
}
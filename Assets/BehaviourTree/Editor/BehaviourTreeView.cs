using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

namespace TheKiwiCoder {
    public class BehaviourTreeView : GraphView {
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, GraphView.UxmlTraits> { }

        public Action<NodeView> OnNodeSelected;

        SerializedBehaviourTree serializer;
        bool dontUpdateModel = false;

        public BehaviourTreeView() {
            
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new HierarchySelector());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            viewTransformChanged += OnViewTransformChanged;

            nodeCreationRequest = NodeCreationRequest;
        }

        void NodeCreationRequest(NodeCreationContext ctx) {
            Debug.Log("Node Creation Request");
        }

        void OnViewTransformChanged(GraphView graphView) {
            Vector3 position = contentViewContainer.transform.position;
            Vector3 scale = contentViewContainer.transform.scale;
            serializer.SetViewTransform(position, scale);
        }

        public NodeView FindNodeView(Node node) {
            return GetNodeByGuid(node.guid) as NodeView;
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
                    Debug.Assert(childView != null, $"Null child view after deserialising parent{parentView.node.GetType().Name}");
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

            foreach(var elem in blockedDeletes) {
                graphViewChange.elementsToRemove.Remove(elem);  
            }

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            // base.BuildContextualMenu(evt); // Disable default cut/copy/paste context menu options.. who uses those anyway?
            CreateNodeWindow.Show(evt.mousePosition, null);
        }

        void SelectFolder(string path) {
            // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
            // Check the path has no '/' at the end, if it does remove it,
            // Obviously in this example it doesn't but it might
            // if your getting the path some other way.

            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
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
            foreach(var connection in childView.input.connections) {
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

        NodeView CreateNodeView(Node node) {
            NodeView nodeView = new NodeView(node, BehaviourTreeEditorWindow.Instance.nodeXml);
            AddElement(nodeView);
            nodeView.OnNodeSelected = OnNodeSelected;
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
            nodes.ForEach(n => {
                NodeView view = n as NodeView;
                view.UpdateState();
            });
        }

        public void SelectNode(NodeView nodeView) {
            ClearSelection();
            if (nodeView != null) {
                AddToSelection(nodeView);
            }
        }
    }
}
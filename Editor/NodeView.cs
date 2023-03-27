using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace TheKiwiCoder {

    public class NodeView : UnityEditor.Experimental.GraphView.Node {
        public Action<NodeView> OnNodeSelected;
        public Node node;
        public Port input;
        public Port output;

        public NodeView NodeParent {
            get {
                using (IEnumerator<Edge> iter = input.connections.GetEnumerator()) {
                    iter.MoveNext();
                    return iter.Current?.output.node as NodeView;
                }
            }
        }

        public List<NodeView> NodeChildren {
            get {
                // This is untested and may not work. Possibly output should be input.
                List<NodeView> children = new List<NodeView>();
                foreach(var edge in output.connections) {
                    NodeView child = edge.output.node as NodeView;
                    if (child != null) {
                        children.Add(child);
                    }
                }
                return children;
            }
        }

        public NodeView(Node node, VisualTreeAsset nodeXml) : base(AssetDatabase.GetAssetPath(nodeXml)) {
            this.capabilities &= ~(Capabilities.Snappable); // Disable node snapping
            this.node = node;
            this.title = node.GetType().Name;
            this.viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();

            this.AddManipulator(new DoubleClickNode());
        }
        
        public void SetupDataBinding() {
            SerializedBehaviourTree serializer = BehaviourTreeEditorWindow.Instance.serializer;
            var nodeProp = serializer.FindNode(serializer.Nodes, node);
            if (nodeProp != null) {
                var descriptionProp = nodeProp.FindPropertyRelative("description");
                if (descriptionProp != null) {
                    Label descriptionLabel = this.Q<Label>("description");
                    descriptionLabel.BindProperty(descriptionProp);
                }
            }
        }

        private void SetupClasses() {
            if (node is ActionNode) {
                AddToClassList("action");
            } else if (node is CompositeNode) {
                AddToClassList("composite");
            } else if (node is DecoratorNode) {
                AddToClassList("decorator");
            } else if (node is RootNode) {
                AddToClassList("root");
            }
        }

        private void CreateInputPorts() {
            if (node is ActionNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is CompositeNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is DecoratorNode) {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } else if (node is RootNode) {

            }

            if (input != null) {
                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts() {
            if (node is ActionNode) {
                // Actions have no outputs
            } else if (node is CompositeNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            } else if (node is DecoratorNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            } else if (node is RootNode) {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            }

            if (output != null) {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        public override void SetPosition(Rect newPos) {
            newPos.x = EditorUtility.RoundTo(newPos.x, BehaviourTreeView.gridSnapSize);
            newPos.y = EditorUtility.RoundTo(newPos.y, BehaviourTreeView.gridSnapSize);

            base.SetPosition(newPos);

            SerializedBehaviourTree serializer = BehaviourTreeEditorWindow.Instance.serializer;
            Vector2 position = new Vector2(newPos.xMin, newPos.yMin);
            serializer.SetNodePosition(node, position);
        }

        public override void OnSelected() {
            base.OnSelected();
            if (OnNodeSelected != null) {
                OnNodeSelected.Invoke(this);
            }
        }

        public void SortChildren() {
            if (node is CompositeNode composite) {
                composite.children.Sort(SortByHorizontalPosition);
            }
        }

        private int SortByHorizontalPosition(Node left, Node right) {
            return left.position.x < right.position.x ? -1 : 1;
        }

        public void UpdateState() {

            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            if (Application.isPlaying) {
                switch (node.state) {
                    case Node.State.Running:
                        if (node.started) {
                            AddToClassList("running");
                        }
                        break;
                    case Node.State.Failure:
                        AddToClassList("failure");
                        break;
                    case Node.State.Success:
                        AddToClassList("success");
                        break;
                }
            }
        }
    }
}
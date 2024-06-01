using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System.Linq;
using UnityEngine.XR;

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
            if (nodeProp == null) {
                return;
            }

            if (node is ActionNode) {
                Label descriptionLabel = this.Q<Label>("description");
                descriptionLabel.text = node.GetType().Name;
            }
            
            if (node is CompositeNode) {
                var descriptionProp = nodeProp.FindPropertyRelative("description");
                Label descriptionLabel = this.Q<Label>("description");
                SetDescriptionFieldVisible(descriptionLabel, descriptionProp);
                descriptionLabel.TrackPropertyValue(descriptionProp, (property) => {
                    SetDescriptionFieldVisible(descriptionLabel, property);
                });
                descriptionLabel.BindProperty(descriptionProp);
            }

            if (node is ConditionNode) {
                var invertProperty = nodeProp.FindPropertyRelative("invert");
                var icon = this.Q<VisualElement>("icon");
                icon.TrackPropertyValue(invertProperty, UpdateConditionNodeClasses);
            }
        }

        void SetDescriptionFieldVisible(Label label, SerializedProperty property) {
            if (property.stringValue == null || property.stringValue.Length == 0) {
                label.style.display = DisplayStyle.None;
            } else {
                label.style.display = DisplayStyle.Flex;
            }
        }

        private void UpdateConditionNodeClasses(SerializedProperty obj) {
            if (obj.boolValue) {
                AddToClassList("invert");
            } else {
                RemoveFromClassList("invert");
            }
        }

        private void SetupClasses() {
            if (node is ActionNode) {
                AddToClassList("action");

                if (node is ConditionNode conditionNode) {
                    AddToClassList("condition");
                    if (conditionNode.invert) {
                        AddToClassList("invert");
                    }
                }
            } else if (node is CompositeNode) {
                AddToClassList("composite");
                if (node is Sequencer) {
                    AddToClassList("sequencer");
                } else if (node is Selector) {
                    AddToClassList("selector");
                } else if (node is Parallel) {
                    AddToClassList("parallel");
                }
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

            var projectSettings = BehaviourTreeEditorWindow.Instance.settings;

            newPos.x = EditorUtility.SnapTo(newPos.x, projectSettings.gridSnapSizeX);
            newPos.y = EditorUtility.SnapTo(newPos.y, projectSettings.gridSnapSizeY);

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

        public void UpdateState(Dictionary<string, Node.State> tickResults) {
            if (Application.isPlaying) {
                Node.State tickResult;
                if(tickResults.TryGetValue(node.guid, out tickResult)) {
                    ApplyActiveNodeStateStyle(tickResult);
                } else {
                    ApplyInactiveNodeStateStyle();
                }
            }
        }

        void ApplyActiveNodeStateStyle(Node.State state) {
            style.borderLeftWidth = 5;
            style.borderRightWidth = 5;
            style.borderTopWidth = 5;
            style.borderBottomWidth = 5;
            style.opacity = 1.0f;

            if (state == Node.State.Success) {
                style.borderLeftColor = Color.green;
                style.borderRightColor = Color.green;
                style.borderTopColor = Color.green;
                style.borderBottomColor = Color.green;
            }
            else if (state == Node.State.Failure) {
                style.borderLeftColor = Color.red;
                style.borderRightColor = Color.red;
                style.borderTopColor = Color.red;
                style.borderBottomColor = Color.red;
            }
            else if (state == Node.State.Running) {
                style.borderLeftColor = Color.yellow;
                style.borderRightColor = Color.yellow;
                style.borderTopColor = Color.yellow;
                style.borderBottomColor = Color.yellow;
            }
        }

        void ApplyInactiveNodeStateStyle() {
            style.borderLeftWidth = 0;
            style.borderRightWidth = 0;
            style.borderTopWidth = 0;
            style.borderBottomWidth = 0;
            style.opacity = 0.5f;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTreeBuilder
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        private readonly Label _descriptionLabel;
        private Color color;
        public Port input;
        public Node node;
        public Action<NodeView> OnNodeSelected;
        public Port output;

        public NodeView(Node node, VisualTreeAsset nodeXml) : base(AssetDatabase.GetAssetPath(nodeXml))
        {
            capabilities &= ~Capabilities.Snappable; // Disable node snapping
            this.node = node;
            title = ObjectNames.NicifyVariableName(node.GetType().Name);
            viewDataKey = node.guid;
            _descriptionLabel = this.Q<Label>("description");

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            AddPortStyle();
            SetNodeColor();

            this.AddManipulator(new DoubleClickNode());
        }

        public NodeView NodeParent
        {
            get
            {
                using (var iter = input.connections.GetEnumerator())
                {
                    iter.MoveNext();
                    return iter.Current?.output.node as NodeView;
                }
            }
        }

        public List<NodeView> NodeChildren
        {
            get
            {
                // This is untested and may not work. Possibly output should be input.
                var children = new List<NodeView>();
                foreach (var edge in output.connections)
                {
                    var child = edge.output.node as NodeView;
                    if (child != null) children.Add(child);
                }

                return children;
            }
        }

        private void AddPortStyle()
        {
            this.Query<NodePort>().ForEach(port =>
                {
                    port.style.height = 14;
                    port.style.width = 20;
                }
            );
        }

        private void SetNodeColor()
        {
            var settings = BehaviourTreeEditorWindow.Instance.settings;
            if (node is ActionNode)
                color = settings.ActionNodeColor;
            else if (node is CompositeNode)
                color = settings.CompositeNodeColor;
            else if (node is DecoratorNode)
                color = settings.DecoratorNodeColor;
            else if (node is RootNode) color = settings.RootNodeColor;

            if (input != null) input.portColor = color;

            if (output != null) output.portColor = color;
            this.Query<VisualElement>(classes: "node-port").ForEach(element => element.style.backgroundColor = color);
        }

        private void SetupClasses()
        {
            if (node is ActionNode)
                AddToClassList("action");
            else if (node is CompositeNode)
                AddToClassList("composite");
            else if (node is DecoratorNode)
                AddToClassList("decorator");
            else if (node is RootNode) AddToClassList("root");
        }

        private void CreateInputPorts()
        {
            if (node is ActionNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (node is CompositeNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (node is DecoratorNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (node is RootNode)
            {
            }

            if (input != null)
            {
                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts()
        {
            if (node is ActionNode)
            {
                // Actions have no outputs
            }
            else if (node is CompositeNode)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }
            else if (node is DecoratorNode)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            }
            else if (node is RootNode)
            {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            }

            if (output != null)
            {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        public override void SetPosition(Rect newPos)
        {
            newPos.x = EditorUtility.RoundTo(newPos.x, BehaviourTreeView.gridSnapSize);
            newPos.y = EditorUtility.RoundTo(newPos.y, BehaviourTreeView.gridSnapSize);

            base.SetPosition(newPos);

            var serializer = BehaviourTreeEditorWindow.Instance.serializer;
            var position = new Vector2(newPos.xMin, newPos.yMin);
            serializer.SetNodePosition(node, position);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (OnNodeSelected != null) OnNodeSelected.Invoke(this);
        }

        public void SortChildren()
        {
            if (node is CompositeNode composite) composite.children.Sort(SortByHorizontalPosition);
        }

        private int SortByHorizontalPosition(Node left, Node right)
        {
            return left.position.x < right.position.x ? -1 : 1;
        }

        public void UpdateDescription()
        {
            var desc = node.OnShowDescription();
            if (_descriptionLabel.text != desc) _descriptionLabel.text = desc;
        }

        public void UpdateState()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            if (input != null)
                foreach (var edge in input.connections)
                {
                    var flowingEdge = (FlowingEdge)edge;
                    flowingEdge.UpdateFlow();
                }

            switch (node.state)
            {
                case Node.State.Running:
                    if (node.started)
                    {
                        AddToClassList("running");
                        EnableFlowing(true);
                    }

                    break;
                case Node.State.Failure:
                    AddToClassList("failure");
                    EnableFlowing(false);
                    break;
                case Node.State.Success:
                    AddToClassList("success");
                    EnableFlowing(false);
                    break;
                case Node.State.Idle:
                    EnableFlowing(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(node.state.ToString(), node.state, "");
            }
        }

        private void EnableFlowing(bool enable)
        {
            if (input != null)
                foreach (var edge in input.connections)
                {
                    var flowingEdge = (FlowingEdge)edge;
                    flowingEdge.EnableFlow = enable;
                    if (enable)
                    {
                        flowingEdge.edgeControl.edgeWidth = 6;
                        flowingEdge.edgeControl.inputColor = new Color(0, 1, 1);
                        flowingEdge.edgeControl.outputColor = new Color(0, 1, 1);
                    }
                }
        }
    }
}
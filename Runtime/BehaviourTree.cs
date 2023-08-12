using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheKiwiCoder {
    [CreateAssetMenu()]
    public class BehaviourTree : ScriptableObject {

        [SerializeReference]
        public RootNode rootNode;

        [SerializeReference]
        public List<Node> nodes = new List<Node>();

        public Node.State treeState = Node.State.Running;

        public Blackboard blackboard = new Blackboard();

        #region  EditorProperties 
        public Vector3 viewPosition = new Vector3(600, 300);
        public Vector3 viewScale = Vector3.one;
        #endregion

        public BehaviourTree() {
            rootNode = new RootNode();
            nodes.Add(rootNode);
        }

        private void OnEnable() {
            // Validate the behaviour tree on load, removing all null children
            nodes.RemoveAll(node => node == null);
            Traverse(rootNode, node => {
                if (node is CompositeNode composite) {
                    composite.children.RemoveAll(child => child == null);
                }
            });
        }

        public Node.State Update() {
            if (treeState == Node.State.Running) {
                treeState = rootNode.Update();
            }
            return treeState;
        }

        public static List<Node> GetChildren(Node parent) {
            IHasChild hasChild = parent as IHasChild;

            return hasChild != null
                ? hasChild.GetChildren()
                : new List<Node>();
        }

        public static void Traverse(Node node, System.Action<Node> visiter) {
            if (node != null) {
                visiter.Invoke(node);
                var children = GetChildren(node);
                children.ForEach((n) => Traverse(n, visiter));
            }
        }

        public BehaviourTree Clone() {
            BehaviourTree tree = Instantiate(this);
            return tree;
        }

        public void Bind(Context context) {
            Traverse(rootNode, node => {
                node.context = context;
                node.blackboard = blackboard;
                node.OnInit();
            });
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace TheKiwiCoder {
    [CreateAssetMenu()]
    public class BehaviourTree : ScriptableObject {

        [SerializeReference]
        public Node rootNode;

        [SerializeReference]
        public List<Node> nodes = new List<Node>();

        public Node.State treeState = Node.State.Running;

        public Blackboard blackboard = new Blackboard();

        public Node.State Update() {
            if (rootNode.state == Node.State.Running) {
                treeState = rootNode.Update();
            }
            return treeState;
        }

        public static List<Node> GetChildren(Node parent) {
            List<Node> children = new List<Node>();

            if (parent is DecoratorNode decorator && decorator.child != null) {
                children.Add(decorator.child);
            }

            if (parent is RootNode rootNode && rootNode.child != null) {
                children.Add(rootNode.child);
            }

            if (parent is CompositeNode composite) {
                return composite.children;
            }

            return children;
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
            });
        }


        #region Editor Compatibility
#if UNITY_EDITOR

        public Node CreateNode(System.Type type) {
            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");

            Node node = System.Activator.CreateInstance(type) as Node;
            node.guid = GUID.Generate().ToString();
            nodes.Add(node);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node) {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            nodes.Remove(node);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parent, Node child) {
            Undo.RecordObject(this, "Behaviour Tree (AddChild)");

            if (parent is DecoratorNode decorator) {
                decorator.child = child;
            }

            if (parent is RootNode rootNode) {
                rootNode.child = child;
            }

            if (parent is CompositeNode composite) {
                composite.children.Add(child);
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void RemoveChild(Node parent, Node child) {
            Undo.RecordObject(this, "Behaviour Tree (RemoveChild)");

            if (parent is DecoratorNode decorator) {
                decorator.child = null;
            }

            if (parent is RootNode rootNode) {
                rootNode.child = null;
            }

            if (parent is CompositeNode composite) {
                composite.children.Remove(child);
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
        #endregion Editor Compatibility
    }
}
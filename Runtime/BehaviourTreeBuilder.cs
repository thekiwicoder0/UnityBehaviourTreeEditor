using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BehaviourTreeBuilder
{
    
    // This is a WIP class for generating behaviour trees dynamically at runtime.
    public class BehaviourTreeBuilder
    {
        public BehaviourTree tree;

        public BehaviourTreeBuilder(string treeName)
        {
            tree = ScriptableObject.CreateInstance<BehaviourTree>();
            tree.name = treeName;
        }

        public T CreateNode<T>(params object[] args) where T : Node
        {
            var newNode = Activator.CreateInstance(typeof(T), args) as T;
            newNode.guid = Guid.NewGuid().ToString();
            tree.nodes.Add(newNode);
            return newNode;
        }

        public void Selector(params Node[] nodes)
        {
            var selector = CreateNode<Selector>();
            selector.children.AddRange(nodes);
            tree.nodes.AddRange(nodes);
        }

        private void LayoutNodes()
        {
            CalculatePositions(tree.rootNode, tree.rootNode.position, 80.0f);
        }

        private float CalculatePositions(Node node, Vector2 position, float verticalSpacing)
        {
            node.position = position;
            var currentX = position.x;

            var children = BehaviourTree.GetChildren(node);
            foreach (var child in children)
            {
                currentX += CalculateTreeWidth(child) / 2.0f;
                CalculatePositions(child, new Vector2(currentX, position.y + verticalSpacing), verticalSpacing);
                currentX += CalculateTreeWidth(child) / 2.0f;
            }

            return currentX;
        }

        private float CalculateTreeWidth(Node node)
        {
            var children = BehaviourTree.GetChildren(node);
            if (children.Count == 0) return 100.0f;

            var totalWidth = children.Sum(c => CalculateTreeWidth(c));
            return totalWidth;
        }
        
#if UNITY_EDITOR
        public void Save(string path)
        {
            if (tree)
            {
                LayoutNodes();
                AssetDatabase.CreateAsset(tree, $"{path}/{tree.name}.asset");
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif


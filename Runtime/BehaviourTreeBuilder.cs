#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheKiwiCoder {
    public class BehaviourTreeBuilder {

        public BehaviourTree tree;

        public BehaviourTreeBuilder(string treeName) {
            tree = ScriptableObject.CreateInstance<BehaviourTree>();
            tree.name = treeName;
        }

        public T CreateNode<T>(params object[] args) where T : Node {
            T newNode = Activator.CreateInstance(typeof(T), args) as T;
            newNode.guid = GUID.Generate().ToString();
            tree.nodes.Add(newNode);
            return newNode;
        }

        public void Selector(params Node[] nodes) {
            Selector selector = CreateNode<Selector>();
            selector.children.AddRange(nodes);
            tree.nodes.AddRange(nodes);
        }

        void LayoutNodes() {
            CalculatePositions(tree.rootNode, tree.rootNode.position, 80.0f);
        }

        float CalculatePositions(Node node, Vector2 position, float verticalSpacing) {
            node.position = position;
            float currentX = position.x;

            var children = BehaviourTree.GetChildren(node);
            foreach (var child in children) {
                currentX += CalculateTreeWidth(child) / 2.0f;
                CalculatePositions(child, new Vector2(currentX, position.y + verticalSpacing), verticalSpacing);
                currentX += CalculateTreeWidth(child) / 2.0f;
            }

            return currentX;
        }

        float CalculateTreeWidth(Node node) {
            var children = BehaviourTree.GetChildren(node);
            if (children.Count == 0) {
                return 100.0f;
            }

            float totalWidth = children.Sum(c => CalculateTreeWidth(c));
            return totalWidth;
        }

        public void Save(string path) {
            if (tree) {
                LayoutNodes();
                AssetDatabase.CreateAsset(tree, $"{path}/{tree.name}.asset");
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
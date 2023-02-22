using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public class BehaviourTreeRunner : MonoBehaviour {

        // The main behaviour tree asset
        public BehaviourTree tree;

        // Storage container object to hold game object subsystems
        Context context;

        // Start is called before the first frame update
        void Start() {
            string cyclePath;
            if (!IsRecursive(tree, out cyclePath)) {
                context = CreateBehaviourTreeContext();
                tree = tree.Clone();
                tree.Bind(context);
            } else {
                tree = null;
                Debug.LogError($"Failed to create recursive behaviour tree. Found cycle at: {cyclePath}");
            }
        }

        // Update is called once per frame
        void Update() {
            if (tree) {
                tree.Update();
            }
        }

        Context CreateBehaviourTreeContext() {
            return Context.CreateFromGameObject(gameObject);
        }

        bool IsRecursive(BehaviourTree tree, out string cycle) {
            
            List<string> treeStack = new List<string>();
            HashSet<BehaviourTree> referencedTrees = new HashSet<BehaviourTree>();
            

            bool cycleFound = false;
            string cyclePath = "";


            System.Action<Node> traverse = null;
            traverse = (node) => {
                if (!cycleFound) {
                    if (node is SubTree subtree) {
                        treeStack.Add(subtree.treeAsset.name);
                        if (referencedTrees.Contains(subtree.treeAsset)) {
                            int index = 0;
                            foreach(var tree in treeStack) {
                                index++;
                                if (index == treeStack.Count) {
                                    cyclePath += $"{tree}";
                                } else {
                                    cyclePath += $"{tree} -> ";
                                }
                            }

                            cycleFound = true;
                        } else {
                            referencedTrees.Add(subtree.treeAsset);
                            BehaviourTree.Traverse(subtree.treeAsset.rootNode, traverse);
                            referencedTrees.Remove(subtree.treeAsset);
                        }
                        treeStack.RemoveAt(treeStack.Count - 1);
                    }
                }
            };
            treeStack.Add(tree.name);

            referencedTrees.Add(tree);
            BehaviourTree.Traverse(tree.rootNode, traverse);
            referencedTrees.Remove(tree);

            treeStack.RemoveAt(treeStack.Count-1);
            cycle = cyclePath;
            return cycleFound;
        }

        private void OnDrawGizmosSelected() {
            if (!tree) {
                return;
            }

            BehaviourTree.Traverse(tree.rootNode, (n) => {
                if (n.drawGizmos) {
                    n.OnDrawGizmos();
                }
            });
        }
    }
}
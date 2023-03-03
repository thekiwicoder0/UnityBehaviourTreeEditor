using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    [AddComponentMenu("TheKiwiCoder/BehaviourTreeInstance")]
    public class BehaviourTreeInstance : MonoBehaviour {

        // The main behaviour tree asset
        [Tooltip("BehaviourTree asset to instantiate during Awake")] 
        public BehaviourTree behaviourTree;
        
        [Tooltip("Run behaviour tree validation at startup (Can be disabled for release)")] 
        public bool validate = true;

        // These values override the keys in the blackboard
        public List<BlackboardKeyValuePair> blackboardOverrides = new List<BlackboardKeyValuePair>();

        // Storage container object to hold game object subsystems
        Context context;

        // Start is called before the first frame update
        void Awake() {
            bool isValid = ValidateTree();
            if (isValid) {
                context = CreateBehaviourTreeContext();
                behaviourTree = behaviourTree.Clone();
                behaviourTree.Bind(context);

                ApplyKeyOverrides();
            } else {
                behaviourTree = null;
            }
        }

        void ApplyKeyOverrides() {
            foreach(var pair in blackboardOverrides) {
                // Find the key from the new behaviour tree instance
                var targetKey = behaviourTree.blackboard.Find(pair.key.name);
                var sourceKey = pair.value;
                if (targetKey != null && sourceKey != null) {
                    targetKey.CopyFrom(sourceKey);
                }
            }
        }

        // Update is called once per frame
        void Update() {
            if (behaviourTree) {
                behaviourTree.Update();
            }
        }

        Context CreateBehaviourTreeContext() {
            return Context.CreateFromGameObject(gameObject);
        }

        bool ValidateTree() {
            bool isValid = true;
            if (validate) {
                string cyclePath;
                isValid = !IsRecursive(behaviourTree, out cyclePath);

                if (!isValid) {
                    Debug.LogError($"Failed to create recursive behaviour tree. Found cycle at: {cyclePath}");
                }
            }

            return isValid;
        }

        bool IsRecursive(BehaviourTree tree, out string cycle) {
            
            // Check if any of the subtree nodes and their decendents form a circular reference, which will cause a stack overflow.
            List<string> treeStack = new List<string>();
            HashSet<BehaviourTree> referencedTrees = new HashSet<BehaviourTree>();

            bool cycleFound = false;
            string cyclePath = "";

            System.Action<Node> traverse = null;
            traverse = (node) => {
                if (!cycleFound) {
                    if (node is SubTree subtree && subtree.treeAsset != null) {
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
            if (!behaviourTree) {
                return;
            }

            BehaviourTree.Traverse(behaviourTree.rootNode, (n) => {
                if (n.drawGizmos) {
                    n.OnDrawGizmos();
                }
            });
        }

        public BlackboardKey<T> FindBlackboardKey<T>(string keyName) {
            if (behaviourTree) {
                return behaviourTree.blackboard.Find<T>(keyName);
            }
            return null;
        }

        public void SetBlackboardValue<T>(string keyName, T value) {
            if (behaviourTree) {
                behaviourTree.blackboard.SetValue(keyName, value);
            }
        }

        public T GetBlackboardValue<T>(string keyName) {
            if (behaviourTree) {
                return behaviourTree.blackboard.GetValue<T>(keyName);
            }
            return default(T);
        }
    }
}
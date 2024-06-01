using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace TheKiwiCoder {

    [AddComponentMenu("TheKiwiCoder/BehaviourTreeInstance")]
    public class BehaviourTreeInstance : MonoBehaviour {

        public enum TickMode {
            None, // Use this update method to manually update the tree by calling ManualTick()
            FixedUpdate,
            Update,
            LateUpdate
        };

        public enum StartMode {
            None, // Use this start method to manually start the tree by calling StartBehaviour()
            OnEnable,
            OnAwake,
            OnStart
        };

        // The main behaviour tree asset
        [Tooltip("BehaviourTree asset to instantiate during Awake")]
        public BehaviourTree behaviourTree;

        [Tooltip("When to update this behaviour tree in the frame")]
        public TickMode tickMode = TickMode.Update;

        [Tooltip("When to start this behaviour tree")]
        public StartMode startMode = StartMode.OnStart;

        [Tooltip("Run behaviour tree validation at startup (Can be disabled for release)")]
        public bool validate = true;

        [Tooltip("Override / set blackboard key values for this behaviour tree instance")]
        public List<BlackboardKeyValuePair> blackboardOverrides = new List<BlackboardKeyValuePair>();
        
        public BehaviourTree RuntimeTree {
            get {
                if (runtimeTree != null) {
                    return runtimeTree;
                } else {
                    return behaviourTree;
                }
            }
        }

        // Runtime tree instance
        BehaviourTree runtimeTree;

        // Storage container object to hold game object subsystems
        Context context;

        // Profile markers
        static readonly ProfilerMarker profileUpdate = new ProfilerMarker("BehaviourTreeInstance.Update");

        // Tree state from last tick
        Node.State treeState = Node.State.Running;

        void OnEnable() {
            if (startMode == StartMode.OnEnable) {
                StartBehaviour(behaviourTree);
            }
        }

        private void Awake() {
            if (startMode == StartMode.OnAwake) {
                StartBehaviour(behaviourTree);
            }
        }

        private void Start() {
            if (startMode == StartMode.OnStart) {
                StartBehaviour(behaviourTree);
            }
        }

        void ApplyBlackboardOverrides() {
            foreach (var pair in blackboardOverrides) {
                // Find the key from the new behaviour tree instance
                var targetKey = runtimeTree.blackboard.Find(pair.key.name);
                var sourceKey = pair.value;
                if (targetKey != null && sourceKey != null) {
                    targetKey.CopyFrom(sourceKey);
                }
            }
        }

        void InternalUpdate(float tickDelta) {
            if (runtimeTree) {
                profileUpdate.Begin();
                treeState = runtimeTree.Tick(tickDelta);
                profileUpdate.End();
            }
        }

        void FixedUpdate() {
            if (tickMode == TickMode.FixedUpdate) {
                InternalUpdate(Time.fixedDeltaTime);
            }
        }

        void Update() {
            if (tickMode == TickMode.Update) {
                InternalUpdate(Time.deltaTime);
            }
        }

        void LateUpdate() {
            if (tickMode == TickMode.LateUpdate) {
                InternalUpdate(Time.deltaTime);
            }
        }

        public void ManualTick(float tickDelta) {
            if (tickMode != TickMode.None) {
                Debug.LogWarning($"Manually ticking the behaviour tree while in {tickMode} will cause duplicate updates");
            }
            InternalUpdate(tickDelta);
        }

        public void StartBehaviour(BehaviourTree tree) {
            bool isValid = ValidateTree(tree);
            if (isValid) {
                InstantiateTree(tree);
            } else {
                runtimeTree = null;
            }
        }

        public void InstantiateTree(BehaviourTree tree) {
            context = CreateBehaviourTreeContext();
            runtimeTree = tree.Clone();
            runtimeTree.Bind(context);
            ApplyBlackboardOverrides();
        }

        Context CreateBehaviourTreeContext() {
            return Context.CreateFromGameObject(gameObject);
        }

        bool ValidateTree(BehaviourTree tree) {
            if (!tree) {
                Debug.LogWarning($"No BehaviourTree assigned to {name}, assign a behaviour tree in the inspector");
                return false;
            }

            bool isValid = true;
            if (validate) {
                string cyclePath;
                isValid = !IsRecursive(tree, out cyclePath);

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
                            foreach (var tree in treeStack) {
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

            treeStack.RemoveAt(treeStack.Count - 1);
            cycle = cyclePath;
            return cycleFound;
        }

        private void OnDrawGizmosSelected() {
            if (!Application.isPlaying) {
                return;
            }

            if (!runtimeTree) {
                return;
            }

            BehaviourTree.Traverse(runtimeTree.rootNode, (n) => {
                if (n.drawGizmos) {
                    n.OnDrawGizmos();
                }
            });
        }

        public BlackboardKey<T> FindBlackboardKey<T>(string keyName) {
            if (runtimeTree) {
                return runtimeTree.blackboard.Find<T>(keyName);
            }
            return null;
        }

        public void SetBlackboardValue<T>(string keyName, T value) {
            if (runtimeTree) {
                runtimeTree.blackboard.SetValue(keyName, value);
            }
        }

        public T GetBlackboardValue<T>(string keyName) {
            if (runtimeTree) {
                return runtimeTree.blackboard.GetValue<T>(keyName);
            }
            return default(T);
        }
    }
}
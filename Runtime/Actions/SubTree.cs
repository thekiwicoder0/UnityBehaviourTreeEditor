using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public class SubTree : ActionNode {
        
        [Tooltip("Behaviour tree asset to run as a subtree")] public BehaviourTree treeAsset;
        [HideInInspector] public BehaviourTree treeInstance;

        public override void OnInit() {
            if (treeAsset) {
                treeInstance = treeAsset.Clone();
                treeInstance.Bind(context);
            }
        }

        protected override void OnStart() {
            
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            if (treeInstance) {
                return treeInstance.Tick(context.tickDelta);
            }
            return State.Failure;
        }
    }
}

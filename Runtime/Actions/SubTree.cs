using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class SubTree : ActionNode
    {
        [Tooltip("Behaviour tree asset to run as a subtree")]
        public BehaviourTree TreeAsset;
        [NonSerialized] public BehaviourTree TreeInstance;

        public override void OnInit()
        {
            if (TreeAsset)
            {
                TreeInstance = TreeAsset.Clone();
                TreeInstance.Bind(context);
            }
        }

        protected override void OnStart()
        {
            if (TreeInstance) TreeInstance.treeState = State.Running;
        }

        protected override void OnStop()
        {
            if (state == State.Idle)
            {
                TreeInstance.Abort();
            }
        }

        protected override void OnFixedUpdate()
        {
            TreeInstance.FixedUpdate();
        }

        protected override State OnUpdate()
        {
            if (TreeInstance) return TreeInstance.Update();
            return State.Failure;
        }
        
        protected override void OnLateUpdate()
        {
            TreeInstance.LateUpdate();
        }

        public override string OnShowDescription()
        {
            return $"Sub Tree: {TreeAsset?.name}";
        }
    }
}
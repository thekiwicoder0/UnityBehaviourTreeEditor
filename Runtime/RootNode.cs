using System;
using UnityEngine;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class RootNode : Node
    {
        [SerializeReference] [HideInInspector] public Node child;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override void OnFixedUpdate()
        {
            child.FixedUpdate();
        }

        protected override State OnUpdate()
        {
            if (child != null)
                return child.Update();
            return State.Failure;
        }

        protected override void OnLateUpdate()
        {
            child.LateUpdate();
        }
    }
}
using System;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class Succeed : DecoratorNode
    {
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
            if (child == null) return State.Failure;

            var state = child.Update();
            if (state == State.Failure) return State.Success;
            return state;
        }

        protected override void OnLateUpdate()
        {
            child.LateUpdate();
        }
    }
}
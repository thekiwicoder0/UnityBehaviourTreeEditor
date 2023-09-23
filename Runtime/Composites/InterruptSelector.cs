using System;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class InterruptSelector : Selector
    {
        protected override State OnUpdate()
        {
            var previous = current;
            base.OnStart();
            var status = base.OnUpdate();
            if (previous != current)
                if (children[previous].state == State.Running)
                    children[previous].Abort();

            return status;
        }
    }
}
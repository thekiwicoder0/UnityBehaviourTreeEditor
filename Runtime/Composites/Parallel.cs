using System;
using System.Collections.Generic;
using System.Linq;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class Parallel : CompositeNode
    {
        private List<State> childrenLeftToExecute = new();

        protected override void OnStart()
        {
            childrenLeftToExecute.Clear();
            children.ForEach(a => { childrenLeftToExecute.Add(State.Running); });
        }

        protected override void OnStop()
        {
        }

        protected override void OnFixedUpdate()
        {
            for (var i = 0; i < childrenLeftToExecute.Count(); ++i) children[i].FixedUpdate();
        }

        protected override State OnUpdate()
        {
            var stillRunning = false;
            for (var i = 0; i < childrenLeftToExecute.Count(); ++i)
                if (childrenLeftToExecute[i] == State.Running)
                {
                    var status = children[i].Update();
                    if (status == State.Failure)
                    {
                        AbortRunningChildren();
                        return State.Failure;
                    }

                    if (status == State.Running) stillRunning = true;

                    childrenLeftToExecute[i] = status;
                }

            return stillRunning ? State.Running : State.Success;
        }

        protected override void OnLateUpdate()
        {
            for (var i = 0; i < childrenLeftToExecute.Count(); ++i) children[i].LateUpdate();
        }

        private void AbortRunningChildren()
        {
            for (var i = 0; i < childrenLeftToExecute.Count(); ++i)
                if (childrenLeftToExecute[i] == State.Running)
                    children[i].Abort();
        }
    }
}
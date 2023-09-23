using System;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class Sequencer : CompositeNode
    {
        protected int current;

        protected override void OnStart()
        {
            current = 0;
        }

        protected override void OnStop()
        {
        }

        protected override void OnFixedUpdate()
        {
            children[current].FixedUpdate();
        }

        protected override State OnUpdate()
        {
            for (var i = current; i < children.Count; ++i)
            {
                current = i;
                var child = children[current];

                switch (child.Update())
                {
                    case State.Running:
                        return State.Running;
                    case State.Failure:
                        return State.Failure;
                    case State.Success:
                        continue;
                }
            }

            return State.Success;
        }

        protected override void OnLateUpdate()
        {
            children[current].LateUpdate();
        }
    }
}
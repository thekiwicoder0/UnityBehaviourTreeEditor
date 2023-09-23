using System;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class Selector : CompositeNode
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
                    case State.Success:
                        return State.Success;
                    case State.Failure:
                        continue;
                }
            }

            return State.Failure;
        }

        protected override void OnLateUpdate()
        {
            children[current].LateUpdate();
        }

        public override string OnShowDescription()
        {
            return $"Current: {current}";
        }
    }
}
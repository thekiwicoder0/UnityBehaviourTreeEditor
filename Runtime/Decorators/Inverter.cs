using System;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class Inverter : DecoratorNode
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

            switch (child.Update())
            {
                case State.Running:
                    return State.Running;
                case State.Failure:
                    return State.Success;
                case State.Success:
                    return State.Failure;
            }

            return State.Failure;
        }

        protected override void OnLateUpdate()
        {
            child.FixedUpdate();
        }
    }
}
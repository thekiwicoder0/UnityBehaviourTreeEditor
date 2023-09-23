using System;
using UnityEngine;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class Repeat : DecoratorNode
    {
        [Tooltip("Restarts the subtree on success")]
        public bool restartOnSuccess = true;

        [Tooltip("Restarts the subtree on failure")]
        public bool restartOnFailure;

        [Tooltip("Maximum number of times the subtree will be repeated. Set to 0 to loop forever")]
        public int maxRepeats;

        private int iterationCount;

        protected override void OnStart()
        {
            iterationCount = 0;
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
                    break;
                case State.Failure:
                    if (restartOnFailure)
                    {
                        iterationCount++;
                        child.Abort();
                        if (iterationCount == maxRepeats && maxRepeats > 0)
                            return State.Failure;
                        return State.Running;
                    }

                    return State.Failure;
                case State.Success:
                    if (restartOnSuccess)
                    {
                        iterationCount++;
                        child.Abort();
                        if (iterationCount == maxRepeats && maxRepeats > 0)
                            return State.Success;
                        return State.Running;
                    }

                    return State.Success;
            }

            return State.Running;
        }

        protected override void OnLateUpdate()
        {
            child.LateUpdate();
        }

        public override string OnShowDescription()
        {
            return
                $"restartOnSuccess:{restartOnSuccess} \nrestartOnFailure:{restartOnFailure} \nmaxRepeats:{maxRepeats}";
        }
    }
}
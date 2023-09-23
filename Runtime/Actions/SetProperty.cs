using System;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class SetProperty : ActionNode
    {
        public BlackboardKeyValuePair pair;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            pair.WriteValue();

            return State.Success;
        }

        public override string OnShowDescription()
        {
            return $"{pair.key.name}{pair.value}";
        }
    }
}
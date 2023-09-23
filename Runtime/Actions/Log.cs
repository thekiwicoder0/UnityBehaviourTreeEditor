using System;
using UnityEngine;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public class Log : ActionNode
    {
        [Tooltip("Message to log to the console")]
        public NodeProperty<string> message = new();

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            Debug.Log($"{message.Value}");
            return State.Success;
        }

        public override string OnShowDescription()
        {
            return $"Log message: {message.Value}";
        }
    }
}
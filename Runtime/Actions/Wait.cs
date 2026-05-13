using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public class Wait : ActionNode {

		[Tooltip("Amount of time to wait before returning success")] public NodeProperty<float> duration = new NodeProperty<float>() { Value = 1.0f };
        
        float startTime;

        protected override void OnStart() {
            startTime = Time.time;
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {

            float timeRemaining = Time.time - startTime;
            if (timeRemaining > duration.Value) {
                return State.Success;
            }
            return State.Running;
        }
    }
}

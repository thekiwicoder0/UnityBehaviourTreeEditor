using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

namespace TheKiwiCoder {

    [System.Serializable]
    public class MoveToPosition : ActionNode {
        [Tooltip("How fast to move")] public float speed = 5;
        [Tooltip("Stop within this distance of the target")] public float stoppingDistance = 0.1f;
        [Tooltip("Updates the agents rotation along the path")] public bool updateRotation = true;
        [Tooltip("Maximum acceleration when following the path")] public float acceleration = 40.0f;
        [Tooltip("Returns success when the remaining distance is less than this amount")] public float tolerance = 1.0f;
        public BlackboardProperty<Vector3> moveKey;

        protected override void OnStart() {
            context.agent.stoppingDistance = stoppingDistance;
            context.agent.speed = speed;
            context.agent.destination = moveKey.Value;
            context.agent.updateRotation = updateRotation;
            context.agent.acceleration = acceleration;
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            if (context.agent.pathPending) {
                return State.Running;
            }

            if (context.agent.remainingDistance < tolerance) {
                return State.Success;
            }

            if (context.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid) {
                return State.Failure;
            }

            return State.Running;
        }
    }
}

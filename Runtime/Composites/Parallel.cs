using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TheKiwiCoder {
    [System.Serializable]
    public class Parallel : CompositeNode {

        public int successThreshold = 1;

        protected override void OnStart() {
            
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            var childCount = children.Count;

            int successCount = 0;
            int failureCount = 0;

            for (int i = 0; i < childCount; ++i) {
                var childState = children[i].Update();
                if (childState == State.Success) {
                    successCount++;
                } else if (childState == State.Failure) {
                    failureCount++;
                }
            }

            if (successCount >= successThreshold) {
                return State.Success;
            }

            if (failureCount > (childCount-successThreshold)) {
                return State.Failure;
            }

            return State.Running;
        }
    }
}
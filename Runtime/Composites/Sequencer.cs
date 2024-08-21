using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    [System.Serializable]
    public class Sequencer : CompositeNode {
        int lastNodeFinished;

        protected override void OnStart() {
            lastNodeFinished = -1;
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            for (int i = 0; i < children.Count; ++i) {
                if (i <= lastNodeFinished) continue;

                var childStatus = children[i].Update();

                if (childStatus == State.Running) {
                    return State.Running;
                } else if (childStatus == State.Failure) {
                    return State.Failure;
                } else if (childStatus == State.Success) {
                    lastNodeFinished = i;
                }
            }

            return State.Success;
        }
    }
}
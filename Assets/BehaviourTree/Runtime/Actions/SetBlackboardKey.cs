using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

namespace TheKiwiCoder {

    [System.Serializable]
    public class SetBlackboardKey : ActionNode
    {
        public BlackboardKeyValuePair pair;

        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            BlackboardKey source = pair.value;
            BlackboardKey destination = pair.key;

            if (source != null && destination != null) {
                destination.CopyFrom(source);
            }
            
            return State.Success;
        }
    }
}

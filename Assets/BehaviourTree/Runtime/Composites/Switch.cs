using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    [System.Serializable]
    public class Switch : CompositeNode
    {
        public BlackboardProperty<int> switchKey;
        public bool interruptable = true;
        int index = 0;

        protected override void OnStart() {
            if (switchKey.IsValid()) {
                index = switchKey.Value;
            }
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            if (!switchKey.IsValid()) { 
                return State.Failure;
            }
            
            if (interruptable) {
                int nextIndex = switchKey.Value;
                if (nextIndex != index) {
                    children[index].Abort();
                }
                index = nextIndex;
            }

            if (index < children.Count) {
                return children[index].Update();
            }
            return State.Failure;
        }
    }
}


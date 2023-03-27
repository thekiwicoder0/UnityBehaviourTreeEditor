using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    [System.Serializable]
    public class Switch : CompositeNode
    {
        public NodeProperty<int> index;
        public bool interruptable = true;
        int currentIndex = 0;

        protected override void OnStart() {
            currentIndex = index.Value;
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            
            if (interruptable) {
                int nextIndex = index.Value;
                if (nextIndex != currentIndex) {
                    children[currentIndex].Abort();
                }
                currentIndex = nextIndex;
            }

            if (currentIndex < children.Count) {
                return children[currentIndex].Update();
            }
            return State.Failure;
        }
    }
}


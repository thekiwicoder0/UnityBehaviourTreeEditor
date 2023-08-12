using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public class RootNode : Node, IHasChild {

        [SerializeReference]
        [HideInInspector] 
        public Node child;

        protected override void OnStart() {

        }

        protected override void OnStop() {

        }

        protected override State OnUpdate() {
            if (child != null) {
                return child.Update();
            } else {
                return State.Failure;
            }
        }

        public void AddChild(Node child)
        {
            this.child = child;
        }

        public void RemoveChild(Node child)
        {
            this.child = null;
        }

        public List<Node> GetChildren()
        {
            return new List<Node>() { this.child };
        }
    }
}
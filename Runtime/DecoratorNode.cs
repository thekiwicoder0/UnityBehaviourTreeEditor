using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    public abstract class DecoratorNode : Node, IHasChild {

        [SerializeReference]
        [HideInInspector] 
        public Node child;

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

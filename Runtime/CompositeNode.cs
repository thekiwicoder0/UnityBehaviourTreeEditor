using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public abstract class CompositeNode : Node, IHasChild {

        [HideInInspector] 
        [SerializeReference]
        public List<Node> children = new List<Node>();

        public void AddChild(Node child)
        {
            this.children.Add(child);
        }

        public void RemoveChild(Node child)
        {
            this.children.Remove(child);
        }

        public List<Node> GetChildren()
        {
            return this.children;
        }
    }
}
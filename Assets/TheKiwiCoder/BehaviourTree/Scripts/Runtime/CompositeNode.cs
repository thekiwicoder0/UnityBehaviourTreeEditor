using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public abstract class CompositeNode : Node {

        [SerializeReference]
        [HideInInspector] 
        public List<Node> children = new List<Node>();
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public abstract class CompositeNode : Node
    {
        [HideInInspector] [SerializeReference] public List<Node> children = new();
    }
}
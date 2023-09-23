using UnityEngine;

namespace BehaviourTreeBuilder
{
    public abstract class DecoratorNode : Node
    {
        [SerializeReference] [HideInInspector] public Node child;
    }
}
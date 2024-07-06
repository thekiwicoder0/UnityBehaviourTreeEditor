using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public abstract class Node {
        public enum State {
            Running,
            Failure,
            Success
        }

        [HideInInspector] public bool started = false;
        [HideInInspector] public string guid = System.Guid.NewGuid().ToString();
        [HideInInspector] public Vector2 position;
        [HideInInspector] public Context context;
        [HideInInspector] public Blackboard blackboard;
        [TextArea] public string description;
        [Tooltip("When enabled, the nodes OnDrawGizmos will be invoked")] public bool drawGizmos = false;

        public virtual void OnInit() {
            // Nothing to do here
        }

        public State Update() {

            if (!started) {
                OnStart();
                started = true;
            }

            var state = OnUpdate();

            context.tickResults[guid] = state;

            if (state != State.Running) {
                OnStop();
                started = false;
            }

            return state;
        }

        public void Abort() {
            BehaviourTree.Traverse(this, (node) => {
                node.started = false;
                node.OnStop();
            });
        }

        public virtual void OnDrawGizmos() { }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();

        protected virtual void Log(string message) {
            Debug.Log($"[{GetType()}]{message}");
        }

        public Node Clone() {
            var clone = MemberwiseClone() as Node;

            // Only clone this node. Child references will be cleared
            if (clone is DecoratorNode decorator && decorator.child != null) {
                decorator.child = null;
            }

            if (clone is RootNode rootNode && rootNode.child != null) {
                rootNode.child = null;
            }

            if (clone is CompositeNode composite) {
                composite.children = new List<Node>();
            }

            return clone;
        }
    }
}
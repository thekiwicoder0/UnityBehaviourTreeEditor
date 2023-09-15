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

        [HideInInspector] public State state = State.Running;
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

            state = OnUpdate();

            if (state != State.Running) {
                OnStop();
                started = false;
            }

            return state;
        }

        public void Abort() {
            BehaviourTree.Traverse(this, (node) => {
                node.started = false;
                node.state = State.Running;
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
    }
}
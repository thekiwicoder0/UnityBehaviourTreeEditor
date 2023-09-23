using System;
using UnityEngine;

namespace BehaviourTreeBuilder
{
    [Serializable]
    public abstract class Node
    {
        public enum State
        {
            Idle,
            Running,
            Failure,
            Success
        }

        [HideInInspector] public string guid = Guid.NewGuid().ToString();
        [HideInInspector] public Vector2 position;

        [Tooltip("When enabled, the nodes OnDrawGizmos will be invoked")]
        public bool drawGizmos;

        [NonSerialized] public Blackboard blackboard;
        [NonSerialized] public Context context;
        [NonSerialized] public bool started;

        [NonSerialized] public State state = State.Idle;

        public virtual void OnInit()
        {
            // Nothing to do here
        }

        public void FixedUpdate()
        {
            if (state == State.Running) OnFixedUpdate();
        }

        public State Update()
        {
            if (state == State.Idle) state = State.Running;
            if (!started)
            {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state != State.Running)
            {
                OnStop();
                started = false;
            }

            return state;
        }

        public void LateUpdate()
        {
            if (state == State.Running) OnLateUpdate();
        }

        public void Abort()
        {
            BehaviourTree.Traverse(this, node =>
            {
                node.started = false;
                node.state = State.Idle;
                node.OnStop();
            });
        }

        // OnFixedUpdate executes during the FixedUpdate loop. The Node Status must be returned within OnUpdate
        protected virtual void OnFixedUpdate()
        {
            // Nothing to do here
        }

        // OnLateUpdate executes during the LateUpdate loop. The Node Status must be returned within OnUpdate
        protected virtual void OnLateUpdate()
        {
            // Nothing to do here
        }

        public virtual string OnShowDescription()
        {
            return "";
        }

        public virtual void OnDrawGizmos()
        {
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();
    }
}
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BehaviourTreeBuilder
{
    [System.Serializable]
    public class CheckLayerInRange : ActionNode
    {
        [SerializeField] private NodeProperty<float> _detectRadius = new NodeProperty<float>(10);
        [SerializeField] private float _tickFrequency = 0.1f;
        [SerializeField] private NodeProperty<LayerMask> _layerMask;
        private float _timer;

        // OnStart is called immediately before execution. It is used to setup any variables that need to be reset from the previous run.
        protected override void OnStart() {
            _timer = 0;
        }
    
        // OnStop is called after execution on a success or failure.
        protected override void OnStop() {
        }
    
        // OnUpdate runs the actual task.
        protected override State OnUpdate()
        {
            _timer += Time.deltaTime;
            if (_timer >= _tickFrequency)
            {
#if CORE_3D
                var hitCollider = Physics.OverlapSphere(context.Transform.position, _detectRadius.Value, _layerMask.Value);
#else
                var hitCollider = Physics2D.OverlapCircleAll(context.Transform.position, _detectRadius.Value, _layerMask.Value);
#endif
                if (hitCollider.Length > 0)
                {
                    return State.Success;
                }
                
                return State.Failure;
            }
        
            return State.Running;
        }

        public override void OnDrawGizmos()
        {
            if(context == null) return;
            Gizmos.color = state == State.Success ? Color.green : Color.red;
            Gizmos.DrawWireSphere(context.Transform.position, _detectRadius.Value);
        }

        public override string OnShowDescription()
        {
            return $"Detect Radius: {_detectRadius.Value} \n Detected: {state == State.Success}";
        }
    }
}
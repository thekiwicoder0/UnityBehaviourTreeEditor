using UnityEngine;
using UnityEngine.AI;

namespace BehaviourTreeBuilder
{
    // The context is a shared object every node has access to.
    // Commonly used components and subsytems should be stored here
    // It will be somewhat specfic to your game exactly what to add here.
    // Feel free to extend this class 
    public class Context
    {
        //AI
#if USE_NAVMESH
        public NavMeshAgent Agent;
#endif
        
#if USE_CHARACTER_CONTROLLER
        public CharacterController CharacterController;
#endif
        //Common
        public Animator Animator;
        public GameObject GameObject;
        public Transform Transform;
        
        //3D
#if CORE_3D
        public BoxCollider BoxCollider;
        public CapsuleCollider CapsuleCollider;
        public Rigidbody Rigidbody;
        public SphereCollider SphereCollider;
#else
        
        //2D
        public BoxCollider2D BoxCollider2D;
        public CapsuleCollider2D CapsuleCollider2D;
        public Rigidbody2D Rigidbody2D;
        public CircleCollider2D CircleCollider2D;
#endif

        // Add other game specific systems here

        public static Context CreateFromGameObject(GameObject gameObject)
        {
            // Fetch all commonly used components
            var context = new Context();
            context.GameObject = gameObject;
            context.Transform = gameObject.transform;
            context.Animator = gameObject.GetComponent<Animator>();
#if USE_NAVMESH
            context.Agent = gameObject.GetComponent<NavMeshAgent>();
#endif
#if USE_CHARACTER_CONTROLLER
            context.CharacterController = gameObject.GetComponent<CharacterController>();
#endif
            
#if CORE_3D
            context.Rigidbody = gameObject.GetComponent<Rigidbody>();
            context.BoxCollider = gameObject.GetComponent<BoxCollider>();
            context.SphereCollider = gameObject.GetComponent<SphereCollider>();
            context.CapsuleCollider = gameObject.GetComponent<CapsuleCollider>();
#else  
            context.Rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
            context.BoxCollider2D = gameObject.GetComponent<BoxCollider2D>();
            context.CapsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D>();
            context.CircleCollider2D = gameObject.GetComponent<CircleCollider2D>();
#endif

            // Add whatever else you need here...

            return context;
        }
    }
}

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BehaviourTreeBuilder
{
    // The Blackboard stores a set of keys which can be used to pass values between different nodes in the behaviour tree.
    // Nodes can read and write to blackboard keys to share data.
    // 
    // To get a reference to a BlackboardKey from a Node, add a BlackboardProperty<T> variable to your node. 
    // You can then read and write to the key using the variable.Value property. 
    //
    // The BlackboardProperty<T> class is only supported inside Nodes within a tree.
    // To read/write to a BlackboardKey from a non-node script (e.g a MonoBehaviour) use the GetValue<T>/SetValue<T> api.
    // The key can be cached during Awake() using Find<T> and written to directly using .value if reading/writing to the key
    // frequently.
    //
    // Note: A BlackboardProperty holds a reference to a BlackboardKey and is not the key itself. It is not intended
    // to add a BlackboardKey directly to a Node. BlackboardKeys are stored uniquely in the blackboard itself, and referenced
    // externally from nodes via the BlackboardProperty<T> class.
    //
    // Example:
    // BlackboardProperty<GameObject> gameObjectProperty;
    // ...
    // gameObjectProperty.Value = gameObject;                 // Set the value
    // GameObject gameObject = gameObjectProperty.Value;      // Get the value
    // 
    // 
    // To define a new blackboard key type, create a subclass of BlackboardKey.
    //
    // Example:
    // [System.Serializable]
    // public class BooleanKey : BlackboardKey<bool> {
    // 
    // }
    //
    // Blackboard keys will be drawn in the Editor using a PropertyField. If you have a custom type, define a PropertyDrawer
    // to customise it's appearance. 
    // 
    // Note: Subtrees have their own unique copy of the blackboard.
    // 
    [Serializable]
    public class Blackboard
    {
        [TableList(HideToolbar = true)]
        [SerializeReference] public List<BlackboardKey> keys = new();

        // Finds the first key which matches keyName
        public BlackboardKey Find(string keyName)
        {
            return keys.Find(key => { return key.name == keyName; });
        }

        // Finds a key that matches keyName with the type specified
        public BlackboardKey<T> Find<T>(string keyName)
        {
            var foundKey = Find(keyName);

            if (foundKey == null)
            {
                Debug.LogWarning($"Failed to find blackboard key, invalid keyname:{keyName}");
                return null;
            }

            if (foundKey.underlyingType != typeof(T))
            {
                Debug.LogWarning(
                    $"Failed to find blackboard key, invalid keytype:{typeof(T)}, Expected:{foundKey.underlyingType}");
                return null;
            }

            var foundKeyTyped = foundKey as BlackboardKey<T>;
            if (foundKeyTyped == null)
            {
                Debug.LogWarning(
                    $"Failed to find blackboard key, casting failed:{typeof(T)}, Expected:{foundKey.underlyingType}");
                return null;
            }

            return foundKeyTyped;
        }

        // Tries to set a key to a value using the type specified.
        // Note: This may fail if the key with the matching name has a different type to the one specified
        public void SetValue<T>(string keyName, T value)
        {
            var key = Find<T>(keyName);
            if (key != null) key.value = value;
        }

        // Tries to get a key value using the type specified.
        // Note: This may fail if the key with the matching name has a different type to the one specified
        public T GetValue<T>(string keyName)
        {
            var key = Find<T>(keyName);
            if (key != null) return key.value;
            return default;
        }
    }
}
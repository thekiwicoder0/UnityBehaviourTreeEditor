using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    // The Blackboard stores a set of keys which can be used to pass values between different nodes in the behaviour tree.
    // Nodes can read and write to blackboard keys to share data.
    // 
    // To get a reference to a BlackboardKey from a Node, add a BlackboardProperty<T> variable to your node. 
    // You can then read and write to the key using the variable.Value property.
    //
    // Note: A BlackboardProperty holds a reference to a BlackboardKey and is not the key itself. It is not intended
    // to add a BlackboardKey directly to a Node. BlackboardKeys are stored uniquely in the blackboard itself, and referenced
    // externally from nodes via the BlackboardProperty type.
    //
    // Example:
    // BlackboardProperty<GameObject> gameObjectProperty;
    // ...
    // gameObjectProperty.Value = gameObject;
    // GameObject gameObject = gameObjectProperty.Value;
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
    [System.Serializable]
    public class Blackboard {

        [SerializeReference]
        public List<BlackboardKey> keys = new List<BlackboardKey>();

        public BlackboardKey Find(string keyName) {
            return keys.Find((key) => {
                return key.name == keyName;
            });
        }
    }
}
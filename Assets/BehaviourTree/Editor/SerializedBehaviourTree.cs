using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TheKiwiCoder {

    // This is a helper class which wraps a serialized object for finding properties on the behaviour.
    // It's best to modify the behaviour tree via SerializedObjects and SerializedProperty interfaces
    // to keep the UI in sync, and undo/redo
    // It's a hodge podge mix of various functions that will evolve over time. It's not exhaustive by any means.
    [System.Serializable]
    public class SerializedBehaviourTree {

        // Wrapper serialized object for writing changes to the behaviour tree
        public SerializedObject serializedObject;

        [SerializeReference]
        public BehaviourTree tree;

        // Property names. These correspond to the variable names on the behaviour tree
        const string sPropRootNode = "rootNode";
        const string sPropNodes = "nodes";
        const string sPropBlackboard = "blackboard";
        const string sPropGuid = "guid";
        const string sPropChild = "child";
        const string sPropChildren = "children";
        const string sPropPosition = "position";
        const string sViewTransformPosition = "viewPosition";
        const string sViewTransformScale = "viewScale";
        const string sBlackboardKeys = "keys";

        public SerializedProperty RootNode {
            get {
                return serializedObject.FindProperty(sPropRootNode);
            }
        }

        public SerializedProperty Nodes {
            get {
                return serializedObject.FindProperty(sPropNodes);
            }
        }

        public SerializedProperty Blackboard {
            get {
                return serializedObject.FindProperty(sPropBlackboard);
            }
        }

        public SerializedProperty BlackboardKeys {
            get {
                return serializedObject.FindProperty(sPropBlackboard).FindPropertyRelative(sBlackboardKeys);
            }
        }

        // Start is called before the first frame update
        public SerializedBehaviourTree(BehaviourTree tree)
        {
            serializedObject = new SerializedObject(tree);
            this.tree = tree;
        }

        public void Save() {
            serializedObject.ApplyModifiedProperties();
        }

        public SerializedProperty FindNode(SerializedProperty array, Node node) {
            for(int i = 0; i < array.arraySize; ++i) {
                var current = array.GetArrayElementAtIndex(i);
                if (current.FindPropertyRelative(sPropGuid).stringValue == node.guid) {
                    return current;
                }
            }
            return null;
        }

        public void SetViewTransform(Vector3 position, Vector3 scale) {
            serializedObject.FindProperty(sViewTransformPosition).vector3Value = position;
            serializedObject.FindProperty(sViewTransformScale).vector3Value = scale;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public void SetNodePosition(Node node, Vector2 position) {
            var nodeProp = FindNode(Nodes, node);
            nodeProp.FindPropertyRelative(sPropPosition).vector2Value = position;
            serializedObject.ApplyModifiedProperties();
        }

        public void DeleteNode(SerializedProperty array, Node node) {
            for (int i = 0; i < array.arraySize; ++i) {
                var current = array.GetArrayElementAtIndex(i);
                if (current.FindPropertyRelative(sPropGuid).stringValue == node.guid) {
                    array.DeleteArrayElementAtIndex(i);
                    return;
                }
            }
        }

        public Node CreateNodeInstance(System.Type type) {
            Node node = System.Activator.CreateInstance(type) as Node;
            node.guid = GUID.Generate().ToString();
            return node;
        }

        SerializedProperty AppendArrayElement(SerializedProperty arrayProperty) {
            arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
            return arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1);
        }

        public Node CreateNode(System.Type type, Vector2 position) {

            Node node = CreateNodeInstance(type);
            node.position = position;

            SerializedProperty newNode = AppendArrayElement(Nodes);
            newNode.managedReferenceValue = node;

            serializedObject.ApplyModifiedProperties();

            return node;
        }

        public void SetRootNode(RootNode node) {
            RootNode.managedReferenceValue = node;
            serializedObject.ApplyModifiedProperties();
        }

        public void DeleteNode(Node node) {

            SerializedProperty nodesProperty = Nodes;

            for(int i = 0; i < nodesProperty.arraySize; ++i) {
                var prop = nodesProperty.GetArrayElementAtIndex(i);
                var guid = prop.FindPropertyRelative(sPropGuid).stringValue;
                DeleteNode(Nodes, node);
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void AddChild(Node parent, Node child) {
            
            var parentProperty = FindNode(Nodes, parent);

            // RootNode, Decorator node
            var childProperty = parentProperty.FindPropertyRelative(sPropChild);
            if (childProperty != null) {
                childProperty.managedReferenceValue = child;
                serializedObject.ApplyModifiedProperties();
                return;
            }

            // Composite nodes
            var childrenProperty = parentProperty.FindPropertyRelative(sPropChildren);
            if (childrenProperty != null) {
                SerializedProperty newChild = AppendArrayElement(childrenProperty);
                newChild.managedReferenceValue = child;
                serializedObject.ApplyModifiedProperties();
                return;
            }
        }

        public void RemoveChild(Node parent, Node child) {
            var parentProperty = FindNode(Nodes, parent);

            // RootNode, Decorator node
            var childProperty = parentProperty.FindPropertyRelative(sPropChild);
            if (childProperty != null) {
                childProperty.managedReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
                return;
            }

            // Composite nodes
            var childrenProperty = parentProperty.FindPropertyRelative(sPropChildren);
            if (childrenProperty != null) {
                DeleteNode(childrenProperty, child);
                serializedObject.ApplyModifiedProperties();
                return;
            }
        }

        public void CreateBlackboardKey(string keyName, BlackboardKey.Type keyType) {
            SerializedProperty keysArray = BlackboardKeys;
            keysArray.InsertArrayElementAtIndex(keysArray.arraySize);
            SerializedProperty newKey = keysArray.GetArrayElementAtIndex(keysArray.arraySize - 1);

            BlackboardKey key = new BlackboardKey();
            key.name = keyName;
            key.type = keyType;
            newKey.managedReferenceValue = key;

            serializedObject.ApplyModifiedProperties();
        }

        public void DeleteBlackboardKey(string keyName) {
            SerializedProperty keysArray = BlackboardKeys;
            for(int i = 0; i < keysArray.arraySize; ++i) {
                var key = keysArray.GetArrayElementAtIndex(i);
                BlackboardKey itemKey = key.managedReferenceValue as BlackboardKey;
                if (itemKey.name == keyName) {
                    keysArray.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    return;
                }
            }
        }
    }
}

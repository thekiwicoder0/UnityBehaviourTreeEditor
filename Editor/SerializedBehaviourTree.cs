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

        public BehaviourTree tree;

        // Property names. These correspond to the variable names on the behaviour tree
        const string sPropRootNode = nameof(BehaviourTree.rootNode);
        const string sPropNodes = nameof(BehaviourTree.nodes);
        const string sPropBlackboard = nameof(BehaviourTree.blackboard);
        const string sPropBlackboardKeys = nameof(TheKiwiCoder.Blackboard.keys);
        const string sPropGuid = nameof(Node.guid);
        const string sPropChild = nameof(DecoratorNode.child);
        const string sPropChildren = nameof(CompositeNode.children);
        const string sPropPosition = nameof(Node.position);
        const string sViewTransformPosition = nameof(BehaviourTree.viewPosition);
        const string sViewTransformScale = nameof(BehaviourTree.viewScale);
        bool batchMode = false;

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
                return serializedObject.FindProperty($"{sPropBlackboard}.{sPropBlackboardKeys}");
            }
        }

        // Start is called before the first frame update
        public SerializedBehaviourTree(BehaviourTree tree)
        {
            serializedObject = new SerializedObject(tree);
            this.tree = tree;
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
            ApplyChanges();
        }

        public void RemoveNodeArrayElement(SerializedProperty array, Node node) {
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

            Node child = CreateNodeInstance(type);
            child.position = position;

            SerializedProperty newNode = AppendArrayElement(Nodes);
            newNode.managedReferenceValue = child;

            ApplyChanges();

            return child;
        }

        public void SetRootNode(RootNode node) {
            RootNode.managedReferenceValue = node;
            ApplyChanges();
        }

        public void DeleteNode(Node node) {

            SerializedProperty nodesProperty = Nodes;

            for(int i = 0; i < nodesProperty.arraySize; ++i) {
                var prop = nodesProperty.GetArrayElementAtIndex(i);
                var guid = prop.FindPropertyRelative(sPropGuid).stringValue;
                RemoveNodeArrayElement(Nodes, node);
            }

            ApplyChanges();
        }

        public void AddChild(Node parent, Node child) {
            
            var parentProperty = FindNode(Nodes, parent);

            // RootNode, Decorator node
            var childProperty = parentProperty.FindPropertyRelative(sPropChild);
            if (childProperty != null) {
                childProperty.managedReferenceValue = child;
                ApplyChanges();
                return;
            }

            // Composite nodes
            var childrenProperty = parentProperty.FindPropertyRelative(sPropChildren);
            if (childrenProperty != null) {
                SerializedProperty newChild = AppendArrayElement(childrenProperty);
                newChild.managedReferenceValue = child;
                ApplyChanges();
                return;
            }
        }

        public void RemoveChild(Node parent, Node child) {
            var parentProperty = FindNode(Nodes, parent);

            // RootNode, Decorator node
            var childProperty = parentProperty.FindPropertyRelative(sPropChild);
            if (childProperty != null) {
                childProperty.managedReferenceValue = null;
                ApplyChanges();
                return;
            }

            // Composite nodes
            var childrenProperty = parentProperty.FindPropertyRelative(sPropChildren);
            if (childrenProperty != null) {
                RemoveNodeArrayElement(childrenProperty, child);
                ApplyChanges();
                return;
            }
        }

        public void CreateBlackboardKey(string keyName, System.Type keyType) {
            BlackboardKey key = BlackboardKey.CreateKey(keyType);
            if (key != null) {
                key.name = keyName;
                SerializedProperty keysArray = BlackboardKeys;
                keysArray.InsertArrayElementAtIndex(keysArray.arraySize);
                SerializedProperty newKey = keysArray.GetArrayElementAtIndex(keysArray.arraySize - 1);

                newKey.managedReferenceValue = key;

                ApplyChanges();
            } else {
                Debug.LogError($"Failed to create blackboard key, invalid type:{keyType}");
            }
        }

        public void DeleteBlackboardKey(string keyName) {
            SerializedProperty keysArray = BlackboardKeys;
            for(int i = 0; i < keysArray.arraySize; ++i) {
                var key = keysArray.GetArrayElementAtIndex(i);
                BlackboardKey itemKey = key.managedReferenceValue as BlackboardKey;
                if (itemKey.name == keyName) {
                    keysArray.DeleteArrayElementAtIndex(i);
                    ApplyChanges();
                    return;
                }
            }
        }

        public void BeginBatch() {
            batchMode = true;
        }

        public void ApplyChanges() {
            if (!batchMode) {
                serializedObject.ApplyModifiedProperties();
            }
        }

        public void EndBatch() {
            batchMode = false;
            ApplyChanges();
        }
    }
}

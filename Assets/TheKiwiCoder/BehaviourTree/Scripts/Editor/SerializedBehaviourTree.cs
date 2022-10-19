using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TheKiwiCoder
{
    public class SerializedBehaviourTree
    {
        public SerializedObject serializedObject;

        // Start is called before the first frame update
        public SerializedBehaviourTree(BehaviourTree tree)
        {
            serializedObject = new SerializedObject(tree);
        }

        public SerializedProperty FindNode(string guid) {
            var nodes = serializedObject.FindProperty("nodes");
            for(int i = 0; i < nodes.arraySize; ++i) {
                var node = nodes.GetArrayElementAtIndex(i);
                if (node.FindPropertyRelative("guid").stringValue == guid) {
                    return node;
                }
            }
            return null;
        }
    }
}

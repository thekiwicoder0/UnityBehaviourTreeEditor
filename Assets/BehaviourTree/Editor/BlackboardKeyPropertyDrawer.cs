using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace TheKiwiCoder {

    [CustomPropertyDrawer(typeof(FloatVar))]
    [CustomPropertyDrawer(typeof(IntVar))]
    [CustomPropertyDrawer(typeof(BoolVar))]
    [CustomPropertyDrawer(typeof(StringVar))]
    [CustomPropertyDrawer(typeof(Vector2Var))]
    [CustomPropertyDrawer(typeof(Vector3Var))]
    [CustomPropertyDrawer(typeof(GameObjectVar))]
    [CustomPropertyDrawer(typeof(TagVar))]
    [CustomPropertyDrawer(typeof(LayerMaskVar))]
    public class BlackboardKeyPropertyDrawer : PropertyDrawer {

        [SerializeField]
        public VisualTreeAsset asset;

        BehaviourTree tree;
        SerializedProperty itemProp;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            tree = property.serializedObject.targetObject as BehaviourTree;

            itemProp = property.FindPropertyRelative("key");

            string currentValue = "null";
            foreach (var key in tree.blackboard.keys) {
                if (key == itemProp.managedReferenceValue) {
                    currentValue = key.name;
                }
            }

            PopupField<string> dropdown = new PopupField<string>();
            dropdown.label = property.name;
            dropdown.value = currentValue;
            dropdown.RegisterCallback<ChangeEvent<string>>((evt) => {
                itemProp.managedReferenceValue = tree.blackboard.Find(evt.newValue);
                bool changesApplied = itemProp.serializedObject.ApplyModifiedProperties();
            });

            dropdown.RegisterCallback<MouseEnterEvent>((evt) => {
                var choices = new List<string>();
                foreach (var key in tree.blackboard.keys) {

                    // Filter out keys of the same type
                    if (!Matches(key.type, fieldInfo.FieldType)) {
                        continue;
                    }

                    choices.Add(key.name);
                }
                dropdown.choices = choices;
            });

            return dropdown;
        }

        bool Matches(BlackboardKey.Type keyType, System.Type propertyType) {
            switch (keyType) {
                case BlackboardKey.Type.Float:
                    return propertyType == typeof(FloatVar);
                case BlackboardKey.Type.Int:
                    return propertyType == typeof(IntVar);
                case BlackboardKey.Type.Boolean:
                    return propertyType == typeof(BoolVar);
                case BlackboardKey.Type.String:
                    return propertyType == typeof(StringVar);
                case BlackboardKey.Type.Vector2:
                    return propertyType == typeof(Vector2Var);
                case BlackboardKey.Type.Vector3:
                    return propertyType == typeof(Vector3Var);
                case BlackboardKey.Type.GameObject:
                    return propertyType == typeof(GameObjectVar);
                case BlackboardKey.Type.Tag:
                    return propertyType == typeof(TagVar);
                case BlackboardKey.Type.LayerMask:
                    return propertyType == typeof(LayerMaskVar);
                default:
                    Debug.LogError($"Unhandled Key Type:{keyType}:{propertyType}");
                    return false;
            }
        }

    }

}
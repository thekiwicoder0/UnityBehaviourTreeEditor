using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace TheKiwiCoder {

    [CustomPropertyDrawer(typeof(BlackboardKey))]
    public class BlackboardKeyPropertyDrawer : PropertyDrawer {

        [SerializeField]
        public VisualTreeAsset asset;

        BehaviourTree tree;
        SerializedProperty itemProp;

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {

            tree = property.serializedObject.targetObject as BehaviourTree;

            itemProp = property.FindPropertyRelative("item");

            var choices = new List<string>();
            string currentValue = "null";
            foreach (var key in tree.blackboard.items) {
                choices.Add(key.key);

                if (key == itemProp.managedReferenceValue) {
                    currentValue = key.key;
                }
            }

            PopupField<string> dropdown = new PopupField<string>();

            dropdown.label = property.name;
            dropdown.value = currentValue;
            dropdown.choices = choices;
            dropdown.RegisterCallback<ChangeEvent<string>>((evt) => {
                itemProp.managedReferenceValue = tree.blackboard.Find(evt.newValue);
                bool changesApplied = itemProp.serializedObject.ApplyModifiedProperties();

            });

            return dropdown;
        }
    }

}

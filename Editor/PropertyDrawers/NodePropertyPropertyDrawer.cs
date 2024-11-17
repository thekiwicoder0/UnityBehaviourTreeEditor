using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Runtime.Remoting.Messaging;

namespace TheKiwiCoder {

    [CustomPropertyDrawer(typeof(NodeProperty<>), true)]
    public class GenericNodePropertyPropertyDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            
            BehaviourTree tree = property.serializedObject.targetObject as BehaviourTree;

            var genericTypes = fieldInfo.FieldType.GenericTypeArguments;
            var propertyType = genericTypes[0];

            SerializedProperty reference = property.FindPropertyRelative("reference");

            Label label = new Label();
            label.AddToClassList("unity-base-field__label");
            label.AddToClassList("unity-property-field__label");
            label.AddToClassList("unity-property-field");
            label.text = property.displayName;

            PropertyField defaultValueField = new PropertyField();
            defaultValueField.label = "";
            defaultValueField.style.flexGrow = 1.0f;
            defaultValueField.bindingPath = nameof(NodeProperty<int>.defaultValue);

            PopupField<BlackboardKey> dropdown = new PopupField<BlackboardKey>();
            dropdown.label = "";
            dropdown.formatListItemCallback = FormatItem;
            dropdown.formatSelectedValueCallback = FormatSelectedItem;
            dropdown.value = reference.managedReferenceValue as BlackboardKey;
            dropdown.tooltip = "Bind value to a BlackboardKey";
            dropdown.style.flexGrow = 1.0f;
            dropdown.RegisterCallback<MouseEnterEvent>((evt) => {
                dropdown.choices.Clear();
                foreach (var key in tree.blackboard.keys) {
                    if (propertyType.IsAssignableFrom(key.underlyingType)) {
                        dropdown.choices.Add(key);
                    }
                }
                dropdown.choices.Add(null);

                dropdown.choices.Sort((left, right) => {
                    if (left == null) {
                        return -1;
                    }

                    if (right == null) {
                        return 1;
                    }
                    return left.name.CompareTo(right.name);
                });
            });

            dropdown.RegisterCallback<ChangeEvent<BlackboardKey>>((evt) => {
                BlackboardKey newKey = evt.newValue;
                reference.managedReferenceValue = newKey;
                BehaviourTreeEditorWindow.Instance.CurrentSerializer.ApplyChanges();

                if (evt.newValue == null) {
                    defaultValueField.style.display = DisplayStyle.Flex;
                    dropdown.style.flexGrow = 0.0f;
                } else {
                    defaultValueField.style.display = DisplayStyle.None;
                    dropdown.style.flexGrow = 1.0f;
                }
            });

            defaultValueField.style.display = dropdown.value == null ? DisplayStyle.Flex : DisplayStyle.None;
            dropdown.style.flexGrow = dropdown.value == null ? 0.0f : 1.0f;

            VisualElement container = new VisualElement();
            container.AddToClassList("unity-base-field");
            container.AddToClassList("node-property-field");
            container.style.flexDirection = FlexDirection.Row;
            container.Add(label);
            container.Add(defaultValueField);
            container.Add(dropdown);

            return container;
        }

        private string FormatItem(BlackboardKey item) {
            if (item == null) {
                return "[Inline]";
            } else {
                return item.name;
            }
        }

        private string FormatSelectedItem(BlackboardKey item) {
            if (item == null) {
                return "";
            } else {
                return item.name;
            }
        }
    }

    [CustomPropertyDrawer(typeof(NodeProperty), true)]
    public class NodePropertyPropertyDrawer : PropertyDrawer {

        public override VisualElement CreatePropertyGUI(SerializedProperty property) {

            BehaviourTree tree = property.serializedObject.targetObject as BehaviourTree;

            SerializedProperty reference = property.FindPropertyRelative("reference");

            PopupField<BlackboardKey> dropdown = new PopupField<BlackboardKey>();
            dropdown.label = property.displayName;
            dropdown.formatListItemCallback = FormatItem;
            dropdown.formatSelectedValueCallback = FormatItem;
            dropdown.value = reference.managedReferenceValue as BlackboardKey;

            dropdown.RegisterCallback<MouseEnterEvent>((evt) => {
                dropdown.choices.Clear();
                foreach (var key in tree.blackboard.keys) {
                    dropdown.choices.Add(key);
                }
                dropdown.choices.Sort((left, right) => {
                    return left.name.CompareTo(right.name);
                });
            });

            dropdown.RegisterCallback<ChangeEvent<BlackboardKey>>((evt) => {
                BlackboardKey newKey = evt.newValue;
                reference.managedReferenceValue = newKey;
                BehaviourTreeEditorWindow.Instance.CurrentSerializer.ApplyChanges();
            });
            return dropdown;
        }

        private string FormatItem(BlackboardKey item) {
            if (item == null) {
                return "(null)";
            } else {
                return item.name;
            }
        }
    }
}
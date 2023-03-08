using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TheKiwiCoder {

    [CustomPropertyDrawer(typeof(BlackboardKey))]
    public class BlackboardKeyPropertyDrawer : PropertyDrawer {
    
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.ArraySize) {

                Label keyName = new Label();
                TextField renameField = new TextField();
                PropertyField keyValue = new PropertyField();
                VisualElement container = new VisualElement();

                keyName.bindingPath = nameof(BlackboardKey.name);
                keyName.AddToClassList("unity-base-field__label");

                renameField.style.display = DisplayStyle.None;
                renameField.bindingPath = nameof(BlackboardKey.name);
                renameField.RegisterCallback<BlurEvent>((evt) => {
                    keyValue.style.display = DisplayStyle.Flex;
                    keyName.style.display = DisplayStyle.Flex;
                    renameField.style.display = DisplayStyle.None;
                });

                keyValue.label = "";
                keyValue.style.flexGrow = 1.0f;
                keyValue.bindingPath = nameof(BlackboardKey<object>.value);
                keyValue.AddManipulator(new ContextualMenuManipulator((evt) => {
                    evt.menu.AppendAction("Delete", (x) => BehaviourTreeEditorWindow.Instance.serializer.DeleteBlackboardKey(property.displayName), DropdownMenuAction.AlwaysEnabled);
                }));

                container.style.flexDirection = FlexDirection.Row;
                container.Add(keyName);
                container.Add(renameField);
                container.Add(keyValue);

                keyName.RegisterCallback<MouseDownEvent>((evt) => {
                    if(evt.clickCount == 2) {
                        renameField.value = keyName.text;
                        renameField.style.display = DisplayStyle.Flex;
                        renameField.Focus();

                        keyValue.style.display = DisplayStyle.None;
                        keyName.style.display = DisplayStyle.None;
                    }
                });


                return container;
            }
            return null;
        }
    }
}

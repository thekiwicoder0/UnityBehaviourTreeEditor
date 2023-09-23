using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BehaviourTreeBuilder
{
    [CustomPropertyDrawer(typeof(BlackboardKey))]
    public class BlackboardKeyPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ArraySize)
            {
                var keyName = new Label();
                var renameField = new TextField();
                var keyValue = new PropertyField();
                var container = new VisualElement();

                keyName.bindingPath = nameof(BlackboardKey.name);
                keyName.AddToClassList("unity-base-field__label");

                renameField.style.display = DisplayStyle.None;
                renameField.bindingPath = nameof(BlackboardKey.name);
                renameField.RegisterCallback<BlurEvent>(evt =>
                {
                    keyValue.style.display = DisplayStyle.Flex;
                    keyName.style.display = DisplayStyle.Flex;
                    renameField.style.display = DisplayStyle.None;
                });

                keyValue.label = "";
                keyValue.style.flexGrow = 1.0f;
                keyValue.bindingPath = nameof(BlackboardKey<object>.value);

                container.style.flexDirection = FlexDirection.Row;
                container.AddManipulator(new ContextualMenuManipulator(evt =>
                {
                    evt.menu.AppendAction("Delete",
                        x => BehaviourTreeEditorWindow.Instance.serializer
                            .DeleteBlackboardKey(property.displayName), DropdownMenuAction.AlwaysEnabled);
                }));
                container.Add(keyName);
                container.Add(renameField);
                container.Add(keyValue);

                keyName.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.clickCount == 2)
                    {
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
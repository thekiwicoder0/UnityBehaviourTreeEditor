using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BehaviourTreeBuilder
{
    [CustomPropertyDrawer(typeof(NodeProperty<>))]
    public class GenericNodePropertyPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var tree = property.serializedObject.targetObject as BehaviourTree;

            var genericTypes = fieldInfo.FieldType.GenericTypeArguments;
            var propertyType = genericTypes[0];

            var reference = property.FindPropertyRelative("reference");

            var label = new Label();
            label.AddToClassList("unity-base-field__label");
            label.AddToClassList("unity-property-field__label");
            label.AddToClassList("unity-property-field");
            label.text = property.displayName;

            var defaultValueField = new PropertyField();
            defaultValueField.label = "";
            defaultValueField.style.flexGrow = 1.0f;
            defaultValueField.bindingPath = nameof(NodeProperty<int>.defaultValue);

            var dropdown = new PopupField<BlackboardKey>();
            dropdown.label = "";
            dropdown.formatListItemCallback = FormatItem;
            dropdown.formatSelectedValueCallback = FormatSelectedItem;
            dropdown.value = reference.managedReferenceValue as BlackboardKey;
            dropdown.tooltip = "Bind value to a BlackboardKey";
            dropdown.style.flexGrow = 1.0f;
            dropdown.RegisterCallback<MouseEnterEvent>(evt =>
            {
                dropdown.choices.Clear();
                foreach (var key in tree.blackboard.keys)
                    if (propertyType.IsAssignableFrom(key.underlyingType))
                        dropdown.choices.Add(key);
                dropdown.choices.Add(null);

                dropdown.choices.Sort((left, right) =>
                {
                    if (left == null) return -1;

                    if (right == null) return 1;
                    return left.name.CompareTo(right.name);
                });
            });

            dropdown.RegisterCallback<ChangeEvent<BlackboardKey>>(evt =>
            {
                var newKey = evt.newValue;
                reference.managedReferenceValue = newKey;
                BehaviourTreeEditorWindow.Instance.serializer.ApplyChanges();

                if (evt.newValue == null)
                {
                    defaultValueField.style.display = DisplayStyle.Flex;
                    dropdown.style.flexGrow = 0.0f;
                }
                else
                {
                    defaultValueField.style.display = DisplayStyle.None;
                    dropdown.style.flexGrow = 1.0f;
                }
            });

            defaultValueField.style.display = dropdown.value == null ? DisplayStyle.Flex : DisplayStyle.None;
            dropdown.style.flexGrow = dropdown.value == null ? 0.0f : 1.0f;

            var container = new VisualElement();
            container.AddToClassList("unity-base-field");
            container.AddToClassList("node-property-field");
            container.style.flexDirection = FlexDirection.Row;
            container.Add(label);
            container.Add(defaultValueField);
            container.Add(dropdown);
            container.tooltip = property.tooltip;

            return container;
        }

        private string FormatItem(BlackboardKey item)
        {
            if (item == null)
                return "[Inline]";
            return item.name;
        }

        private string FormatSelectedItem(BlackboardKey item)
        {
            if (item == null)
                return "";
            return item.name;
        }
    }

    [CustomPropertyDrawer(typeof(NodeProperty))]
    public class NodePropertyPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var tree = property.serializedObject.targetObject as BehaviourTree;

            var reference = property.FindPropertyRelative("reference");

            var dropdown = new PopupField<BlackboardKey>();
            dropdown.label = property.displayName;
            dropdown.formatListItemCallback = FormatItem;
            dropdown.formatSelectedValueCallback = FormatItem;
            dropdown.value = reference.managedReferenceValue as BlackboardKey;

            dropdown.RegisterCallback<MouseEnterEvent>(evt =>
            {
                dropdown.choices.Clear();
                foreach (var key in tree.blackboard.keys) dropdown.choices.Add(key);
                dropdown.choices.Sort((left, right) => { return left.name.CompareTo(right.name); });
            });

            dropdown.RegisterCallback<ChangeEvent<BlackboardKey>>(evt =>
            {
                var newKey = evt.newValue;
                reference.managedReferenceValue = newKey;
                BehaviourTreeEditorWindow.Instance.serializer.ApplyChanges();
            });
            return dropdown;
        }

        private string FormatItem(BlackboardKey item)
        {
            if (item == null)
                return "(null)";
            return item.name;
        }
    }
}
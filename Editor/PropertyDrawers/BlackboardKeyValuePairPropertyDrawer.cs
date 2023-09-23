using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTreeBuilder
{
    [CustomPropertyDrawer(typeof(BlackboardKeyValuePair))]
    public class BlackboardKeyValuePairPropertyDrawer : PropertyDrawer
    {
        private VisualElement pairContainer;

        private BehaviourTree GetBehaviourTree(SerializedProperty property)
        {
            if (property.serializedObject.targetObject is BehaviourTree tree)
                return tree;
            if (property.serializedObject.targetObject is BehaviourTreeInstance instance) return instance.RuntimeTree;
            Debug.LogError("Could not find behaviour tree this is referencing");
            return null;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var first = property.FindPropertyRelative(nameof(BlackboardKeyValuePair.key));
            var second = property.FindPropertyRelative(nameof(BlackboardKeyValuePair.value));

            var dropdown = new PopupField<BlackboardKey>();
            dropdown.label = first.displayName;
            dropdown.formatListItemCallback = FormatItem;
            dropdown.formatSelectedValueCallback = FormatItem;
            dropdown.value = first.managedReferenceValue as BlackboardKey;

            var tree = GetBehaviourTree(property);
            dropdown.RegisterCallback<MouseEnterEvent>(evt =>
            {
                dropdown.choices.Clear();
                foreach (var key in tree.blackboard.keys) dropdown.choices.Add(key);
            });

            dropdown.RegisterCallback<ChangeEvent<BlackboardKey>>(evt =>
            {
                var newKey = evt.newValue;
                first.managedReferenceValue = newKey;
                property.serializedObject.ApplyModifiedProperties();

                if (pairContainer.childCount > 1) pairContainer.RemoveAt(1);

                if (second.managedReferenceValue == null ||
                    second.managedReferenceValue.GetType() != dropdown.value.GetType())
                {
                    second.managedReferenceValue = BlackboardKey.CreateKey(dropdown.value.GetType());
                    second.serializedObject.ApplyModifiedProperties();
                }

                var field = new PropertyField();
                field.label = second.displayName;
                field.BindProperty(second.FindPropertyRelative(nameof(BlackboardKey<object>.value)));
                pairContainer.Add(field);
            });

            pairContainer = new VisualElement();
            pairContainer.tooltip = property.tooltip;
            pairContainer.Add(dropdown);

            if (dropdown.value != null)
            {
                if (second.managedReferenceValue == null ||
                    first.managedReferenceValue.GetType() != second.managedReferenceValue.GetType())
                {
                    second.managedReferenceValue = BlackboardKey.CreateKey(dropdown.value.GetType());
                    second.serializedObject.ApplyModifiedProperties();
                }

                var field = new PropertyField();
                field.label = second.displayName;
                field.bindingPath = nameof(BlackboardKey<object>.value);
                pairContainer.Add(field);
            }

            return pairContainer;
        }

        private string FormatItem(BlackboardKey item)
        {
            if (item == null)
                return "(null)";
            return item.name;
        }
    }
}
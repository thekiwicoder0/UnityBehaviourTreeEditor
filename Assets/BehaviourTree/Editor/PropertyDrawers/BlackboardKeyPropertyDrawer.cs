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
                PropertyField field = new PropertyField();
                
                field.label = $"{property.displayName}";
                field.bindingPath = nameof(BlackboardKey<object>.value);

                field.AddManipulator(new ContextualMenuManipulator((evt) => {
                    evt.menu.AppendAction("Delete", (x) => BehaviourTreeEditorWindow.Instance.serializer.DeleteBlackboardKey(property.displayName), DropdownMenuAction.AlwaysEnabled);
                }));
                    
                return field;
            }
            return null;
        }
    }
}

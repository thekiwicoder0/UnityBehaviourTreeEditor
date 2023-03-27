using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TheKiwiCoder {
    [CustomEditor(typeof(BehaviourTreeInstance))]
    public class BehaviourTreeInstanceEditor : Editor {

        public override VisualElement CreateInspectorGUI() {

            VisualElement container = new VisualElement();

            PropertyField treeField = new PropertyField();
            treeField.bindingPath = nameof(BehaviourTreeInstance.behaviourTree);

            PropertyField validateField = new PropertyField();
            validateField.bindingPath = nameof(BehaviourTreeInstance.validate);

            PropertyField publicKeys = new PropertyField();
            publicKeys.bindingPath = nameof(BehaviourTreeInstance.blackboardOverrides);

            container.Add(treeField);
            container.Add(validateField);
            container.Add(publicKeys);

            return container;
        }
    }
}

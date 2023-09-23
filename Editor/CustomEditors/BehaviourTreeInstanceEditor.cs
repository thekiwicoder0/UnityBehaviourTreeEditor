using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BehaviourTreeBuilder
{
    [CustomEditor(typeof(BehaviourTreeInstance))]
    public class BehaviourTreeInstanceEditor : Editor
    {
        private BehaviourTree _tree;

        private void OnEnable()
        {
            var behaviourTreeInstance = target as BehaviourTreeInstance;
            if (behaviourTreeInstance != null) _tree = behaviourTreeInstance.behaviourTree;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
        
            var treeField = new PropertyField();
            treeField.bindingPath = nameof(BehaviourTreeInstance.behaviourTree);
        
            var validateField = new PropertyField();
            validateField.bindingPath = nameof(BehaviourTreeInstance.validate);
        
            var publicKeys = new PropertyField();
            publicKeys.bindingPath = nameof(BehaviourTreeInstance.blackboardOverrides);
        
            var openEditorButton = new Button(() => BehaviourTreeEditorWindow.OpenWindow(_tree));
            openEditorButton.style.height = 30;
            openEditorButton.style.marginTop = 10;
            openEditorButton.text = "Open In Editor";
        
            container.Add(treeField);
            container.Add(validateField);
            container.Add(publicKeys);
            container.Add(openEditorButton);
        
            return container;
        }
    }
}
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BehaviourTreeBuilder
{
    public class BlackboardView : VisualElement
    {
        private SerializedBehaviourTree behaviourTree;

        private Button createButton;

        private ListView listView;
        private TextField newKeyTextField;
        private PopupField<Type> newKeyTypeField;

        internal void Bind(SerializedBehaviourTree behaviourTree)
        {
            this.behaviourTree = behaviourTree;

            listView = this.Q<ListView>("ListView_Keys");
            newKeyTextField = this.Q<TextField>("TextField_KeyName");
            var popupContainer = this.Q<VisualElement>("PopupField_Placeholder");

            createButton = this.Q<Button>("Button_KeyCreate");

            // ListView
            listView.Bind(behaviourTree.serializedObject);

            newKeyTypeField = new PopupField<Type>();
            newKeyTypeField.label = "Type";
            newKeyTypeField.formatListItemCallback = FormatItem;
            newKeyTypeField.formatSelectedValueCallback = FormatItem;

            var types = TypeCache.GetTypesDerivedFrom<BlackboardKey>();
            foreach (var type in types)
            {
                if (type.IsGenericType) continue;
                newKeyTypeField.choices.Add(type);
                if (newKeyTypeField.value == null) newKeyTypeField.value = type;
            }

            popupContainer.Clear();
            popupContainer.Add(newKeyTypeField);

            // TextField
            newKeyTextField.RegisterCallback<ChangeEvent<string>>(evt => { ValidateButton(); });

            // Button
            createButton.clicked -= CreateNewKey;
            createButton.clicked += CreateNewKey;

            ValidateButton();
        }

        private string FormatItem(Type arg)
        {
            if (arg == null)
                return "(null)";
            return arg.Name.Replace("Key", "");
        }

        private void ValidateButton()
        {
            // Disable the create button if trying to create a non-unique key
            var isValidKeyText = ValidateKeyText(newKeyTextField.text);
            createButton.SetEnabled(isValidKeyText);
        }

        private bool ValidateKeyText(string text)
        {
            if (text == "") return false;

            var tree = behaviourTree.Blackboard.serializedObject.targetObject as BehaviourTree;
            var keyExists = tree.blackboard.Find(newKeyTextField.text) != null;
            return !keyExists;
        }

        private void CreateNewKey()
        {
            var newKeyType = newKeyTypeField.value;
            if (newKeyType != null) behaviourTree.CreateBlackboardKey(newKeyTextField.text, newKeyType);
            ValidateButton();
        }

        public void ClearView()
        {
            behaviourTree = null;
            if (listView != null) listView.Unbind();
        }

        public new class UxmlFactory : UxmlFactory<BlackboardView, UxmlTraits>
        {
        }
    }
}
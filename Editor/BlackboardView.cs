using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace TheKiwiCoder {

    [UxmlElement]
    public partial class BlackboardView : VisualElement {

        private SerializedBehaviourTree behaviourTree;

        private ListView listView;
        private TextField newKeyTextField;
        private PopupField<Type> newKeyTypeField;

        private Button createButton;

        internal void Bind(SerializedBehaviourTree behaviourTree) {
            
            this.behaviourTree = behaviourTree;

            listView = this.Q<ListView>("ListView_Keys");
            newKeyTextField = this.Q<TextField>("TextField_KeyName");
            VisualElement popupContainer = this.Q<VisualElement>("PopupField_Placeholder");
            
            createButton = this.Q<Button>("Button_KeyCreate");

            // ListView
            listView.Bind(behaviourTree.serializedObject);

            newKeyTypeField = new PopupField<Type>();
            newKeyTypeField.label = "Type";
            newKeyTypeField.formatListItemCallback = FormatItem;
            newKeyTypeField.formatSelectedValueCallback = FormatItem;

            var types = TypeCache.GetTypesDerivedFrom<BlackboardKey>();
            foreach(var type in types) {
                if (type.IsGenericType) {
                    continue;
                }
                newKeyTypeField.choices.Add(type);
                if (newKeyTypeField.value == null) {
                    newKeyTypeField.value = type;
                }
            }
            popupContainer.Clear();
            popupContainer.Add(newKeyTypeField);

            // TextField
            newKeyTextField.RegisterCallback<ChangeEvent<string>>((evt) => {
                ValidateButton();
            });

            // Button
            createButton.clicked -= CreateNewKey;
            createButton.clicked += CreateNewKey;

            ValidateButton();
        }

        private string FormatItem(Type arg) {
            if (arg == null) {
                return "(null)";
            } else {
                return arg.Name.Replace("Key", "");
            }
        }

        private void ValidateButton() {
            // Disable the create button if trying to create a non-unique key
            bool isValidKeyText = ValidateKeyText(newKeyTextField.text);
            createButton.SetEnabled(isValidKeyText);
        }

        bool ValidateKeyText(string text) {
            if (text == "") {
                return false;
            }

            BehaviourTree tree = behaviourTree.Blackboard.serializedObject.targetObject as BehaviourTree;
            bool keyExists = tree.blackboard.Find(newKeyTextField.text) != null;
            return !keyExists;
        }

        void CreateNewKey() {
            Type newKeyType = newKeyTypeField.value;
            if (newKeyType != null) {
                behaviourTree.CreateBlackboardKey(newKeyTextField.text, newKeyType);
            }
            ValidateButton();
        }

        public void ClearView() {
            this.behaviourTree = null;
            if (listView != null) {
                listView.Unbind();
            }
        }
    }
}
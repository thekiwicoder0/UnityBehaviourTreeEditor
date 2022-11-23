using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace TheKiwiCoder {
    public class BlackboardView : VisualElement {

        public new class UxmlFactory : UxmlFactory<BlackboardView, VisualElement.UxmlTraits> { }

        private SerializedBehaviourTree behaviourTree;

        private ListView listView;
        private TextField newKeyTextField;
        private EnumField newKeyEnumField;

        private Button createButton;

        internal void Bind(SerializedBehaviourTree behaviourTree) {
            
            this.behaviourTree = behaviourTree;

            listView = this.Q<ListView>("ListView_Keys");
            newKeyTextField = this.Q<TextField>("TextField_KeyName");
            newKeyEnumField = this.Q<EnumField>("EnumField_KeyType");
            createButton = this.Q<Button>("Button_KeyCreate");

            // ListView
            listView.BindProperty(behaviourTree.BlackboardKeys);
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;

            // TextField
            newKeyTextField.RegisterCallback<ChangeEvent<string>>((evt) => {
                ValidateButton();
            });

            // EnumField
            newKeyEnumField.Init(BlackboardKey.Type.Float);

            // Button
            createButton.clicked -= CreateNewKey;
            createButton.clicked += CreateNewKey;

            ValidateButton();
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

        void BindItem(VisualElement item, int index) {
            Label label = item.Q<Label>();
            var blackboardKeys = behaviourTree.BlackboardKeys;
            var keyProp = blackboardKeys.GetArrayElementAtIndex(index);
            var keyName = keyProp.FindPropertyRelative("name");
            label.BindProperty(keyName);

            BlackboardValueField valueField = item.Q<BlackboardValueField>();
            valueField.BindProperty(keyProp);
        }

        VisualElement MakeItem() {

            VisualElement container = new VisualElement();
            container.style.flexGrow = 1.0f;
            container.style.flexDirection = FlexDirection.Row;

            Label keyField = new Label();
            keyField.style.width = 100.0f;

            BlackboardValueField valueField = new BlackboardValueField();
            valueField.style.flexGrow = 1.0f;

            container.Add(keyField);
            container.Add(valueField);

            container.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) => {
                evt.menu.AppendAction($"Delete Key", (a) => {
                    Label label = container.Q<Label>();
                    DeleteKey(label.text);
                });
            }));

            return container;
        }

        void CreateNewKey() {
            behaviourTree.CreateBlackboardKey(newKeyTextField.text, (BlackboardKey.Type)newKeyEnumField.value);
            ValidateButton();
        }

        void DeleteKey(string keyName) {
            behaviourTree.DeleteBlackboardKey(keyName);
        }

        public void ClearView() {
            this.behaviourTree = null;
            if (listView != null) {
                listView.itemsSource = null;
                listView.Rebuild();
            }
        }
    }
}
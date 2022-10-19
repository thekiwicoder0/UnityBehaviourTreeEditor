using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace TheKiwiCoder {
    public class BlackboardView : VisualElement {

        [SerializeField]
        VisualTreeAsset m_ItemAsset;

        public new class UxmlFactory : UxmlFactory<BlackboardView, VisualElement.UxmlTraits> { }

        private SerializedProperty blackboard;

        private ListView listView;
        private TextField newKeyTextField;
        private EnumField newKeyEnumField;

        private Button createButton;

        public BlackboardView() {

        }

        internal void Bind(SerializedProperty blackboard) {

            this.blackboard = blackboard;

            newKeyTextField = this.Q<TextField>("NewKeyTextField");
            newKeyTextField.RegisterCallback<ChangeEvent<string>>((evt) => {
                Validate();
            });

            listView = this.Q<ListView>("ListView");
            newKeyEnumField = this.Q<EnumField>();

            createButton = this.Q<Button>("NewKeyButton");
            createButton.clicked -= CreateNewKey;
            createButton.clicked += CreateNewKey;

            BehaviourTree tree = blackboard.serializedObject.targetObject as BehaviourTree;

            listView.itemsSource = tree.blackboard.items;
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;

            Validate();
        }

        private void Validate() {
            BehaviourTree tree = blackboard.serializedObject.targetObject as BehaviourTree;
            bool keyExists = tree.blackboard.Find(newKeyTextField.text) != null;
            createButton.SetEnabled(!keyExists);
        }

        void BindItem(VisualElement item, int index) {
            Label label = item.Q<Label>();
            var blackboardItems = blackboard.FindPropertyRelative("items");
            var itemProp = blackboardItems.GetArrayElementAtIndex(index);
            var itemKey = itemProp.FindPropertyRelative("key");
            label.BindProperty(itemKey);

            BlackboardValueField valueField = item.Q<BlackboardValueField>();
            valueField.BindProperty(itemProp);
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

            return container;
        }

        void CreateNewKey() {
            SerializedProperty itemsArray = blackboard.FindPropertyRelative("items");
            itemsArray.InsertArrayElementAtIndex(itemsArray.arraySize);
            SerializedProperty newItem = itemsArray.GetArrayElementAtIndex(itemsArray.arraySize - 1);


            BlackboardItem item = new BlackboardItem();
            item.key = newKeyTextField.text;
            item.type = (BlackboardItem.Type)newKeyEnumField.value;
            newItem.managedReferenceValue = item;

            //SerializedProperty keyProp = newItem.FindPropertyRelative("key");
            //SerializedProperty typeProp = newItem.FindPropertyRelative("type");
            //keyProp.stringValue = newKeyTextField.text;
            //typeProp.enumValueIndex = Convert.ToInt32(newKeyEnumField.value);

            newItem.serializedObject.ApplyModifiedProperties();
            listView.Rebuild();
            Validate();
        }
    }
}
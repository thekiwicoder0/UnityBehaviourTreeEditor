using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

namespace TheKiwiCoder {
    /*
    [CustomPropertyDrawer(typeof(Blackboard))]
    public class BlackboardPropertyDrawer : PropertyDrawer {

        BehaviourTree tree;
        MultiColumnListView listView;
        TextField newKeyName;
        EnumField newKeyType;


        public override VisualElement CreatePropertyGUI(SerializedProperty property) {

            tree = property.serializedObject.targetObject as BehaviourTree;

            VisualElement container = new VisualElement();
            container.style.flexGrow = 1.0f;

            container.Add(CreateListView());
            container.Add(CreateSpace());
            container.Add(CreateFooter());
            return container;
        }

        VisualElement CreateListView() {

            listView = new MultiColumnListView();
            listView.reorderMode = ListViewReorderMode.Animated;
            listView.sortingEnabled = true;
            listView.itemsSource = tree.blackboard.items;

            var keys = new Column();
            keys.name = "Key";
            keys.title = "KEYS";
            keys.width = 130;

            var values = new Column();
            values.name = "Value";
            values.title = "VALUES";
            values.width = 200;

            var remove = new Column();
            remove.name = "Remove";
            remove.title = "";
            remove.maxWidth = 40;

            var id = new Column();
            id.name = "Id";
            id.title = "ID";

            listView.columns.Add(keys);
            listView.columns.Add(values);
            listView.columns.Add(remove);
            listView.columns.Add(id);

            listView.columns["Key"].makeCell = () => new Label();
            listView.columns["Value"].makeCell = () => new BlackboardValueField();
            listView.columns["Remove"].makeCell = () => new Button();
            listView.columns["Id"].makeCell = () => new Label();

            listView.columns["Key"].bindCell = (VisualElement element, int index) => (element as Label).text = tree.blackboard.items[index].key;
            listView.columns["Value"].bindCell = (VisualElement element, int index) => (element as BlackboardValueField).value = tree.blackboard.items[index];
            listView.columns["Remove"].bindCell = (VisualElement element, int index) => (element as Button).clicked += () => DeleteKey(index);
            listView.columns["Id"].bindCell = (VisualElement element, int index) => (element as Label).text = tree.blackboard.items[index].id.ToString();


            return listView;
        }

        VisualElement CreateSpace() {
            VisualElement elem = new VisualElement();
            elem.style.flexGrow = 1.0f;
            return elem;
        }

        VisualElement CreateFooter() {
            VisualElement footer = new VisualElement();
            footer.style.flexDirection = FlexDirection.Row;
            footer.style.marginBottom = 20;
            footer.style.borderTopColor = Color.grey;
            footer.style.borderTopWidth = 1.0f;

            newKeyName = new TextField();
            newKeyName.style.flexGrow = 1.0f;
            newKeyName.value = "New Key";
            footer.Add(newKeyName);

            newKeyType = new EnumField(BlackboardItem.Type.Float);
            newKeyType.style.flexGrow = 1.0f;
            footer.Add(newKeyType);

            Button btn = new Button();
            btn.style.flexGrow = 1.0f;
            btn.text = "Add Key";
            btn.clicked += CreateNewKey;
            footer.Add(btn);

            return footer;
        }

        void CreateNewKey() {
            BlackboardItem newKey = new BlackboardItem();
            newKey.key = newKeyName.value;
            newKey.type = (BlackboardItem.Type)newKeyType.value;
            tree.blackboard.items.Add(newKey);

            listView.Rebuild();
        }

        void DeleteKey(int index) {
            tree.blackboard.items.RemoveAt(index);
            listView.Rebuild();
        }
    }*/
}

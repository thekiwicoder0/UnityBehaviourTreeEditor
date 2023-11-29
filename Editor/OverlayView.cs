using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using WebSocketSharp;

namespace TheKiwiCoder {
    public class OverlayView : VisualElement {
        public new class UxmlFactory : UxmlFactory<OverlayView, UxmlTraits> { }

        public Action<BehaviourTree> OnTreeSelected;

        Button createButton;
        VisualElement listViewContainer;
        MultiColumnListView projectListView;

        string NameColumn = "Name";
        string PathColumn = "Path";
        List<string> assetPaths;

        Column CreateColumn(string name) {
            var column = new Column();
            column.name = name;
            column.title = name;
            column.width = 100.0f;
            column.stretchable = true;
            return column;
        }

        MultiColumnListView CreateListView() {

            var listView = new MultiColumnListView();
            listView.showBorder = true;
            listView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            listView.fixedItemHeight = 30.0f;
            listView.showBoundCollectionSize = false;
            listView.showAddRemoveFooter = false;
            listView.reorderable = false;
            listView.itemsSource = assetPaths;

            listView.columns.Add(CreateColumn(NameColumn));
            listView.columns.Add(CreateColumn(PathColumn));

            listView.columns[NameColumn].makeCell = () => new Label();
            listView.columns[PathColumn].makeCell = () => new Label();

            listView.columns[NameColumn].bindCell = BindName;
            listView.columns[PathColumn].bindCell = BindPath;

            return listView;
        }

        private void BindName(VisualElement element, int index) {
            Label label = element as Label;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            var fileName = System.IO.Path.GetFileNameWithoutExtension(assetPaths[index]);
            label.text = fileName;
        }

        private void BindPath(VisualElement element, int index) {
            Label label = element as Label;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.text = assetPaths[index];
        }

        public void Show() {
            // Hidden in UIBuilder while editing..
            style.visibility = Visibility.Visible;

            // Configure fields
            createButton = this.Q<Button>("CreateButton");
            listViewContainer = this.Q<VisualElement>("ListViewContainer");

            // Find all behaviour tree assets
            assetPaths = EditorUtility.GetAssetPaths<BehaviourTree>();
            assetPaths.Sort();

            // Configure create asset button
            createButton.clicked -= OnCreateAsset;
            createButton.clicked += OnCreateAsset;

            projectListView = CreateListView();
            listViewContainer.Clear();
            listViewContainer.Add(projectListView);
            projectListView.selectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(IEnumerable<object> obj) {
            OnOpenAsset();
        }

        public void Hide() {
            style.visibility = Visibility.Hidden;
        }

        public string ToMenuFormat(string one) {
            // Using the slash creates submenus...
            return one.Replace("/", "|");
        }

        public string ToAssetFormat(string one) {
            // Using the slash creates submenus...
            return one.Replace("|", "/");
        }

        void OnOpenAsset() {
            var path = assetPaths[projectListView.selectedIndex];

            BehaviourTree tree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(path);
            if (tree) {
                TreeSelected(tree);
                style.visibility = Visibility.Hidden;
            }
        }

        void OnCreateAsset() {
            var settings = BehaviourTreeEditorWindow.Instance.settings;

            string savePath = UnityEditor.EditorUtility.SaveFilePanel("Create New", settings.newTreePath, "New Behavior Tree", "asset");
            if (savePath.IsNullOrEmpty()) {
                return;
            }

            string name = System.IO.Path.GetFileNameWithoutExtension(savePath);
            string folder = System.IO.Path.GetDirectoryName(savePath);
            folder = folder.Substring(folder.IndexOf("Assets"));

            //System.IO.Directory.CreateDirectory(folder);
            BehaviourTree tree = EditorUtility.CreateNewTree(name, folder);


            if (tree) {
                TreeSelected(tree);
                style.visibility = Visibility.Hidden;
            }
        }

        void TreeSelected(BehaviourTree tree) {
            OnTreeSelected.Invoke(tree);
        }
    }
}

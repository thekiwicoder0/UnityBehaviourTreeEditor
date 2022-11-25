using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;
using System.Linq;

namespace TheKiwiCoder {

    public class BehaviourTreeEditorWindow : EditorWindow {

        [System.Serializable]
        private class PackageManifest {
            public string name;
            public string version;
        }

        private PackageManifest GetPackageManifest() {
            // Loop through all package.json files in the project and find this one.. 
            string[] packageJsons = AssetDatabase.FindAssets("package");
            string[] packagePaths = packageJsons.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            foreach(var path in packagePaths){
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (asset) {
                    PackageManifest manifest = JsonUtility.FromJson<PackageManifest>(asset.text);
                    if (manifest.name == "com.thekiwicoder.behaviourtreeditor") {
                        return manifest;
                    }
                }
            }
            return null;

        }


        public class BehaviourTreeEditorAssetModificationProcessor : UnityEditor.AssetModificationProcessor {

            static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt) {
                if (EditorWindow.HasOpenInstances<BehaviourTreeEditorWindow>()) {
                    BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
                    wnd.ClearIfSelected(path);
                }
                return AssetDeleteResult.DidNotDelete;
            }
        }
        public static BehaviourTreeEditorWindow Instance;
        public BehaviourTreeProjectSettings settings;
        public VisualTreeAsset behaviourTreeXml;
        public VisualTreeAsset nodeXml;
        public StyleSheet behaviourTreeStyle;
        public TextAsset scriptTemplateActionNode;
        public TextAsset scriptTemplateCompositeNode;
        public TextAsset scriptTemplateDecoratorNode;
        
        public BehaviourTreeView treeView;
        public InspectorView inspectorView;
        public BlackboardView blackboardView;
        public OverlayView overlayView;
        public ToolbarMenu toolbarMenu;
        public Label titleLabel;
        public Label versionLabel;

        [HideInInspector]
        public BehaviourTree tree;
        public SerializedBehaviourTree serializer;

        [MenuItem("TheKiwiCoder/BehaviourTreeEditor ...")]
        public static void OpenWindow() {
            BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            wnd.minSize = new Vector2(800, 600);
        }

        public static void OpenWindow(BehaviourTree tree) {
            BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            wnd.minSize = new Vector2(800, 600);
            wnd.SelectTree(tree);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line) {
            if (Selection.activeObject is BehaviourTree) {
                OpenWindow(Selection.activeObject as BehaviourTree);
                return true;
            }
            return false;
        }

        public void CreateGUI() {
            Instance = this;
            settings = BehaviourTreeProjectSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = behaviourTreeXml;
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = behaviourTreeStyle;
            root.styleSheets.Add(styleSheet);

            // Main treeview
            treeView = root.Q<BehaviourTreeView>();
            inspectorView = root.Q<InspectorView>();
            blackboardView = root.Q<BlackboardView>();
            toolbarMenu = root.Q<ToolbarMenu>();
            overlayView = root.Q<OverlayView>("OverlayView");
            titleLabel = root.Q<Label>("TitleLabel");
            versionLabel = root.Q<Label>("Version");

            treeView.styleSheets.Add(behaviourTreeStyle);

            // Toolbar assets menu
            toolbarMenu.RegisterCallback<MouseEnterEvent>((evt) => {

                // Refresh the menu options just before it's opened (on mouse enter)
                toolbarMenu.menu.MenuItems().Clear();
                var behaviourTrees = EditorUtility.GetAssetPaths<BehaviourTree>();
                behaviourTrees.ForEach(path => {
                    var fileName = System.IO.Path.GetFileName(path);
                    toolbarMenu.menu.AppendAction($"{fileName}", (a) => {
                        var tree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(path);
                        SelectTree(tree);
                    });
                });
                toolbarMenu.menu.AppendSeparator();
                toolbarMenu.menu.AppendAction("New Tree...", (a) => OnToolbarNewAsset());
            });

            // Version label
            var packageManifest = GetPackageManifest();
            if (packageManifest != null) {
                versionLabel.text = $"v {packageManifest.version}";
            }

            // Overlay view
            treeView.OnNodeSelected -= OnNodeSelectionChanged;
            treeView.OnNodeSelected += OnNodeSelectionChanged;

            overlayView.OnTreeSelected -= SelectTree;
            overlayView.OnTreeSelected += SelectTree;

            if (serializer == null) {
                overlayView.Show();
            } else {
                SelectTree(serializer.tree);
            }
        }

        void OnUndoRedo() {
            if (tree != null) {
                serializer.serializedObject.Update();
                treeView.PopulateView(serializer);
            }
        }

        private void OnEnable() {
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable() {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj) {
            switch (obj) {
                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.delayCall += OnSelectionChange;
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    EditorApplication.delayCall += OnSelectionChange;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    inspectorView.Clear();
                    break;
            }
        }

        private void OnSelectionChange() {
            if (Selection.activeGameObject) {
                BehaviourTreeRunner runner = Selection.activeGameObject.GetComponent<BehaviourTreeRunner>();
                if (runner) {
                    SelectTree(runner.tree);
                }
            }
        }

        void SelectTree(BehaviourTree newTree) {
            if (!newTree) {
                ClearSelection();
                return;
            }

            tree = newTree;
            serializer = new SerializedBehaviourTree(newTree);

            if (titleLabel != null) {
                string path = AssetDatabase.GetAssetPath(serializer.tree);
                if (path == "") {
                    path = serializer.tree.name;
                }
                titleLabel.text = $"TreeView ({path})";
            }

            overlayView.Hide();
            treeView.PopulateView(serializer);
            blackboardView.Bind(serializer);
        }

        void ClearSelection() {
            tree = null;
            serializer = null;
            inspectorView.Clear();
            treeView.ClearView();
            blackboardView.ClearView();
            overlayView.Show();
        }

        void ClearIfSelected(string path) {
            if (serializer == null) {
                return;
            }

            if (AssetDatabase.GetAssetPath(serializer.tree) == path) {
                // Need to delay because this is called from a will delete asset callback
                EditorApplication.delayCall += () => {
                    SelectTree(null);
                };
            }
        }

        void OnNodeSelectionChanged(NodeView node) {
            inspectorView.UpdateSelection(serializer, node);
        }

        private void OnInspectorUpdate() {
            if (Application.isPlaying) {
                treeView?.UpdateNodeStates();
            }
        }

        void OnToolbarNewAsset() {
            BehaviourTree tree = EditorUtility.CreateNewTree("New Behaviour Tree", "Assets/");
            if (tree) {
                SelectTree(tree);
            }
        }
    }
}
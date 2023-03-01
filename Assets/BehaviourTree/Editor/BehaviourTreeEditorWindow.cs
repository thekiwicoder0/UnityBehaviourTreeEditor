using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;


namespace TheKiwiCoder {

    public class BehaviourTreeEditorWindow : EditorWindow {

        [System.Serializable]
        public class PendingScriptCreate {
            public bool pendingCreate = false;
            public string scriptName = "";
            public string sourceGuid = "";
            public bool isSourceParent = false;
            public Vector2 nodePosition;

            public void Reset() {
                pendingCreate = false;
                scriptName = "";
                sourceGuid = "";
                isSourceParent = false;
                nodePosition = Vector2.zero;
            }
        }

        public class BehaviourTreeEditorAssetModificationProcessor : AssetModificationProcessor {

            static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt) {
                if (HasOpenInstances<BehaviourTreeEditorWindow>()) {
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
        public Label versionLabel;
        public NewScriptDialogView newScriptDialog;
        public ToolbarBreadcrumbs breadcrumbs;

        [SerializeField]
        public PendingScriptCreate pendingScriptCreate = new PendingScriptCreate();

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
            wnd.SelectNewTree(tree);
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
            newScriptDialog = root.Q<NewScriptDialogView>("NewScriptDialogView");
            breadcrumbs = root.Q<ToolbarBreadcrumbs>("breadcrumbs");
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
                        SelectNewTree(tree);
                    });
                });
                toolbarMenu.menu.AppendSeparator();
                toolbarMenu.menu.AppendAction("New Tree...", (a) => OnToolbarNewAsset());
            });

            // Version label
            var packageManifest = EditorUtility.GetPackageManifest();
            if (packageManifest != null) {
                versionLabel.text = $"v {packageManifest.version}";
            }

            treeView.OnNodeSelected -= OnNodeSelectionChanged;
            treeView.OnNodeSelected += OnNodeSelectionChanged;

            // Overlay view
            overlayView.OnTreeSelected -= SelectTree;
            overlayView.OnTreeSelected += SelectTree;

            // New Script Dialog
            newScriptDialog.style.visibility = Visibility.Hidden;

            if (serializer == null) {
                overlayView.Show();
            } else {
                SelectTree(serializer.tree);
            }

            // Create new node for any scripts just created coming back from a compile.
            if (pendingScriptCreate != null && pendingScriptCreate.pendingCreate) {
                CreatePendingScriptNode();
            }
        }

        void CreatePendingScriptNode() {

            // #TODO: Unify this with CreateNodeWindow.CreateNode

            NodeView source = treeView.GetNodeByGuid(pendingScriptCreate.sourceGuid) as NodeView;
            var nodeType = Type.GetType($"{pendingScriptCreate.scriptName}, Assembly-CSharp");
            if (nodeType != null) {
                NodeView createdNode;
                if (source != null) {
                    if (pendingScriptCreate.isSourceParent) {
                        createdNode = treeView.CreateNode(nodeType, pendingScriptCreate.nodePosition, source);
                    } else {
                        createdNode = treeView.CreateNodeWithChild(nodeType, pendingScriptCreate.nodePosition, source);
                    }
                } else {
                    createdNode = treeView.CreateNode(nodeType, pendingScriptCreate.nodePosition, null);
                }

                treeView.SelectNode(createdNode);
            }

            pendingScriptCreate.Reset();
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
                    inspectorView?.Clear();
                    break;
            }
        }

        private void OnSelectionChange() {
            if (Selection.activeGameObject) {
                BehaviourTreeRunner runner = Selection.activeGameObject.GetComponent<BehaviourTreeRunner>();
                if (runner) {
                    SelectNewTree(runner.tree);
                }
            }
        }

        void SelectNewTree(BehaviourTree tree) {
            ClearBreadcrumbs();
            SelectTree(tree);
        }

        void SelectTree(BehaviourTree newTree) {
            
            // If tree view is null the window is probably unfocused
            if (treeView == null) {
                return;
            }

            if (!newTree) {
                ClearSelection();
                return;
            }

            if (newTree != tree) {
                ClearSelection();
            }

            tree = newTree;
            serializer = new SerializedBehaviourTree(newTree);

            int childCount = breadcrumbs.childCount;
            breadcrumbs.PushItem($"{serializer.tree.name}", () => PopToSubtree(childCount, newTree));

            overlayView?.Hide();
            treeView?.PopulateView(serializer);
            blackboardView?.Bind(serializer);
        }

        void ClearSelection() {
            tree = null;
            serializer = null;
            inspectorView?.Clear();
            treeView?.ClearView();
            blackboardView?.ClearView();
            overlayView?.Show();
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
            BehaviourTree tree = EditorUtility.CreateNewTree("New Behaviour Tree", settings.newTreePath);
            if (tree) {
                SelectNewTree(tree);
            }
        }

        public void PushSubTreeView(SubTree subtreeNode) {
            if (subtreeNode.treeAsset != null) {
                if (Application.isPlaying) {
                    SelectTree(subtreeNode.treeInstance);
                } else {
                    SelectTree(subtreeNode.treeAsset);
                }
            } else {
                Debug.LogError("Invalid subtree assigned. Assign a a behaviour tree to the tree asset field");
            }
        }

        public void PopToSubtree(int depth, BehaviourTree tree) {
            while (breadcrumbs.childCount > depth) {
                breadcrumbs.PopItem();
            }
            
            if (tree) {
                SelectTree(tree);
            }
        }

        public void ClearBreadcrumbs() {
            PopToSubtree(0, null);
        }
    }
}
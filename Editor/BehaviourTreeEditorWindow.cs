using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;
using Unity.Profiling;

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

        static readonly ProfilerMarker editorUpdate = new ProfilerMarker("BehaviourTree.EditorUpdate");
        public static BehaviourTreeEditorWindow Instance;
        public BehaviourTreeProjectSettings settings;
        public VisualTreeAsset behaviourTreeXml;
        public VisualTreeAsset nodeXml;
        public StyleSheet behaviourTreeStyle;
        public TextAsset scriptTemplateActionNode;
        public TextAsset scriptTemplateConditionNode;
        public TextAsset scriptTemplateCompositeNode;
        public TextAsset scriptTemplateDecoratorNode;

        public BehaviourTreeView CurrentTreeView {
            get {
                var activeTab = tabView.activeTab as TreeViewTab;
                if (activeTab != null) {
                    return activeTab.treeView;
                }
                return null;
            }
        }
        public BehaviourTree CurrentTree {
            get {
                var activeTab = tabView.activeTab as TreeViewTab;
                if (activeTab != null) {
                    return activeTab.serializer.tree;
                }
                return null;
            }
        }
        public SerializedBehaviourTree CurrentSerializer {
            get {
                var activeTab = tabView.activeTab as TreeViewTab;
                if (activeTab != null) {
                    return activeTab.serializer;
                }
                return null;
            }
        }

        public InspectorView inspectorView;
        public BlackboardView blackboardView;
        public OverlayView overlayView;
        public ToolbarMenu toolbarMenu;
        public Label versionLabel;
        public NewScriptDialogView newScriptDialog;
        public TabView tabView;
        public BehaviourTreeEditorWindowState windowState;

        [SerializeField]
        public PendingScriptCreate pendingScriptCreate = new PendingScriptCreate();


        [MenuItem("Window/AI/BehaviourTree")]
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
            windowState = settings.windowState;

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
            inspectorView = root.Q<InspectorView>();
            blackboardView = root.Q<BlackboardView>();
            toolbarMenu = root.Q<ToolbarMenu>();
            overlayView = root.Q<OverlayView>("OverlayView");
            newScriptDialog = root.Q<NewScriptDialogView>("NewScriptDialogView");
            tabView = root.Q<TabView>();
            tabView.activeTabChanged -= OnTabChanged;
            tabView.activeTabChanged += OnTabChanged;

            versionLabel = root.Q<Label>("Version");

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
                if (EditorApplication.isPlaying) {

                    toolbarMenu.menu.AppendSeparator();

                    var behaviourTreeInstances = Resources.FindObjectsOfTypeAll(typeof(BehaviourTreeInstance));
                    foreach (var instance in behaviourTreeInstances) {
                        BehaviourTreeInstance behaviourTreeInstance = instance as BehaviourTreeInstance;
                        GameObject gameObject = behaviourTreeInstance.gameObject;
                        if (behaviourTreeInstance != null && gameObject.scene != null && gameObject.scene.name != null) {

                            toolbarMenu.menu.AppendAction($"{gameObject.name} [{behaviourTreeInstance.behaviourTree.name}]", (a) => {
                                SelectNewTree(behaviourTreeInstance.RuntimeTree);
                                Selection.activeObject = gameObject;

                            });
                        }
                    }

                }
                toolbarMenu.menu.AppendSeparator();
                toolbarMenu.menu.AppendAction("New Tree...", (a) => OnToolbarNewAsset());
            });

            // Version label
            var packageManifest = EditorUtility.GetPackageManifest();
            if (packageManifest != null) {
                versionLabel.text = $"v {packageManifest.version}";
            }

            // Overlay view
            overlayView.OnTreeSelected -= t => NewTab(t, true);
            overlayView.OnTreeSelected += t => NewTab(t, true);

            // New Script Dialog
            newScriptDialog.style.visibility = Visibility.Hidden;

            // Restore window state between compilations
            windowState.Restore(this);

            // Create new node for any scripts just created coming back from a compile.
            if (pendingScriptCreate != null && pendingScriptCreate.pendingCreate) {
                CreatePendingScriptNode();
            }
        }

        private void OnTabChanged(Tab previous, Tab current) {
            TreeViewTab newTab = current as TreeViewTab;
            inspectorView.Clear();
            blackboardView?.Bind(newTab.serializer);
            windowState.TabChanged(tabView.selectedTabIndex);
        }

        public void OnTabClosed(Tab tab) {
            TreeViewTab treeTab = tab as TreeViewTab;
            windowState.TabClosed(treeTab);
        }

        void CreatePendingScriptNode() {

            // #TODO: Unify this with CreateNodeWindow.CreateNode

            if (CurrentTreeView == null) {
                return;
            }

            NodeView source = CurrentTreeView.GetNodeByGuid(pendingScriptCreate.sourceGuid) as NodeView;
            var nodeType = Type.GetType($"{pendingScriptCreate.scriptName}, Assembly-CSharp");
            if (nodeType != null) {
                NodeView createdNode;
                if (source != null) {
                    if (pendingScriptCreate.isSourceParent) {
                        createdNode = CurrentTreeView.CreateNode(nodeType, pendingScriptCreate.nodePosition, source);
                    } else {
                        createdNode = CurrentTreeView.CreateNodeWithChild(nodeType, pendingScriptCreate.nodePosition, source);
                    }
                } else {
                    createdNode = CurrentTreeView.CreateNode(nodeType, pendingScriptCreate.nodePosition, null);
                }

                CurrentTreeView.SelectNode(createdNode);
                EditorUtility.OpenScriptInEditor(createdNode);
            }

            pendingScriptCreate.Reset();
        }

        void OnUndoRedo() {
            if (CurrentTree != null) {
                CurrentSerializer.serializedObject.Update();
                CurrentTreeView.PopulateView(CurrentSerializer);
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
                    EditorApplication.delayCall += OnExitPlayMode;
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    EditorApplication.delayCall += OnEnterPlayMode;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    inspectorView?.Clear();
                    break;
            }
        }

        void CloseRuntimeTabs() {
            var tabs = tabView.Query<TreeViewTab>().ToList();
            foreach (var tab in tabs) {
                if (tab.isRuntimeTab) {
                    tab.Close();
                }
            }
        }

        void OnEnterPlayMode() {
            OnSelectionChange();
        }

        void OnExitPlayMode() {
            OnSelectionChange();
            CloseRuntimeTabs();
        }

        private void OnSelectionChange() {
            if (Selection.activeGameObject) {
                BehaviourTreeInstance runner = Selection.activeGameObject.GetComponent<BehaviourTreeInstance>();
                if (runner) {
                    SelectNewTree(runner.RuntimeTree);
                }
            }
        }

        void SelectNewTree(BehaviourTree tree) {
            NewTab(tree, true);
        }

        public void NewTab(BehaviourTree newTree, bool focus) {

            // Check for existing tab
            var existingTab = tabView.Q<TreeViewTab>(newTree.name);
            if (existingTab != null) {

                // Focus existing tab
                if (focus) {
                    tabView.activeTab = existingTab;
                }
                return;
            }

            // Otherwise create a new tab
            TreeViewTab newTab = new TreeViewTab(newTree, behaviourTreeStyle);
            tabView.Add(newTab);

            // Record active tab
            windowState.TabOpened(newTab);

            // And Focus
            if (focus) {
                tabView.activeTab = newTab;
                overlayView?.Hide();
            }
        }

        void ClearSelection() {
            inspectorView?.Clear();
            CurrentTreeView?.ClearView();
            blackboardView?.ClearView();
            overlayView?.Show();
        }

        void ClearIfSelected(string path) {
            if (CurrentSerializer == null) {
                return;
            }

            if (AssetDatabase.GetAssetPath(CurrentSerializer.tree) == path) {
                // Need to delay because this is called from a will delete asset callback
                EditorApplication.delayCall += () => {
                    NewTab(null, true);
                };
            }
        }

        private void OnInspectorUpdate() {
            if (Application.isPlaying) {
                editorUpdate.Begin();
                CurrentTreeView?.UpdateNodeStates();
                editorUpdate.End();
            }
        }

        void OnToolbarNewAsset() {
            BehaviourTree tree = EditorUtility.CreateNewTree();
            if (tree) {
                SelectNewTree(tree);
            }
        }

        public void InspectNode(SerializedBehaviourTree serializer, NodeView node) {
            inspectorView.UpdateSelection(serializer, node);
        }
    }
}
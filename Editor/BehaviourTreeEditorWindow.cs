using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTreeBuilder
{
    public class BehaviourTreeEditorWindow : EditorWindow
    {
        public static BehaviourTreeEditorWindow Instance;
        public BehaviourTreeProjectSettings settings;
        public VisualTreeAsset behaviourTreeXml;
        public VisualTreeAsset nodeXml;
        public StyleSheet behaviourTreeStyle;
        public TextAsset scriptTemplateActionNode;
        public TextAsset scriptTemplateCompositeNode;
        public TextAsset scriptTemplateDecoratorNode;

        [SerializeField] public PendingScriptCreate pendingScriptCreate = new();

        [HideInInspector] public BehaviourTree tree;

        public SerializedBehaviourTree serializer;
        public BlackboardView blackboardView;
        public ToolbarBreadcrumbs breadcrumbs;
        public InspectorView inspectorView;
        public OverlayView overlayView;
        public ToolbarMenu toolbarMenu;

        public BehaviourTreeView treeView;
        public Label versionLabel;
        public NewScriptWindow newScriptWindow;

        private void Update()
        {
            treeView?.UpdateNodeStates();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            
            if(newScriptWindow == null) newScriptWindow = CreateInstance<NewScriptWindow>();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        public void CreateGUI()
        {
            Instance = this;
            settings = BehaviourTreeProjectSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

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
            breadcrumbs = root.Q<ToolbarBreadcrumbs>("breadcrumbs");
            versionLabel = root.Q<Label>("Version");

            treeView.styleSheets.Add(behaviourTreeStyle);

            // Toolbar assets menu
            toolbarMenu.RegisterCallback<MouseEnterEvent>(evt =>
            {
                // Refresh the menu options just before it's opened (on mouse enter)
                toolbarMenu.menu.MenuItems().Clear();
                var behaviourTrees = EditorUtility.GetAssetPaths<BehaviourTree>();
                behaviourTrees.ForEach(path =>
                {
                    var fileName = Path.GetFileName(path);
                    toolbarMenu.menu.AppendAction($"{fileName}", a =>
                    {
                        var tree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(path);
                        SelectNewTree(tree);
                    });
                });
                toolbarMenu.menu.AppendSeparator();
                toolbarMenu.menu.AppendAction("New Tree", a => OnToolbarNewAsset());
            });

            // Version label
            var packageManifest = EditorUtility.GetPackageManifest();
            if (packageManifest != null) versionLabel.text = $"v {packageManifest.version}";

            treeView.OnNodeSelected -= OnNodeSelectionChanged;
            treeView.OnNodeSelected += OnNodeSelectionChanged;

            // Overlay view
            overlayView.OnTreeSelected -= SelectTree;
            overlayView.OnTreeSelected += SelectTree;

            if (serializer == null)
                overlayView.Show();
            else
                SelectTree(serializer.tree);

            // Create new node for any scripts just created coming back from a compile.
            if (pendingScriptCreate != null && pendingScriptCreate.pendingCreate) CreatePendingScriptNode();
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject)
            {
                var runner = Selection.activeGameObject.GetComponent<BehaviourTreeInstance>();
                if (runner) SelectNewTree(runner.RuntimeTree);
            }
        }

        [MenuItem("Behaviour Tree/Editor", false, 2)]
        public static void OpenWindow()
        {
            var wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            wnd.minSize = new Vector2(800, 600);
        }

        public static void OpenWindow(BehaviourTree tree)
        {
            var wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            wnd.minSize = new Vector2(800, 600);
            wnd.SelectNewTree(tree);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviourTree)
            {
                OpenWindow(Selection.activeObject as BehaviourTree);
                return true;
            }

            return false;
        }

        private void CreatePendingScriptNode()
        {
            // #TODO: Unify this with CreateNodeWindow.CreateNode

            var source = treeView.GetNodeByGuid(pendingScriptCreate.sourceGuid) as NodeView;
            var nodeType = Type.GetType($"{settings.namespaceScriptNode}.{pendingScriptCreate.scriptName}, Assembly-CSharp");
            if (nodeType != null)
            {
                NodeView createdNode;
                if (source != null)
                {
                    if (pendingScriptCreate.isSourceParent)
                        createdNode = treeView.CreateNode(nodeType, pendingScriptCreate.nodePosition, source);
                    else
                        createdNode = treeView.CreateNodeWithChild(nodeType, pendingScriptCreate.nodePosition, source);
                }
                else
                {
                    createdNode = treeView.CreateNode(nodeType, pendingScriptCreate.nodePosition, null);
                }

                treeView.SelectNode(createdNode);
            }

            pendingScriptCreate.Reset();
        }

        private void OnUndoRedo()
        {
            if (tree != null)
            {
                serializer.serializedObject.Update();
                treeView.PopulateView(serializer);
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
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

        private void SelectNewTree(BehaviourTree tree)
        {
            ClearBreadcrumbs();
            SelectTree(tree);
        }

        private void SelectTree(BehaviourTree newTree)
        {
            // If tree view is null the window is probably unfocused
            if (treeView == null) return;

            if (!newTree)
            {
                ClearSelection();
                return;
            }

            if (newTree != tree) ClearSelection();

            tree = newTree;
            serializer = new SerializedBehaviourTree(newTree);

            var childCount = breadcrumbs.childCount;
            breadcrumbs.PushItem($"{serializer.tree.name}", () => PopToSubtree(childCount, newTree));

            overlayView?.Hide();
            treeView?.PopulateView(serializer);
            blackboardView?.Bind(serializer);
        }

        private void ClearSelection()
        {
            tree = null;
            serializer = null;
            inspectorView?.Clear();
            treeView?.ClearView();
            blackboardView?.ClearView();
            overlayView?.Show();
        }

        private void ClearIfSelected(string path)
        {
            if (serializer == null) return;

            if (AssetDatabase.GetAssetPath(serializer.tree) == path)
                // Need to delay because this is called from a will delete asset callback
                EditorApplication.delayCall += () => { SelectTree(null); };
        }

        private void OnNodeSelectionChanged(NodeView node)
        {
            inspectorView.UpdateSelection(serializer, node);
        }

        private void OnToolbarNewAsset()
        {
            var tree = EditorUtility.CreateNewTree("New Behaviour Tree", settings.newTreePath);
            if (tree) SelectNewTree(tree);
        }

        public void PushSubTreeView(SubTree subtreeNode)
        {
            if (subtreeNode.TreeAsset != null)
            {
                if (Application.isPlaying)
                    SelectTree(subtreeNode.TreeInstance);
                else
                    SelectTree(subtreeNode.TreeAsset);
            }
            else
            {
                Debug.LogError("Invalid subtree assigned. Assign a a behaviour tree to the tree asset field");
            }
        }

        public void PopToSubtree(int depth, BehaviourTree tree)
        {
            while (breadcrumbs != null && breadcrumbs.childCount > depth) breadcrumbs.PopItem();

            if (tree) SelectTree(tree);
        }

        public void ClearBreadcrumbs()
        {
            PopToSubtree(0, null);
        }

        [Serializable]
        public class PendingScriptCreate
        {
            public bool pendingCreate;
            public string scriptName = "";
            public string sourceGuid = "";
            public bool isSourceParent;
            public Vector2 nodePosition;

            public void Reset()
            {
                pendingCreate = false;
                scriptName = "";
                sourceGuid = "";
                isSourceParent = false;
                nodePosition = Vector2.zero;
            }
        }

        public class BehaviourTreeEditorAssetModificationProcessor : AssetModificationProcessor
        {
            private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
            {
                if (HasOpenInstances<BehaviourTreeEditorWindow>())
                {
                    var wnd = GetWindow<BehaviourTreeEditorWindow>();
                    wnd.ClearIfSelected(path);
                }

                return AssetDeleteResult.DidNotDelete;
            }
        }
    }
}
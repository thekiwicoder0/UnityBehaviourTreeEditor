using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace TheKiwiCoder {

    public class CreateNodeWindow : ScriptableObject, ISearchWindowProvider {

        Texture2D icon;
        BehaviourTreeView treeView;
        NodeView source;
        bool isSourceParent;
        EditorUtility.ScriptTemplate[] scriptFileAssets;

        TextAsset GetScriptTemplate(int type) {
            var projectSettings = BehaviourTreeProjectSettings.GetOrCreateSettings();

            switch (type) {
                case 0:
                    if (projectSettings.scriptTemplateActionNode) {
                        return projectSettings.scriptTemplateActionNode;
                    }
                    return BehaviourTreeEditorWindow.Instance.scriptTemplateActionNode;
                case 1:
                    if (projectSettings.scriptTemplateConditionNode) {
                        return projectSettings.scriptTemplateConditionNode;
                    }
                    return BehaviourTreeEditorWindow.Instance.scriptTemplateConditionNode;
                case 2:
                    if (projectSettings.scriptTemplateCompositeNode) {
                        return projectSettings.scriptTemplateCompositeNode;
                    }
                    return BehaviourTreeEditorWindow.Instance.scriptTemplateCompositeNode;
                case 3:
                    if (projectSettings.scriptTemplateDecoratorNode) {
                        return projectSettings.scriptTemplateDecoratorNode;
                    }
                    return BehaviourTreeEditorWindow.Instance.scriptTemplateDecoratorNode;

            }
            Debug.LogError("Unhandled script template type:" + type);
            return null;
        }

        public void Initialise(BehaviourTreeView treeView, NodeView source, bool isSourceParent) {
            this.treeView = treeView;
            this.source = source;
            this.isSourceParent = isSourceParent;

            icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            icon.Apply();

            scriptFileAssets = new EditorUtility.ScriptTemplate[] {
                new EditorUtility.ScriptTemplate { templateFile = GetScriptTemplate(0), defaultFileName = "NewActionNode", subFolder = "Actions" },
                new EditorUtility.ScriptTemplate { templateFile = GetScriptTemplate(1), defaultFileName = "NewConditionNode", subFolder = "Conditions" },
                new EditorUtility.ScriptTemplate { templateFile = GetScriptTemplate(2), defaultFileName = "NewCompositeNode", subFolder = "Composites" },
                new EditorUtility.ScriptTemplate { templateFile = GetScriptTemplate(3), defaultFileName = "NewDecoratorNode", subFolder = "Decorators" },
            };
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {

            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };

            // Action nodes can only be added as children
            if (isSourceParent || source == null) {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Actions")) { level = 1 });
                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types) {

                    // Ignore condition types
                    if (!type.IsSubclassOf(typeof(ConditionNode))) {
                        System.Action invoke = () => CreateNode(type, context);
                        tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) { level = 2, userData = invoke });
                    }
                }
            }

            // Condition nodes can only be added as children
            if (isSourceParent || source == null) {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Conditions")) { level = 1 });
                var types = TypeCache.GetTypesDerivedFrom<ConditionNode>();
                foreach (var type in types) {
                    System.Action invoke = () => CreateNode(type, context);
                    tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) { level = 2, userData = invoke });
                }
            }

            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Composites")) { level = 1 });
                {
                    var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
                    foreach (var type in types) {
                        System.Action invoke = () => CreateNode(type, context);
                        tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) { level = 2, userData = invoke });
                    }
                }
            }

            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Decorators")) { level = 1 });
                {
                    var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                    foreach (var type in types) {
                        System.Action invoke = () => CreateNode(type, context);
                        tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) { level = 2, userData = invoke });
                    }
                }
            }

            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Subtrees")) { level = 1 });
                {
                    var behaviourTrees = EditorUtility.GetAssetPaths<BehaviourTree>();
                    behaviourTrees.ForEach(path => {
                        var fileName = System.IO.Path.GetFileName(path);

                        System.Action invoke = () => {
                            var tree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(path);
                            var subTreeNodeView = CreateNode(typeof(SubTree), context);
                            SubTree subtreeNode = subTreeNodeView.node as SubTree;
                            subtreeNode.treeAsset = tree;
                        };
                        tree.Add(new SearchTreeEntry(new GUIContent($"{fileName}")) { level = 2, userData = invoke });
                    });
                }
            }

            {

                tree.Add(new SearchTreeGroupEntry(new GUIContent("New Script ...")) { level = 1 });

                System.Action createActionScript = () => CreateScript(scriptFileAssets[0], context);
                tree.Add(new SearchTreeEntry(new GUIContent($"New Action Script")) { level = 2, userData = createActionScript });

                System.Action createConditionScript = () => CreateScript(scriptFileAssets[1], context);
                tree.Add(new SearchTreeEntry(new GUIContent($"New Condition Script")) { level = 2, userData = createConditionScript });

                System.Action createCompositeScript = () => CreateScript(scriptFileAssets[2], context);
                tree.Add(new SearchTreeEntry(new GUIContent($"New Composite Script")) { level = 2, userData = createCompositeScript });

                System.Action createDecoratorScript = () => CreateScript(scriptFileAssets[3], context);
                tree.Add(new SearchTreeEntry(new GUIContent($"New Decorator Script")) { level = 2, userData = createDecoratorScript });
            }

            {
                System.Action invoke = () => {
                    var newTree = EditorUtility.CreateNewTree();
                    if (newTree) {
                        var subTreeNodeView = CreateNode(typeof(SubTree), context);
                        SubTree subtreeNode = subTreeNodeView.node as SubTree;
                        subtreeNode.treeAsset = newTree;
                    }
                };
                tree.Add(new SearchTreeEntry(new GUIContent("     New Subtree ...")) { level = 1, userData = invoke });
            }


            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            System.Action invoke = (System.Action)searchTreeEntry.userData;
            invoke();
            return true;
        }

        public NodeView CreateNode(System.Type type, SearchWindowContext context) {
            BehaviourTreeEditorWindow editorWindow = BehaviourTreeEditorWindow.Instance;

            var windowMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
            var graphMousePosition = editorWindow.CurrentTreeView.contentViewContainer.WorldToLocal(windowMousePosition);
            var nodeOffset = new Vector2(-75, -20);
            var nodePosition = graphMousePosition + nodeOffset;

            nodePosition.x = EditorUtility.SnapTo(nodePosition.x, editorWindow.settings.gridSnapSizeX);
            nodePosition.y = EditorUtility.SnapTo(nodePosition.y, editorWindow.settings.gridSnapSizeY);

            // #TODO: Unify this with CreatePendingScriptNode
            NodeView createdNode;
            if (source != null) {
                if (isSourceParent) {
                    createdNode = treeView.CreateNode(type, nodePosition, source);
                } else {
                    createdNode = treeView.CreateNodeWithChild(type, nodePosition, source);
                }
            } else {
                createdNode = treeView.CreateNode(type, nodePosition, null);
            }

            treeView.SelectNode(createdNode);
            return createdNode;
        }

        public void CreateScript(EditorUtility.ScriptTemplate scriptTemplate, SearchWindowContext context) {
            BehaviourTreeEditorWindow editorWindow = BehaviourTreeEditorWindow.Instance;

            var windowMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
            var graphMousePosition = editorWindow.CurrentTreeView.contentViewContainer.WorldToLocal(windowMousePosition);
            var nodeOffset = new Vector2(-75, -20);
            var nodePosition = graphMousePosition + nodeOffset;

            EditorUtility.CreateNewScript(scriptTemplate, source, isSourceParent, nodePosition);
        }

        public static void Show(Vector2 mousePosition, NodeView source, bool isSourceParent = false) {
            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mousePosition);
            CreateNodeWindow searchWindowProvider = ScriptableObject.CreateInstance<CreateNodeWindow>();
            searchWindowProvider.Initialise(BehaviourTreeEditorWindow.Instance.CurrentTreeView, source, isSourceParent);
            SearchWindowContext windowContext = new SearchWindowContext(screenPoint, 240, 320);
            SearchWindow.Open(windowContext, searchWindowProvider);
        }
    }
}

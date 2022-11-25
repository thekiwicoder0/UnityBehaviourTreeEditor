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
        BehaviourTreeView.ScriptTemplate[] scriptFileAssets;

        public void Initialise(BehaviourTreeView treeView, NodeView source, bool isSourceParent) {
            this.treeView = treeView;
            this.source = source;
            this.isSourceParent = isSourceParent;

            icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            icon.Apply();

            scriptFileAssets = new BehaviourTreeView.ScriptTemplate[] {
                new BehaviourTreeView.ScriptTemplate { templateFile = BehaviourTreeEditorWindow.Instance.scriptTemplateActionNode, defaultFileName = "NewActionNode.cs", subFolder = "Actions" },
                new BehaviourTreeView.ScriptTemplate { templateFile = BehaviourTreeEditorWindow.Instance.scriptTemplateCompositeNode, defaultFileName = "NewCompositeNode.cs", subFolder = "Composites" },
                new BehaviourTreeView.ScriptTemplate { templateFile = BehaviourTreeEditorWindow.Instance.scriptTemplateDecoratorNode, defaultFileName = "NewDecoratorNode.cs", subFolder = "Decorators" },
            };
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {

            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };

            // Action nodes can only be added as children
            if (isSourceParent || source == null)
            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("Actions")) { level = 1 });
                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types) {
                    System.Action invoke = () => CreateNode(type, context);
                    tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) {level = 2,userData = invoke });
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
                        tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) {level = 2, userData = invoke});
                    }
                }
            }

            {
                tree.Add(new SearchTreeGroupEntry(new GUIContent("New Script...")) { level = 1 });

                System.Action createActionScript = () => CreateScript(scriptFileAssets[0], context);
                tree.Add(new SearchTreeEntry(new GUIContent($"New Action Script")) { level = 2, userData = createActionScript });

                System.Action createCompositeScript = () => CreateScript(scriptFileAssets[1], context);
                tree.Add(new SearchTreeEntry(new GUIContent($"New Composite Script")) { level = 2, userData = createCompositeScript });

                System.Action createDecoratorScript = () => CreateScript(scriptFileAssets[2], context);
                tree.Add(new SearchTreeEntry(new GUIContent($"New Decorator Script")) { level = 2, userData = createDecoratorScript });
            }


            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            System.Action invoke = (System.Action)searchTreeEntry.userData;
            invoke();
            return true;
        }

        public void CreateNode(System.Type type, SearchWindowContext context) {
            BehaviourTreeEditorWindow editorWindow = BehaviourTreeEditorWindow.Instance;
            
            var windowMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
            var graphMousePosition = editorWindow.treeView.contentViewContainer.WorldToLocal(windowMousePosition);
            var nodeOffset = new Vector2(-75, -20);
            var nodePosition = graphMousePosition + nodeOffset;

            if (source != null) {
                if (isSourceParent) {
                    treeView.CreateNode(type, nodePosition, source);
                } else {
                    treeView.CreateNodeWithChild(type, nodePosition, source);
                }
            } else {
                treeView.CreateNode(type, nodePosition, null);
            }
        }

        public void CreateScript(BehaviourTreeView.ScriptTemplate scriptTemplate, SearchWindowContext context) {
            BehaviourTreeEditorWindow editorWindow = BehaviourTreeEditorWindow.Instance;
            editorWindow.treeView.CreateNewScript(scriptTemplate);
        }

        public static void Show(Vector2 mousePosition, NodeView source, bool isSourceParent = false) {
            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mousePosition);
            CreateNodeWindow searchWindowProvider = ScriptableObject.CreateInstance<CreateNodeWindow>();
            searchWindowProvider.Initialise(BehaviourTreeEditorWindow.Instance.treeView, source, isSourceParent);
            SearchWindowContext windowContext = new SearchWindowContext(screenPoint, 240, 320);
            SearchWindow.Open(windowContext, searchWindowProvider);
        }
    }
}

using System;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviourTreeBuilder
{
    public class NewScriptWindow : OdinEditorWindow
    {
        public enum NodeType
        {
            Action,
            Composite,
            Decorator
        }

        private Vector2 _nodePosition;
        private NodeView _source;
        private bool _isSourceParent;
        private bool _inEditor;
        private BehaviourTreeProjectSettings _setting;
        
        [SerializeField] [Required] private string _scriptName;
        [SerializeField] [EnumToggleButtons] private NodeType _nodeType;
        private string _scriptPath;

        private void OnBecameVisible()
        {
            if (_setting == null) _setting = BehaviourTreeProjectSettings.GetOrCreateSettings();
        }

        [MenuItem("Behaviour Tree/Create New Node Script", false, 3)]
        public static void OpenWindow()
        {
            var window = GetWindow<NewScriptWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(400, 110);
            window.maximized = false;
            window.maxSize = new(400, 110);
            window.minSize = new(400, 110);
            window.ShowModal();
        }

        [Button(ButtonSizes.Large)]
        [GUIColor(0, 1f, 0)]
        private void CreateNewScript()
        {
            if(String.IsNullOrEmpty(_scriptName)) return;
            var scriptName = _scriptName;
            var nameSpace = _setting.namespaceScriptNode;
            if (String.IsNullOrEmpty(nameSpace))
            {
               bool openSetting = UnityEditor.EditorUtility.DisplayDialog("Error!!!",
                    $"namespace for script is required", "Open Setting", "Cancel");
               if (openSetting) BehaviourTreeSettingWindow.OpenWindow();
                return;
            }
            var template = new EditorUtility.ScriptTemplate();
            switch (_nodeType)
            {
                case NodeType.Action:
                    template.TemplateFile = GetScriptTemplate(0);
                    template.SubFolder = "Actions";

                    break;
                case NodeType.Composite:
                    template.TemplateFile = GetScriptTemplate(1);
                    template.SubFolder = "Composite";
                    break;
                case NodeType.Decorator:
                    template.TemplateFile = GetScriptTemplate(2);
                    template.SubFolder = "Decorator";
                    break;
            }

            var newNodePath = $"{_setting.newNodePath}";
            if (AssetDatabase.IsValidFolder(newNodePath))
            {
                var path = $"{_setting.newNodePath}/{template.SubFolder}";
                if (path[path.Length - 1] == '/')
                    path = path.Substring(0, path.Length - 1);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                var templateString = template.TemplateFile.text;
                templateString = templateString.Replace("#SCRIPTNAME#", scriptName).Replace("#NAMESPACE#", nameSpace);
                _scriptPath = $"{path}/{scriptName}.cs";

                if (!File.Exists(_scriptPath))
                {
                    File.WriteAllText(_scriptPath, templateString);

                    // TODO: There must be a better way to survive domain reloads after script compiling than this
                    if (_inEditor)
                    {
                        BehaviourTreeEditorWindow.Instance.pendingScriptCreate.pendingCreate = true;
                        BehaviourTreeEditorWindow.Instance.pendingScriptCreate.scriptName = scriptName;
                        BehaviourTreeEditorWindow.Instance.pendingScriptCreate.nodePosition = _nodePosition;
                        if (_source != null)
                        {
                            BehaviourTreeEditorWindow.Instance.pendingScriptCreate.sourceGuid = _source.node.guid;
                            BehaviourTreeEditorWindow.Instance.pendingScriptCreate.isSourceParent = _isSourceParent;
                        }
                    }
                    AssetDatabase.Refresh();
                    // Also flash the folder yellow to highlight it
                    var obj = AssetDatabase.LoadAssetAtPath(_scriptPath, typeof(Object));
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                    Close();
                    // EditorApplication.delayCall += DelayCall;
                }
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("Error!!!",
                        $"Script with that name already exists:{_scriptPath}", "OK");
                    // Close();
                }
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog("Error!!!",
                    $"Invalid folder path:{newNodePath}. Check the project configuration settings 'newNodePath' is configured to a valid folder",
                    "OK");
            }
        }

        private void DelayCall()
        {
            var obj = AssetDatabase.LoadAssetAtPath(_scriptPath, typeof(Object));
            Selection.activeObject = obj;
            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }

        public void CreateScript(NodeView source, bool isSourceParent, Vector2 nodePosition)
        {
            _source = source;
            _isSourceParent = isSourceParent;
            _nodePosition = nodePosition;
            _inEditor = true;
            OpenWindow();
        }
        
        private TextAsset GetScriptTemplate(int type)
        {
            var projectSettings = BehaviourTreeProjectSettings.GetOrCreateSettings();

            switch (type)
            {
                case 0:
                    if (projectSettings.scriptTemplateActionNode) return projectSettings.scriptTemplateActionNode;
                    return BehaviourTreeEditorWindow.Instance.scriptTemplateActionNode;
                case 1:
                    if (projectSettings.scriptTemplateCompositeNode) return projectSettings.scriptTemplateCompositeNode;
                    return BehaviourTreeEditorWindow.Instance.scriptTemplateCompositeNode;
                case 2:
                    if (projectSettings.scriptTemplateDecoratorNode) return projectSettings.scriptTemplateDecoratorNode;
                    return BehaviourTreeEditorWindow.Instance.scriptTemplateDecoratorNode;
            }

            Debug.LogError("Unhandled script template type:" + type);
            return null;
        }
    }
}
using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace BehaviourTreeBuilder
{
    public class CreateTreeWindow: OdinEditorWindow
    {
        [SerializeField, Required] private string _treeName;
        [SerializeField, FolderPath, Required] private string _path;

        private void OnBecameVisible()
        {
            _path = BehaviourTreeProjectSettings.GetOrCreateSettings().newTreePath;
        }

        [MenuItem("Behaviour Tree/Create New Tree",false,3)]
        private static void OpenWindow()
        {
            var window = GetWindow<CreateTreeWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(400, 130);
            window.maximized = false;
            window.maxSize = new(400, 130);
            window.minSize = new(400, 130);
            window.ShowModal();
        }

        [Button(ButtonSizes.Large)]
        [GUIColor(0, 1f, 0)]
        private void CreateTree()
        {
            if(String.IsNullOrEmpty(_treeName)) return;
            var tree = EditorUtility.CreateNewTree(_treeName, _path);
            Close();
            BehaviourTreeEditorWindow.OpenWindow(tree);
        }
    }
}
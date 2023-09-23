using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourTreeBuilder
{
    // Create a new type of Settings Asset.
    public class BehaviourTreeProjectSettings : ScriptableObject
    {
        [Required]
        public string namespaceScriptNode;
        [Tooltip("Folder where new tree assets will be created")]
        [FolderPath, Required]
        public string newTreePath = "Assets";

        [Tooltip("Folder where new node scripts will be created")]
        [FolderPath, Required]
        public string newNodePath = "Assets";
        
        [Header("Custom Script Template")]
        [Tooltip("Script template to use when creating action nodes")]
        public TextAsset scriptTemplateActionNode;

        [Tooltip("Script template to use when creating composite nodes")]
        public TextAsset scriptTemplateCompositeNode;

        [Tooltip("Script template to use when creating decorator nodes")]
        public TextAsset scriptTemplateDecoratorNode;

        [Header("Node view Color")]
        public Color RootNodeColor = new Color(1f, 0.231f, 0.231f, 1f);
        public Color ActionNodeColor = new Color(0.383f, 1f, 0f, 1f);
        public Color CompositeNodeColor = new Color(1f, 0.937f, 0.137f, 1f);
        public Color DecoratorNodeColor = new Color(0f, 0.572f, 1f, 1f);

        [Header("Define Settings")]
        [SerializeField] private bool _core3D;
        [SerializeField] private bool _navMesh;
        [SerializeField] private bool _characterController;
        [SerializeField] private BuildTargetGroup[] _buildTargetGroups;

        private static BehaviourTreeProjectSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(BehaviourTreeProjectSettings)}");
            if (guids.Length > 1) Debug.LogWarning("Found multiple settings files, using the first.");

            switch (guids.Length)
            {
                case 0:
                    return null;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<BehaviourTreeProjectSettings>(path);
            }
        }

        internal static BehaviourTreeProjectSettings GetOrCreateSettings()
        {
            var settings = FindSettings();
            if (settings == null)
            {
                settings = CreateInstance<BehaviourTreeProjectSettings>();
                string path = "Assets/Behaviour Tree";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                AssetDatabase.CreateAsset(settings, $"{path}/BehaviourTreeProjectSettings.asset");
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
        
        [Button(ButtonSizes.Large)]
        private void BuildDefine()
        {
            if (_core3D)  EditorUtility.AddCompileDefine("CORE_3D", _buildTargetGroups);
            else  EditorUtility.RemoveCompileDefine("CORE_3D", _buildTargetGroups);
            
            if (_navMesh) EditorUtility.AddCompileDefine("USE_NAVMESH", _buildTargetGroups);
            else EditorUtility.RemoveCompileDefine("USE_NAVMESH", _buildTargetGroups);
            
            if(_characterController) EditorUtility.AddCompileDefine("USE_CHARACTER_CONTROLLER", _buildTargetGroups);
            else EditorUtility.RemoveCompileDefine("USE_CHARACTER_CONTROLLER", _buildTargetGroups);
            
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }
}
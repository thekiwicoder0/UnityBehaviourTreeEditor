using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace TheKiwiCoder
{
    public static class EditorUtility
    {
        public static BehaviourTree CreateNewTree(string assetName, string folder) {

            if(!AssetDatabase.IsValidFolder(folder)){
                Debug.LogError($"Failed to create behaviour tree asset: Invalid folder path:{folder}");
                return null;
            }

            string path = System.IO.Path.Join(folder, $"{assetName}.asset");
            if (System.IO.File.Exists(path)) {
                Debug.LogError($"Failed to create behaviour tree asset: Path already exists:{assetName}");
                return null;
            }
            BehaviourTree tree = ScriptableObject.CreateInstance<BehaviourTree>();
            tree.name = assetName;
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(tree);
            return tree;
        }

        public static List<T> LoadAssets<T>() where T : UnityEngine.Object {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            List<T> assets = new List<T>();
            foreach (var assetId in assetIds) {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }
            return assets;
        }

        public static List<string> GetAssetPaths<T>() where T : UnityEngine.Object {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            List<string> paths = new List<string>();
            foreach (var assetId in assetIds) {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                paths.Add(path);
            }
            return paths;
        }
    }
}

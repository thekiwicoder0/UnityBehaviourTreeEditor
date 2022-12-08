using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Search;
using System.Linq;

namespace TheKiwiCoder
{
    public static class EditorUtility
    {
        public struct ScriptTemplate {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }

        [System.Serializable]
        public class PackageManifest {
            public string name;
            public string version;
        }


        public static BehaviourTree CreateNewTree(string assetName, string folder) {

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

        public static void CreateNewScript(ScriptTemplate scriptTemplate, NodeView source, bool isSourceParent, Vector2 position) {
            BehaviourTreeEditorWindow.Instance.newScriptDialog.CreateScript(scriptTemplate, source, isSourceParent, position);
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

        public static PackageManifest GetPackageManifest() {
            // Loop through all package.json files in the project and find this one.. 
            string[] packageJsons = AssetDatabase.FindAssets("package");
            string[] packagePaths = packageJsons.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            foreach (var path in packagePaths) {
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

    }
}

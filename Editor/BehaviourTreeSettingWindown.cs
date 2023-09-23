using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;

namespace BehaviourTreeBuilder
{
    public class BehaviourTreeSettingWindow : OdinEditorWindow
    {

        [MenuItem("Behaviour Tree/Setting")]
        public static void OpenWindow()
        {
            var window = InspectObject(BehaviourTreeProjectSettings.GetOrCreateSettings());
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 500);
            window.maximized = false;
            window.maxSize = new(600, 700);
            window.minSize = new(600, 400);
        }
    }
}
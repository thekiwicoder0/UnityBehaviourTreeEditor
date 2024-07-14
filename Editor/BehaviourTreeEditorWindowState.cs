using System.Collections.Generic;

namespace TheKiwiCoder {

    [System.Serializable]
    public class BehaviourTreeEditorWindowState {

        public List<BehaviourTree> activeTabs = new List<BehaviourTree>();
        public int activeTabIndex = -1;

        public void Restore(BehaviourTreeEditorWindow window) {
            if (activeTabs.Count == 0) {
                window.overlayView.Show();
            } else {
                int previousActiveTabIndex = activeTabIndex;
                foreach (var tree in activeTabs) {
                    window.NewTab(tree, true);
                }
                window.tabView.selectedTabIndex = previousActiveTabIndex;
            }
        }

        public void TabOpened(TreeViewTab newTab) {
            if (!newTab.isRuntimeTab) {
                if (!activeTabs.Contains(newTab.serializer.tree)) {
                    activeTabs.Add(newTab.serializer.tree);
                    Save();
                }
            }
        }

        public void TabChanged(int tabIndex) {
            activeTabIndex = tabIndex;
            Save();
        }

        public void TabClosed(TreeViewTab tab) {
            activeTabs.Remove(tab.serializer.tree);
            Save();
        }

        void Save() {
            BehaviourTreeEditorWindow window = BehaviourTreeEditorWindow.Instance;
            BehaviourTreeProjectSettings settings = window.settings;
            UnityEditor.EditorUtility.SetDirty(settings);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using TheKiwiCoder;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

public class TreeViewTab : Tab {

    public SerializedBehaviourTree serializer;
    public BehaviourTreeView treeView;

    public TreeViewTab(BehaviourTree tree, StyleSheet styleSheet) : base(tree.name) {
        name = tree.name;
        closeable = true;
        serializer = new SerializedBehaviourTree(tree);

        treeView = new BehaviourTreeView();
        treeView.styleSheets.Add(styleSheet);
        treeView.PopulateView(serializer);
        treeView.OnNodeSelected += OnNodeSelected;

        // TODO: Remove selection when closing the tab

        Add(treeView);
    }

    void OnNodeSelected(NodeView node) {
        BehaviourTreeEditorWindow window = BehaviourTreeEditorWindow.Instance;
        window.InspectNode(serializer, node);
    }
}

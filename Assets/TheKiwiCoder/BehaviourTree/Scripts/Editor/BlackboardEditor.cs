using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace TheKiwiCoder
{
    [CustomEditor(typeof(Blackboard))]
    public class BlackboardEditor : Editor
    {
        [SerializeField]
        VisualTreeAsset m_ItemAsset;

        public override VisualElement CreateInspectorGUI() {
            return new Label("Hello");
        }
    }
}

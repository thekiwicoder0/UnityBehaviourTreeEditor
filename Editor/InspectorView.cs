using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BehaviourTreeBuilder
{
    public class InspectorView : VisualElement
    {
        internal void UpdateSelection(SerializedBehaviourTree serializer, NodeView nodeView)
        {
            Clear();

            if (nodeView == null) return;

            var nodeProperty = serializer.FindNode(serializer.Nodes, nodeView.node);
            if (nodeProperty == null) return;

            // Auto-expand the property
            nodeProperty.isExpanded = true;

            // Property field
            var field = new PropertyField();
            field.label = nodeProperty.managedReferenceValue.GetType().ToString();
            field.BindProperty(nodeProperty);

            Add(field);
        }

        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
        {
        }
    }
}
using System.Collections.Generic;

namespace TheKiwiCoder {
    public interface IHasChild {
        void AddChild(Node child);
        void RemoveChild(Node child);
        List<Node> GetChildren();
    }
}
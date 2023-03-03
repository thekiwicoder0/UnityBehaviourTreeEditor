using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public class NodeProperty {
        [SerializeReference]
        public BlackboardKey reference; 
    }

    [System.Serializable]
    public class NodeProperty<T> : NodeProperty {

        public T defaultValue = default(T);
        private BlackboardKey<T> _typedKey = null;

        private BlackboardKey<T> typedKey {
            get {
                if (_typedKey == null && reference != null) {
                    _typedKey = reference as BlackboardKey<T>;
                }
                return _typedKey;
            }
        }

        public T Value {
            set {
                if (typedKey != null) {
                    typedKey.value = value;
                } else {
                    defaultValue = value;
                }
            }
            get {
                if (typedKey != null) {
                    return typedKey.value;
                } else {
                    return defaultValue;
                }
            }
        }
    }
}
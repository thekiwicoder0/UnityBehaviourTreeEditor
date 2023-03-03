using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public class BlackboardProperty {

        [SerializeReference]
        public BlackboardKey reference; 
    }

    [System.Serializable]
    public class BlackboardProperty<T> : BlackboardProperty {

        BlackboardKey<T> _typedKey = null;
        private BlackboardKey<T> typedKey {
            get {
                if (_typedKey == null) {
                    _typedKey = reference as BlackboardKey<T>;
                }
                return _typedKey;
            }
        }

        public T Value {
            set {
                if (typedKey != null) {
                    typedKey.value = value;
                }
            }
            get {
                if (typedKey != null) {
                    return typedKey.value;
                }
                return default(T);
            }
        }

        public bool IsValid() {
            return typedKey != null;
        }
    }
}
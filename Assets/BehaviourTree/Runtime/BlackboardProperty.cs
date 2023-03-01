using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public class BlackboardProperty {

        [SerializeReference]
        public BlackboardKey reference; 
    }

    [System.Serializable]
    public class BlackboardProperty<T> : BlackboardProperty {

        public T Value {
            set {
                BlackboardKey<T> typedKey = reference as BlackboardKey<T>;
                typedKey.value = value;
            }
            get {
                BlackboardKey<T> typedKey = reference as BlackboardKey<T>;
                return typedKey.value;
            }
        }
    }
}
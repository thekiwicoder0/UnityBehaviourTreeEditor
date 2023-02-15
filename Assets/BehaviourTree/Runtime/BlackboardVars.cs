using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public class FloatVar {

        [SerializeReference]
        private BlackboardKey key;

        public float Value {
            get {
                return key != null ? key.floatValue : 0.0f;
            }
            set {
                if (key != null) {
                    key.floatValue = value;
                }
            }
        }
    }

    [System.Serializable]
    public class IntVar {

        [SerializeReference]
        private BlackboardKey key;

        public int Value {
            get {
                return key != null ? key.intValue : 0;
            }
            set {
                if (key != null) {
                    key.intValue = value;
                }
            }
        }
    }

    [System.Serializable]
    public class BoolVar {

        [SerializeReference]
        private BlackboardKey key;

        public bool Value {
            get {
                return key != null ? key.booleanValue : false;
            }
            set {
                if (key != null) {
                    key.booleanValue = value;
                }
            }
        }
    }

    [System.Serializable]
    public class StringVar {

        [SerializeReference]
        private BlackboardKey key;

        public string Value {
            get {
                return key != null ? key.stringValue : "";
            }
            set {
                if (key != null) {
                    key.stringValue = value;
                }
            }
        }
    }

    [System.Serializable]
    public class Vector2Var {

        [SerializeReference]
        private BlackboardKey key;

        public Vector2 Value {
            get {
                return key != null ? key.vector2Value : Vector2.zero;
            }
            set {
                if (key != null) {
                    key.vector2Value = value;
                }
            }
        }
    }

    [System.Serializable]
    public class Vector3Var {

        [SerializeReference]
        private BlackboardKey key;

        public Vector3 Value {
            get {
                return key != null ? key.vector3Value : Vector3.zero;
            }
            set {
                if (key != null) {
                    key.vector3Value = value;
                }
            }
        }
    }

    [System.Serializable]
    public class GameObjectVar {

        [SerializeReference]
        private BlackboardKey key;

        public GameObject Value {
            get {
                return key != null ? key.gameObjectValue : null;
            }
            set {
                if (key != null) {
                    key.gameObjectValue = value;
                }
            }
        }
    }

    [System.Serializable]
    public class TagVar {

        [SerializeReference]
        private BlackboardKey key;

        public string Value {
            get {
                return key != null ? key.stringValue : "";
            }
            set {
                if (key != null) {
                    key.stringValue = value;
                }
            }
        }
    }

    [System.Serializable]
    public class LayerMaskVar {

        [SerializeReference]
        private BlackboardKey key;

        public LayerMask Value {
            get {
                return key != null ? key.layerMaskValue : new LayerMask();
            }
            set {
                if (key != null) {
                    key.layerMaskValue = value;
                }
            }
        }
    }
}
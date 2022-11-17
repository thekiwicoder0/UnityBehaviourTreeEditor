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
                return key.floatValue;
            }
            set {
                key.floatValue = value;
            }
        }
    }

    [System.Serializable]
    public class IntVar {

        [SerializeReference]
        private BlackboardKey key;

        public int Value {
            get {
                return key.intValue;
            }
            set {
                key.intValue = value;
            }
        }
    }

    [System.Serializable]
    public class BoolVar {

        [SerializeReference]
        private BlackboardKey key;

        public bool Value {
            get {
                return key.booleanValue;
            }
            set {
                key.booleanValue = value;
            }
        }
    }

    [System.Serializable]
    public class StringVar {

        [SerializeReference]
        private BlackboardKey key;

        public string Value {
            get {
                return key.stringValue;
            }
            set {
                key.stringValue = value;
            }
        }
    }

    [System.Serializable]
    public class Vector2Var {

        [SerializeReference]
        private BlackboardKey key;

        public Vector2 Value {
            get {
                return key.vector2Value;
            }
            set {
                key.vector2Value = value;
            }
        }
    }

    [System.Serializable]
    public class Vector3Var {

        [SerializeReference]
        private BlackboardKey key;

        public Vector3 Value {
            get {
                return key.vector3Value;
            }
            set {
                key.vector3Value = value;
            }
        }
    }

    [System.Serializable]
    public class GameObjectVar {

        [SerializeReference]
        private BlackboardKey key;

        public GameObject Value {
            get {
                return key.gameObjectValue;
            }
            set {
                key.gameObjectValue = value;
            }
        }
    }

    [System.Serializable]
    public class TagVar {

        [SerializeReference]
        private BlackboardKey key;

        public string Value {
            get {
                return key.stringValue;
            }
            set {
                key.stringValue = value;
            }
        }
    }

    [System.Serializable]
    public class LayerMaskVar {

        [SerializeReference]
        private BlackboardKey key;

        public LayerMask Value {
            get {
                return key.layerMaskValue;
            }
            set {
                key.layerMaskValue = value;
            }
        }
    }
}
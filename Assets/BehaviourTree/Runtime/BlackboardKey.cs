using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public class BlackboardKey {

        public enum Type {
            Float,
            Int,
            Boolean,
            String,
            Vector2,
            Vector3,
            GameObject,
            Tag,
            LayerMask
        }

        public string name;
        public Type type;

        public float floatValue;
        public int intValue;
        public bool booleanValue;
        public string stringValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public GameObject gameObjectValue;
        public LayerMask layerMaskValue;
    }
}
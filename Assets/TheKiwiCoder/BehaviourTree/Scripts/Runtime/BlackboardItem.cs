using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    [System.Serializable]
    public class BlackboardItem {

        public enum Type {
            Float,
            Int,
            Boolean,
            Vector3,
            Object
        }

        public string key;
        public Type type;


        public float floatValue;
        public int intValue;
        public bool booleanValue;
        public Vector3 vector3Value;
        public GameObject gameObjectValue;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    [System.Serializable]
    public class BlackboardKey {

        [SerializeReference]
        [HideInInspector] public BlackboardItem item;

        public void SetFloat(float value) {
            item.floatValue = value;
        }
    }
}

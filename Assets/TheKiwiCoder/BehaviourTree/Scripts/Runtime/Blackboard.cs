using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {

    // This is the blackboard container shared between all nodes.
    // Use this to store temporary data that multiple nodes need read and write access to.
    // Add other properties here that make sense for your specific use case.
    [System.Serializable]
    public class Blackboard {

        [SerializeReference]
        public List<BlackboardItem> items = new List<BlackboardItem>();

        public BlackboardItem Find(string key) {
            return items.Find((item) => {
                return item.key == key;
            });
        }
    }
}
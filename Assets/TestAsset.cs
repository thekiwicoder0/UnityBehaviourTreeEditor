using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder
{
    [CreateAssetMenu()]
    public class TestAsset : ScriptableObject
    {
        [System.Serializable]
        public class NodeTest {
            public int data = 7;
        }

        [SerializeReference]
        public NodeTest testNode = new NodeTest();

        [SerializeReference]
        public NodeTest testNode2;

        public TestAsset() {
            testNode2 = testNode;
        }
    }
}

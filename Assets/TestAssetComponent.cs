using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder
{
    public class TestAssetComponent : MonoBehaviour
    {
        public TestAsset asset;

        // Start is called before the first frame update
        void Start()
        {
            asset = Instantiate(asset);
        }

        // Update is called once per frame
        void Update()
        {
            asset.testNode.data++;
        }
    }
}

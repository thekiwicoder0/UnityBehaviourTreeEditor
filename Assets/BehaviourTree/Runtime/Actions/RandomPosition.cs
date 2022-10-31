using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

namespace TheKiwiCoder {
    [System.Serializable]
    public class RandomPosition : ActionNode {
        public Vector2 min = Vector2.one * -10;
        public Vector2 max = Vector2.one * 10;

        protected override void OnStart() {
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            Vector3 pos = new Vector3();
            pos.x = Random.Range(min.x, max.x);
            pos.y = Random.Range(min.y, max.y);
            blackboard.moveToPosition = pos;
            return State.Success;
        }
    }
}

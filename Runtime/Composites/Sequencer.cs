using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder {
    [System.Serializable]
    public class Sequencer : CompositeNode {

		int current = 0;

		protected override void OnStart() {
			current = 0;
		}

		protected override void OnStop() {
		}

		protected override State OnUpdate() {
			Node child = children[current];

			switch (child.Update()) {
				case State.Running:
					return State.Running;
				case State.Failure:
					return State.Failure;
				case State.Success:
					current++;
					break;
			}
			return current == children.Count ? State.Success : State.Running;
		}
	}
}
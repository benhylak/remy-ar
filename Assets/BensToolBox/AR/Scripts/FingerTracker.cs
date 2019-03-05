using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace BensToolBox.AR.Scripts
{
	public class FingerTracker : MonoBehaviour
	{

		private Vector3 leftHandSmoothed = Vector3.zero;

		private Vector3 rightHandSmoothed = Vector3.zero;

		[SerializeField] private GameObject leftFingerCollider;

		[SerializeField] private GameObject rightFingerCollider;

		public float lerpSpeed = 2;
		
		// Use this for initialization
		void Start()
		{
		}

		// Update is called once per frame
		void Update()
		{
			UpdateSmoothedFingerPosition(MLHands.Left, ref leftHandSmoothed);
			UpdateSmoothedFingerPosition(MLHands.Right, ref rightHandSmoothed);

			leftFingerCollider.transform.position = leftHandSmoothed;
			rightFingerCollider.transform.position = rightHandSmoothed;
		}

		void UpdateSmoothedFingerPosition(MLHand hand, ref Vector3 lastSmoothed)
		{
			if (lastSmoothed == null) return;
			
			if (hand != null)
			{
				Vector3 currentPosition = hand.Thumb.Tip.Position;

				if (lastSmoothed.Equals(Vector3.zero)) lastSmoothed = currentPosition;
				else
				{
					lastSmoothed = Vector3.Lerp(lastSmoothed, currentPosition, lerpSpeed * Time.deltaTime);
				}
			}
		}
	}
}

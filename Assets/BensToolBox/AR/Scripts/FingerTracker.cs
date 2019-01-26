using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DG;
using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class FingerTracker : MonoBehaviour {

	private Vector3 leftHandSmoothed = Vector3.zero;
	
	private Vector3 rightHandSmoothed = Vector3.zero;

	[SerializeField]
	private GameObject leftFingerCollider;
	
	[SerializeField]
	private GameObject rightFingerCollider;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update () {
		UpdateSmoothedFingerPosition(MLHands.Left);
		UpdateSmoothedFingerPosition(MLHands.Right);

		leftFingerCollider.transform.position = leftHandSmoothed;
		rightFingerCollider.transform.position = rightHandSmoothed;
	}

	void UpdateSmoothedFingerPosition(MLHand hand)
	{
		if (hand != null)
		{
			List<MLKeyPoint> drawingKeypoints = hand.Index.KeyPoints;
			Vector3 currentPosition = drawingKeypoints.First().Position;

		//	Vector3 lastSmoothed;
			
			if (hand == MLHands.Left)
			{
				leftFingerCollider.transform.position = currentPosition;
			}
			else
			{
				rightFingerCollider.transform.position = currentPosition;
			}
				
//			if (lastSmoothed == Vector3.zero) lastSmoothed = currentPosition;
//			else
//			{
//				lastSmoothed = Vector3.Lerp(lastSmoothed, currentPosition, 6.0f * Time.deltaTime);
//			}
//			
//			if (hand == MLHands.Left)
//			{
//				leftHandSmoothed = lastSmoothed;
//			}
//			else
//			{
//				rightHandSmoothed = lastSmoothed;
//			}
//			
//			Debug.Log("Updated pos: " + lastSmoothed.x);
		}
		
	}
}

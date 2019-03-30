using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeapInternal;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

/**
 * Smoothly follows image tracker. If Image tracker went from active-> inactive, it jumps instead of lerping (to avoid
 * really long lerps from god knows where)
 */
public class ImageTrackerLerper : MonoBehaviour
{
	public float lerpSpeed = 0.4f;

	public MLImageTrackerBehavior imageTracker;

	private bool wasActive = false;

	[NonSerialized]
	public bool TrackingEnabled = true;
	
	// Update is called once per frame
	void Update ()
	{
		if(!TrackingEnabled) return;
		
		if (imageTracker.IsTracking && imageTracker.TrackingStatus == MLImageTargetTrackingStatus.Tracked)
		{
			if (wasActive)
			{
				transform.position =
					Vector3.Lerp(
						transform.position,
						imageTracker.transform.position,
						lerpSpeed * Time.deltaTime);

				transform.rotation =
					Quaternion.Slerp(
						transform.rotation,
						imageTracker.transform.rotation,
						lerpSpeed * Time.deltaTime);
			}
			else JumpToTracker();

			wasActive = true;
		}
		else wasActive = false;
	}

	public void JumpToTracker()
	{
		transform.position = imageTracker.transform.position;
		transform.rotation = imageTracker.transform.rotation;
	}
}

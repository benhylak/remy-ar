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

	public bool wasActive = false;
	
	// Update is called once per frame
	void Update ()
	{
		bool targetSighted = imageTracker.TrackingStatus == MLImageTargetTrackingStatus.Tracked;

		if (targetSighted)
		{
			if (wasActive)
			{
				this.transform.position =
					Vector3.Lerp(
						transform.position,
						imageTracker.transform.position,
						lerpSpeed * Time.deltaTime);

				this.transform.rotation =
					Quaternion.Slerp(
						transform.rotation,
						imageTracker.transform.rotation,
						lerpSpeed * Time.deltaTime);
			}
			else
			{
				this.transform.position = imageTracker.transform.position;
				this.transform.rotation = imageTracker.transform.rotation;
			}
		}

		wasActive = targetSighted;
	}
}

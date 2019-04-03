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

	private Camera _mainCamera;
	private bool wasActive = false;

	[NonSerialized] public bool IsTrackingEnabled = true;

	public bool CompensateForDistance = true;
	public bool PreventClipping = false;
	public float lastActiveTime;
	public float adjustedLerpSpeed;
	
	//to select multisided objects, or even just multiple trackers for same thing, take an array of imageTrackers and find
	//the one that is most directly facing -- which we know will yield better recog results.
	
	void Start()
	{
		_mainCamera = Camera.main;
	}
	// Update is called once per frame
	void Update ()
	{
		if (!IsTrackingEnabled) return;
		
		var trackerPosition = imageTracker.transform.position;
		
		adjustedLerpSpeed = lerpSpeed;
		var trackerDistToCamera = Vector3.Distance(trackerPosition, _mainCamera.transform.position);
		
		if (CompensateForDistance)
		{ 
			//from 0.5m -> 2.5m, adjust from 100% to 30% of max lerp speed
			adjustedLerpSpeed = lerpSpeed * Mathf.Lerp(1f, 0.14f,
				                    Mathf.InverseLerp(0.4f, 1f,
				                  trackerDistToCamera));
		}
		
		if (PreventClipping && trackerDistToCamera < _mainCamera.nearClipPlane + .03f)
		{
			var hereToCameraVec = _mainCamera.transform.position - imageTracker.transform.position;
			var distToCamera = hereToCameraVec.magnitude;

			float adjustDist = distToCamera - (_mainCamera.nearClipPlane + .03f);
			trackerPosition += hereToCameraVec.normalized * adjustDist;
		}	
			
		if (imageTracker.IsTracking && imageTracker.TrackingStatus == MLImageTargetTrackingStatus.Tracked)
		{
			if (Time.time - lastActiveTime < 0.7f) //if it was less than .7 seconds since you've been tracked.
			{
				transform.position =
					Vector3.Lerp(
						transform.position,
						trackerPosition,
						adjustedLerpSpeed * Time.deltaTime);

				transform.rotation =
					Quaternion.Slerp(
						transform.rotation,
						imageTracker.transform.rotation,
						adjustedLerpSpeed * Time.deltaTime);
			}
			else JumpToTracker();
			lastActiveTime = Time.time;
		}
	}

	public void JumpToTracker()
	{
		transform.position = imageTracker.transform.position;
		transform.rotation = imageTracker.transform.rotation;
	}
}

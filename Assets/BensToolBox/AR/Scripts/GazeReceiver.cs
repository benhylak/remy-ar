using System;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GazeReceiver : MonoBehaviour
{

	//threshold, in seconds, for isLookedAt. If the last time it was looked at is within this threshold,
	//isLookAt will return true
	public float isLookedAtThreshold = 0.15f;
	public bool isLookedAt =>  _lastLookedAt != -1f && (Time.time - _lastLookedAt) < isLookedAtThreshold;

	public IObservable<Unit> OnGazeEnter;
	public IObservable<float> OnGazeExit;
	
	public float timeSinceLastGaze => Time.time - _lastLookedAt;

	public bool hasBeenLookedAt = false;


	public GazeReceiver()
	{
		var gazeChanged = Observable
			.EveryUpdate()
			.Select(_ => isLookedAt)
			.DistinctUntilChanged()
			.Skip(1); // skip the first one

		OnGazeEnter =
			gazeChanged
				.Where(lookedAt => lookedAt == true)
				.Select(_ => Unit.Default);

		OnGazeExit =
			gazeChanged
				.Where(lookedAt => lookedAt == false) // only take values where it is not lookedAt.
				.Select(_ => Time.time - _gazeStartTime); //map to gaze duration
	}
	
	//if current 
	public float currentGazeDuration
	{
		get
		{
			if (isLookedAt)
			{
				return Time.time - _gazeStartTime;
			}
			else return -1;
		}
	}

	private float _gazeStartTime = -1;
	
	//time that this object was last lookedAt
	private float _lastLookedAt = -1f;

	private bool _previousIsLookedAt;
	
	// Use this for initialization
	void Start ()
	{
		
		
			
	}
	
	// Update is called once per frame
	void Update()
	{		
		_previousIsLookedAt = isLookedAt;
	}

	public void OnLook()
	{
		_lastLookedAt = Time.time;		
		
		if (isLookedAt && !_previousIsLookedAt)
		{
			_gazeStartTime = Time.time;
		}

		hasBeenLookedAt = true;
	}

	//resets hasBeenLookedAt to false. Useful for tracking whether or not something has been looked at, at least once,
	//within a certain frame of reference
	public void ResetLookedAtFlag()
	{
		hasBeenLookedAt = false;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Allows for one sided collisions. This is done by having two colliders -- one "main collider" and another
 * "ignore zone" collider, with trigger set to true. If a collision is detected while in this ignore zone,
 * the collision with the main collider is ignored.
 *
 * This is useful for say, a button, where you wouldn't want to be able to lift up the button. And also, because
 * Magic Leap's hand tracking is awful.
 *
 * Optionally, can "freeze" the button until out of the ignore zone. Useful for, hey, a button where you want to hold it
 * down. I'm really starting to think this was written for a button!
 */
public class IgnoreColliderZone : MonoBehaviour
{
	public Collider mainCollider;

	public bool maintainLocalPosTillExit = true;

	private Vector3 _posToHold;

	private bool _ignoreInProgress;

	private void Update()
	{
		if (_ignoreInProgress && maintainLocalPosTillExit)
		{
			transform.localPosition = _posToHold;
			
			Debug.Log("Hold your position!");
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Physics.IgnoreCollision(mainCollider, other);

		_posToHold = transform.localPosition;

		_ignoreInProgress = true;
	}

	private void OnTriggerExit(Collider other)
	{
		Physics.IgnoreCollision(mainCollider, other, false);

		_ignoreInProgress = false;
	}
}

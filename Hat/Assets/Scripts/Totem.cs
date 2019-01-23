using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class Totem : MonoBehaviour
{
    [SerializeField]
    private MLImageTrackerBehavior marker;

    private bool _hasBeenInit = false;

    private bool enter = true;

    public StoveManager _StoveManager;
    
    // Use this for initialization
    void Start () {
		
    }
	
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Hand")
        {
            _StoveManager.DisplaySelectors(0, 0);
            Debug.Log("entered");
        }
    }
    
    // Update is called once per frame
    void Update () {
        if (marker.IsTracking && marker.TrackingStatus == MLImageTargetTrackingStatus.Tracked)
        {
            if (!_hasBeenInit)
            {
                _hasBeenInit = true;
                transform.position = marker.gameObject.transform.position;
                transform.rotation = marker.gameObject.transform.rotation;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, marker.gameObject.transform.position, 4* Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, marker.gameObject.transform.rotation, 4* Time.deltaTime);
            }	
        }
        
    }
    
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

//will enable these objects once privileges are received
public class PrivilegeWaiter : MonoBehaviour
{
	public GameObject[] _objectsToControl;
	private PrivilegeRequester _privilegeRequester;
	
	void Start()
	{
	
	}
	void Awake ()
	{
		#if UNITY_EDITOR
			foreach(var o in _objectsToControl) o.SetActive(true);
		#else
			foreach(var o in _objectsToControl) o.SetActive(false);
			
			_privilegeRequester = GetComponent<PrivilegeRequester>();
			if (_privilegeRequester == null)
			{
				Debug.LogError("Missing PrivilegeRequester component");
				enabled = false;
				return;
			}
			_privilegeRequester.OnPrivilegesDone += HandlePrivilegesDone;
		#endif	
	}

	void OnDestroy()
	{
		if (_privilegeRequester != null)
		{
			_privilegeRequester.OnPrivilegesDone -= HandlePrivilegesDone;
		}
	}

	void HandlePrivilegesDone(MLResult result)
	{
		if (!result.IsOk)
		{
			Debug.LogError("Failed to get all requested privileges. MLResult: " + result);
			enabled = false;
			return;
		}

		foreach (var o in _objectsToControl)
		{
			o.SetActive(true);
		}
	}

}

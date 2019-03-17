using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RamenUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//x and z 
		// -10 to 0 over X seconds

		Vector3 eulerAngles = new Vector3();

		eulerAngles.x = 6f;
		eulerAngles.z = 6f * Mathf.Sin(1.25f * Time.time);
		eulerAngles.y = transform.localRotation.eulerAngles.y + 125f * Time.deltaTime;

		this.transform.localRotation = Quaternion.Euler(eulerAngles);

	}
}

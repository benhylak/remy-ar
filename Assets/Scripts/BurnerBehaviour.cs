using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnerBehaviour : MonoBehaviour
{
	public Burner _model;
	public Burner.BurnerPosition _position;
	
	public StoveTimer _Timer;

	public GameObject burnerMesh;
	public GameObject ring;

	public bool IsLookedAt;

	public void ShowInputPrompt()
	{
		
	}
	
	public void HideInputPrompt()
	{
		
	}

	public void SuggestTimer()
	{
		Debug.Log("Proactive Timer Activated");
	}
	
	//public TimerRing _timerRingPrefab;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

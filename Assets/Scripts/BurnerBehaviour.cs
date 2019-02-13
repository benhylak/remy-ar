using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using BensToolBox.AR.Scripts;

public class BurnerBehaviour : MonoBehaviour
{
	public Burner _model;
	public Burner.BurnerPosition _position;
	
	public StoveTimer _Timer;

	public GameObject burnerMesh;
	public GameObject ring;

	public float RING_RADIUS = 0.18f;

	public bool IsLookedAt;
	public Text FloatingLabel;
	private bool IsLabelVisible = false;

	public void Start()
	{
		FloatingLabel.DOFade(0, 0);
	}
	
	public void ShowInputPrompt()
	{
		
	}
	
	public void HideInputPrompt()
	{
		
	}

	public void ShowProactiveTimer()
	{
		ring.SetActive(true);
		SetRingRadius(0);
		
		FloatingLabel.DOFade(1, 1.5f);
		DOTween.To(GetRingRadius, SetRingRadius, 0.18f, 1f)
			.SetEase(Ease.InSine);

		IsLabelVisible = true;
	
		Debug.Log("Proactive Timer Activated");
	}

	public void HideProactiveTimer()
	{
		FloatingLabel.DOFade(0, 1.5f);
		DOTween.To(GetRingRadius, SetRingRadius, 0f, 1f)
			.SetEase(Ease.OutSine);

		IsLabelVisible = false;
	}

	private void SetRingRadius(float radius)
	{
		ring.GetComponent<Renderer>().material.SetFloat("_Radius", radius);
	}
	
	private float GetRingRadius()
	{
		return ring.GetComponent<Renderer>().sharedMaterial.GetFloat("_Radius");
	}
	
	// Update is called once per frame
	void Update () {
		if (IsLabelVisible)
		{
			FloatingLabel.transform.rotation = Quaternion.LookRotation(FloatingLabel.transform.position - Camera.main.transform.position);
		}
	}
}

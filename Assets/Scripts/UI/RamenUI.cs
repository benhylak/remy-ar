using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using UnitySDK.WebSocketSharp;

//TODO:
//create ramen state machine:
//	none
// 	voice input
//  recognized

// none
public class RamenUI : MonoBehaviour
{
	public GameObject ring;
	public Text ramenLabel;
	public Text promptLabel;
	public SpriteRenderer microphoneIcon;
	
	public bool isListening;
	private bool _inputIsEnabled = true;

	public Image _addWaterInstructions;
	
	// Use this for initialization
	void Start ()
	{
		ring.GetComponent<Renderer>().material.DOFade(0, 0);
		ramenLabel.DOFade(0, 0);
		promptLabel.DOFade(0, 0);
		microphoneIcon.DOFade(0, 0);
	}

	void StartListening()
	{
		if (BigKahuna.Instance.speechRecognizer.IsInitalized)
		{
			isListening = true;

			promptLabel.text = "what do you want to do?";

			ring.GetComponent<Renderer>().material.DOFade(1, 0.25f);
			ramenLabel.DOFade(1, 0.25f);
			promptLabel.DOFade(1, 0.25f);
			microphoneIcon.DOFade(1, 0.25f);

			BigKahuna.Instance.speechRecognizer.Active = true;
		}
	}

	public void MakeRamen()
	{
		Debug.Log("Successful Command: Making Ramen");
		StopListening();
		
		var ramenRecipe = new RamenRecipe(this);
		RecipeManager.Instance.StartRecipe(ramenRecipe);

		_inputIsEnabled = false;
	}

	void StopListening()
	{
		isListening = false;
		
		ring.GetComponent<Renderer>().material.DOFade(0, 0.25f);
		ramenLabel.DOFade(0, 0.25f);
		promptLabel.DOFade(0, 0.25f);
		microphoneIcon.DOFade(0, 0.25f);

		BigKahuna.Instance.speechRecognizer.recognizedText = ""; //consume text

		BigKahuna.Instance.speechRecognizer.Active = false;
	}
	
	// Update is called once per frame
	void Update () {
		//x and z 
		// -10 to 0 over X seconds
		
		if (_inputIsEnabled && Vector3.Distance(transform.position, Camera.main.transform.position) < 0.6f)
		{
			if (!isListening)
			{
				StartListening(); //start mic
			}
		}
		else if (isListening)
		{
			//gonna need a state here, transition to show 1 -- a managed state, until the recipe is no longer being made
			//sometype of buffer period
			//then 
			StopListening();
		}	
		
		if (isListening)
		{
			UpdateListeningRing();

			var recognizedText = BigKahuna.Instance.speechRecognizer.recognizedText.ToLower();

			if (!recognizedText.IsNullOrEmpty())
			{
				promptLabel.text = recognizedText;
			}
			
			if (BigKahuna.Instance.speechRecognizer.finalized)
			{
			
				if (recognizedText.Contains("make") ||
				    recognizedText.Contains("take") ||
				    recognizedText.Contains("cook") ||
				    recognizedText.Contains("prepare") ||
				    recognizedText.Contains("eat"))
				{
					MakeRamen();
					//trigger make ramen instruction
				}
				else
				{
					BigKahuna.Instance.speechRecognizer.finalized = false;
					BigKahuna.Instance.speechRecognizer.recognizedText = "";
					
					var seq = DOTween.Sequence();

					seq.Append(
						promptLabel
							.DOColor(new Color(255f, 0, 0, 80f), 0.2f));

					seq.Append(
						promptLabel.DOFade(0, 0.3f)
							.SetDelay(.35f)
							.OnComplete(() =>
							{
								promptLabel.DOColor(new Color(255, 255, 255, 0), 0);
								promptLabel.text = "what do you want to do?";
							})
					);

					seq.Append(
						promptLabel.DOFade(1, 0.3f));
				}
			}
		}
	}

	void UpdateListeningRing()
	{
		Vector3 eulerAngles = new Vector3();

		eulerAngles.x = 6f;
		eulerAngles.z = 6f * Mathf.Sin(1.25f * Time.time);
		eulerAngles.y = ring.transform.localRotation.eulerAngles.y + 125f * Time.deltaTime;

		ring.transform.localRotation = Quaternion.Euler(eulerAngles);
	}

	public void ShowStep1()
	{
		_addWaterInstructions.DOFade(1f, 0.3f);
	}

	public void HideStep1()
	{
		_addWaterInstructions.DOFade(0f, 0.3f);
	}
}

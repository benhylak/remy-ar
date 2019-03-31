using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using UnityEngine.XR.MagicLeap;
using UnitySDK.WebSocketSharp;

//TODO:
//create ramen state machine:
//	none
// 	voice input
//  recognized

// none
public class RamenUI : MonoBehaviour, InstructionsAnchorable
{
	public GameObject ring;
	public Text ramenLabel;
	public Text promptLabel;
	public SpriteRenderer microphoneIcon;

	public Transform instructionsAnchorFlat;
	public Transform instructionsAnchorBillboard;
	
	public bool isListening;
	public bool inputIsEnabled = true;

	private readonly float SWITCH_TO_BILLBOARD_DIST = 0.65f;
	private readonly float SWITCH_TO_FLAT_DIST = 0.5f;

	private Transform _lastBestAnchor;

	private InstructionUI _instructionUi;
	
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
		//RecipeManager.Instance.StartRecipe(ramenRecipe);
	
		RecipeManager.Instance.StartRecipe(ramenRecipe);
		inputIsEnabled = false;
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

		if (inputIsEnabled && Vector3.Distance(transform.position, Camera.main.transform.position) < 0.6f)
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

			//stand in for working voice recog.
			if (MLInput.IsStarted && MLInput.GetController(0).TriggerValue > MLInput.TriggerDownThreshold)
			{
				MakeRamen();
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
		Vector3 eulerAngles = new Vector3
		{
			x = 15f,
			z = 15f * Mathf.Sin(1.3f * Time.time),
			y = ring.transform.localRotation.eulerAngles.y + 125f * Time.deltaTime
		};

		ring.transform.localRotation = Quaternion.Euler(eulerAngles);
	}

	public void AnchorInstructions(InstructionUI instructions)
	{			
		_instructionUi = instructions;
		
		instructions.transform.parent = this.transform;
		
		instructions.transform.localScale = instructionsAnchorFlat.localScale;
		instructions.transform.position = instructionsAnchorFlat.position;
		instructions.transform.rotation = instructionsAnchorFlat.rotation;

		instructions.Show();
	}

	public Transform GetBestAnchorPoint()
	{
		//if there hasn't been a best anchor yet, set it to default
		if (_lastBestAnchor == null)
		{
			_lastBestAnchor = instructionsAnchorFlat;
			_instructionUi.LookAtCamera = false;		
		}

		Transform bestAnchorPoint = _lastBestAnchor;

		//check if there's a better point besides default
		if (Vector3.Distance(transform.position, Camera.main.transform.position) > SWITCH_TO_BILLBOARD_DIST)
		{				
			_instructionUi.LookAtCamera = true;		
			bestAnchorPoint = instructionsAnchorBillboard;
		}
		else if (Vector3.Distance(transform.position, Camera.main.transform.position) < SWITCH_TO_FLAT_DIST)
		{
			_instructionUi.LookAtCamera = false;
			bestAnchorPoint = instructionsAnchorFlat;
		}

		if (bestAnchorPoint != _lastBestAnchor)
		{
			_instructionUi.transform.DOScale(bestAnchorPoint.localScale, 0.3f).SetEase(Ease.OutQuad);
			_lastBestAnchor = bestAnchorPoint;
		}

		return bestAnchorPoint;
	}

	public void DeAnchor()
	{
		_instructionUi.transform.parent = null;
		_instructionUi = null;
		_lastBestAnchor = null;
	}
}

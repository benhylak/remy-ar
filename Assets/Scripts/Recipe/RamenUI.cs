using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using UnityEngine.XR.MagicLeap;
using UnitySDK.WebSocketSharp;
using BensToolBox.AR.Scripts;

// none
public class RamenUI : MonoBehaviour, InstructionsAnchorable
{
	public GameObject ring;
	public Text ramenLabel;
	public Text promptLabel;
	public SpriteRenderer microphoneIcon;

	public Transform headAnchorPoint;
	public Transform instructionsAnchorFlat;
	public Transform instructionsAnchorBillboard;
	
	public bool isListening;
	public bool inputIsEnabled = true;

	private readonly float SWITCH_TO_BILLBOARD_DIST = 0.65f;
	private readonly float SWITCH_TO_FLAT_DIST = 0.5f;

	public SpriteRenderer outline;
	
	private Transform _lastBestAnchor;

	private InstructionUI _instructionUi;
	private ImageTrackerLerper _imageTracker;
	private Camera _mainCamera;

	public bool headTrackingEnabled = true;
	
	// Use this for initialization
	void Start ()
	{
		ring.GetComponent<Renderer>().material.DOFade(0, 0);
		ramenLabel.DOFade(0, 0);
		promptLabel.DOFade(0, 0);
		microphoneIcon.DOFade(0, 0);

		_imageTracker = GetComponent<ImageTrackerLerper>();
		
		_mainCamera = Camera.main;
	}

	void StartListening()
	{
		if (BigKahuna.Instance.speechRecognizer.IsInitalized)
		{
			BigKahuna.Instance.DisableOtherListeners = true;
			
			isListening = true;

			promptLabel.text = "what do you want to do?";

			ring.GetComponent<Renderer>().material.DOFade(1, 0.35f);
			ramenLabel.DOFade(1, 0.35f);
			promptLabel.DOFade(1, 0.35f);
			microphoneIcon.DOFade(1, 0.35f);
			outline.DOFade(1, 0.35f);

			BigKahuna.Instance.speechRecognizer.Active = true;
		}
	}

	public void MakeRamen()
	{
		Debug.Log("Successful Command: Making Ramen");
		
		var ramenRecipe = new RamenRecipe(this);
		//RecipeManager.Instance.StartRecipe(ramenRecipe);

		inputIsEnabled = false;
		
		this.DelayedInvokeOnMainThread(.3f, () => RecipeManager.Instance.StartRecipe(ramenRecipe));
	}

	void StopListening()
	{
		isListening = false;

		ring.GetComponent<Renderer>().material.DOFade(0, 0.25f);
		ramenLabel.DOFade(0, 0.25f);
		promptLabel.DOFade(0, 0.25f);
		microphoneIcon.DOFade(0, 0.25f);
		outline.DOFade(0, 0.25f);

		BigKahuna.Instance.speechRecognizer.recognizedText = ""; //consume text
		BigKahuna.Instance.speechRecognizer.Active = false;
		BigKahuna.Instance.DisableOtherListeners = false;
	}

	// Update is called once per frame
	void Update () {

		if (inputIsEnabled && _imageTracker.IsTrackingActive() && Vector3.Distance(transform.position, _mainCamera.transform.position) < 0.6f)
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
								promptLabel.text = "say an action like 'make' or 'buy more'";
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

		var instructionTransform = instructions.transform;

		var bestAnchor = GetBestAnchorPoint();

		instructionTransform.parent = bestAnchor;

		instructionTransform.localScale = bestAnchor.localScale;
		instructionTransform.position = bestAnchor.position;
		instructionTransform.rotation = bestAnchor.rotation;

		instructions.Show();
	}

	public bool IsFullyVisible()
	{
		return ramenLabel.rectTransform.IsFullyVisibleFrom(_mainCamera);
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
		
//		if (headTrackingEnabled && (!IsFullyVisible() || !_imageTracker.IsTrackingActive()))
//		{
//			bestAnchorPoint = headAnchorPoint;
//			_instructionUi.LookAtCamera = false;
//		}else
		if (Vector3.Distance(transform.position, _mainCamera.transform.position) > SWITCH_TO_BILLBOARD_DIST)
		{				
			_instructionUi.LookAtCamera = true;		
			bestAnchorPoint = instructionsAnchorBillboard;
		}
		else if (Vector3.Distance(transform.position, _mainCamera.transform.position) < SWITCH_TO_FLAT_DIST)
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

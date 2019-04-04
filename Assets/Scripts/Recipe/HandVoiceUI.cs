using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using UnityEngine.XR.MagicLeap;
using UnitySDK.WebSocketSharp;
using BensToolBox.AR.Scripts;

// none
[RequireComponent(typeof(KeyPoseTracker))]
public class HandVoiceUI : MonoBehaviour
{
	public GameObject ring;
	public Text mainLabel;
	public Text promptLabel;
	public SpriteRenderer microphoneIcon;

	public bool isListening;
	public bool inputIsEnabled = true;

	private Camera _mainCamera;
	private KeyPoseTracker _keyPoseTracker;
	private float _lastStateChange = 0;

	public MeshRenderer sphere;
	private bool _sphereIsVisible = false;
	
	enum VoiceUIState
	{
		WHAT,
		HOW_MANY
	}

	private VoiceUIState _inputState;
	
	// Use this for initialization
	void Start ()
	{
		ring.GetComponent<Renderer>().material.DOFade(0, 0);
		mainLabel.DOFade(0, 0);
		promptLabel.DOFade(0, 0);
		microphoneIcon.DOFade(0, 0);
		_keyPoseTracker = GetComponent<KeyPoseTracker>();

		_mainCamera = Camera.main;
		
		sphere.material.DOFade(0, 0).SetEase(Ease.InSine);
	}

	public void StartListening()
	{
		if (BigKahuna.Instance.speechRecognizer.IsInitalized)
		{
			BigKahuna.Instance.DisableOtherListeners = true;
			
			isListening = true;

			promptLabel.text = "what do you want to do?";

			ring.GetComponent<Renderer>().material.DOFade(1, 0.25f);
			mainLabel.DOFade(1, 0.25f);
			promptLabel.DOFade(1, 0.25f);
			microphoneIcon.DOFade(1, 0.25f);

			BigKahuna.Instance.speechRecognizer.Active = true;
		}
	}

//	public void MakeRamen()
//	{
//		Debug.Log("Successful Command: Making Ramen");
//		
//		RecipeManager.Instance.RamenPackage.MakeRamen();
//		
//		var ramenRecipe = new RamenRecipe();
//		//RecipeManager.Instance.StartRecipe(ramenRecipe);
//
//		inputIsEnabled = false;
//		
//		this.DelayedInvokeOnMainThread(.3f, () => RecipeManager.Instance.StartRecipe(ramenRecipe));
//	}

	private void DefaultPrompts()
	{
		mainLabel.text = "Listening...";
		promptLabel.text = "what do you want to make?";
	}

	public void MakePancakes()
	{
		Debug.Log("Successful Command: Making Ramen");
		
		var pancakeRecipe = new PancakeRecipe();
		//RecipeManager.Instance.StartRecipe(ramenRecipe);

		inputIsEnabled = false;
		StopListening();
				
		this.DelayedInvokeOnMainThread(.3f, () => RecipeManager.Instance.StartRecipe(pancakeRecipe));
		this.DelayedInvokeOnMainThread(5f, () => inputIsEnabled = true);
	}
	
	public void StopListening()
	{
		isListening = false;

		ring.GetComponent<Renderer>().material.DOFade(0, 0.25f);
		mainLabel.DOFade(0, 0.25f);
		promptLabel.DOFade(0, 0.25f);
		microphoneIcon.DOFade(0, 0.25f);
		
		this.DelayedInvokeOnMainThread(0.25f, DefaultPrompts);

		BigKahuna.Instance.speechRecognizer.recognizedText = ""; //consume text
		BigKahuna.Instance.speechRecognizer.Active = false;
		BigKahuna.Instance.DisableOtherListeners = false; // need to add more flags for this to avoid conflict with package
	}

	// Update is called once per frame
	void Update ()
	{
//		if (inputIsEnabled && Vector3.Distance(transform.position, _mainCamera.transform.position) < 0.6f)
//		{
//			if (!isListening)
//			{
//				StartListening(); //start mic
//			}
//		}
//		else if (isListening)
//		{
//			//gonna need a state here, transition to show 1 -- a managed state, until the recipe is no longer being made
//			//sometype of buffer period
//			//then 
//			StopListening();
//		}	
//		
		if (RecipeManager.Instance.IsRecipeInProgress)
		{
			if (_sphereIsVisible)
			{
				sphere.material.DOKill();
				sphere.material.DOFade(0, 0.2f).SetEase(Ease.InSine);
				_sphereIsVisible = false;
			}
		}
		else if (_keyPoseTracker.KeyposeActive)
		{
			transform.position = _keyPoseTracker.handPosition;
			if (!_sphereIsVisible)
			{
				sphere.material.DOKill();
				sphere.material.DOFade(0.65f, 0.2f).SetEase(Ease.InSine);
				_sphereIsVisible = true;
			}
		}
		else if (_sphereIsVisible)
		{
			sphere.material.DOKill();
			sphere.material.DOFade(0, 0.2f).SetEase(Ease.InSine);
			_sphereIsVisible = false;
		}

		if (Time.time - _lastStateChange > 0.75f && inputIsEnabled)
		{		
			if (isListening && (!_keyPoseTracker.KeyposeActive || 
			                    BigKahuna.Instance.ramenUI.isListening || 
			                    RecipeManager.Instance.IsRecipeInProgress || 
			                Time.time - _keyPoseTracker.lastStationaryTime > 0.3))
			{
				StopListening();			
				_lastStateChange = Time.time;
			}
			else if (_keyPoseTracker.KeyposeActive && !isListening && _keyPoseTracker.HasBeenStationaryForSeconds > 0.45f)
			{ 
				StartListening();			
				_lastStateChange = Time.time;
			}
		}             
		
		if (isListening)
		{
			UpdateListeningRing();
			
			var lookVec = transform.position - _mainCamera.transform.position;
			lookVec.y = lookVec.y / 3; //make the angle less aggressive.
				
			transform.rotation = Quaternion.LookRotation(lookVec);

			var recognizedText = BigKahuna.Instance.speechRecognizer.recognizedText.ToLower();

			if (!recognizedText.IsNullOrEmpty())
			{
				promptLabel.text = recognizedText;
			}

			if (BigKahuna.Instance.speechRecognizer.finalized)
			{	
				if (_inputState == VoiceUIState.WHAT && recognizedText.Contains("pancake"))
				{
					_inputState = VoiceUIState.HOW_MANY;
					mainLabel.text = "How many?";
					promptLabel.text = "say a number";

					//ask "How many?"
					//trigger make ramen instruction
				}
				else if (_inputState == VoiceUIState.HOW_MANY && recognizedText.Any(char.IsDigit))
				{
					MakePancakes(); //would someday pass in the number
				}
				else
				{
					
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
								promptLabel.text = "sorry, i didn't get that";
							})
					);

					seq.Append(
						promptLabel.DOFade(1, 0.3f));
				}
				
				BigKahuna.Instance.speechRecognizer.finalized = false;
				BigKahuna.Instance.speechRecognizer.recognizedText = "";
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
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using BensToolBox.AR.Scripts;
using Burners.States;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Services.VisualRecognition.v3;
using UnityEngine.Serialization;
using UnityEngine.XR.MagicLeap;

public class BurnerBehaviour : MonoBehaviour, InstructionsAnchorable
{	
	public Burner _model;
	public Burner.BurnerPosition _position;
	
	public BurnerTimer _Timer;

	public GameObject burnerMesh;
	public BurnerRingController ring;

	public BurnerOnVisualizer BurnerOnVisualizer;
	
	public Text FloatingLabel;
	
	private bool IsLabelVisible = false;	
	private bool showInput = false;

	private bool isMonitorBoiling;

	public NotificationManager.NotificationEventHandler OnBurnerNotification;

	public State _state;
	public GazeReceiver _gazeReceiver;

	public Transform instructionsMiddleAnchorPoint;
	public Transform instructionsEdgeAnchorPoint;
	private Transform _lastBestAnchorPoint;
	
	private InstructionUI _instructionUi;

	private readonly float SWITCH_TO_EDGE_DIST = 0.35f;
	private readonly float SWITCH_TO_CENTER_DIST = 0.55f;

	private Tween _labelTween;
	private bool _labelIsHidden = true;

	public float? targetTemperature = null;
	private Camera _mainCamera;

	public void Start()
	{
		FloatingLabel.gameObject.SetActive(true);
		HideLabel();
		
		_state = new BurnerStates.AvailableState(this);
		_gazeReceiver = GetComponent<GazeReceiver>();
		_mainCamera = Camera.main;
		
	}
	
	public bool IsBoiling()
	{
		return _model.IsBoiling.Value;
	}

	public void RaiseBurnerNotification(string text) //urgency level
	{
		OnBurnerNotification(new NotificationManager.Notification(text, this));
	}

	public bool HasReachedTargetTemp()
	{
		if (targetTemperature.HasValue)
		{
			return _model.Temperature.Value > targetTemperature.Value;
		}
		else
		{
			throw new Exception("HasReachedTargetTemp called, but burner is not targeting a temp.");
		}
	}

	public void SetStateToDefault()
	{
		if (_Timer.isSet)
		{
			_Timer.Reset();
			_Timer.Hide();
		}
		_state = new BurnerStates.AvailableState(this);
	}
	


	public void SetTimer(TimeSpan ts)
	{  
		_state = new TimerStates.WaitingForTimerState(this, ts);
	}
	
	// Update is called once per frame
	void Update () {
		instructionsEdgeAnchorPoint.position = GetEdgeAnchorPosition();	
		
		if (FloatingLabel.isActiveAndEnabled)
		{
			FloatingLabel.transform.rotation =
				Quaternion.LookRotation(FloatingLabel.transform.position - _mainCamera.transform.position);
		}

		var resultState = _state.Update();
		_state = resultState ?? _state;
	}

	//wait -- these functions don't make any sense. at least not to be called from the notification itself
	//dismissing notification wouldn't dismiss the state. but the state could change...
	// like if the state == boiling
	// if pot removed = false, dismiss the state
	public void SetLabel(string text, float duration = 0.35f)
	{
		Debug.Log("Set Label to: " + text);
		_labelTween?.Kill();
		
		if (_labelIsHidden)
		{
			Debug.Log("Label is already hidden...");
			
			FloatingLabel.text = text;
			_labelTween = FloatingLabel.DOFade(1, duration).OnComplete(() => _labelIsHidden = false);
		}
		else
		{
			_labelTween = HideLabel();
			
			_labelTween
				.OnComplete(() =>
				{
					FloatingLabel.text = text;
					_labelTween = FloatingLabel.DOFade(1, duration).SetEase(Ease.InSine)
						.OnComplete(() => _labelIsHidden = false);
				});
		}
	}

	#region Display Functions -- (they don't handle state)

	public Tween HideLabel(float duration = 0.3f)
	{
		Debug.Log("Hide called");
		
		_labelTween?.Kill();

		_labelTween =
			FloatingLabel
				.DOFade(0, duration)
				.SetEase(Ease.OutSine)
				.OnComplete(() => _labelIsHidden = true);

		return _labelTween;
	}
	
	public void ShowInputPrompt()
	{
		SetLabel( "Timer for how long?");
		
		ring.SetVoiceLerp(0);
		ring.SetMaterialToVoiceInput();
				
		ring.SetWaveAmplitude(0f);

		DOTween.To(ring.GetVoiceLerp, ring.SetVoiceLerp, 1f, 0.3f).SetEase(Ease.InSine);
		DOTween.To(ring.GetWaveAmplitude, ring.SetWaveAmplitude, ring.WAVE_AMPLITUDE, 1f)
			.SetEase(Ease.InSine);
	}

	public void SetInputLevel(float volume)
	{
		ring.SetInputLevel(volume);
	}

	public void HiddenToProactive()
	{
		ring.gameObject.SetActive(true);
		ring.ShowFancy();
				
		FloatingLabel.gameObject.SetActive(true);
		FloatingLabel.DOFade(1, 1.5f).SetEase(Ease.InCubic);					
						
		Debug.Log("Proactive Timer Activated");
	}

	public void InputToProactive()
	{
		SetLabel("Set a Timer");

		Sequence inputToProactiveSeq = DOTween.Sequence();

		inputToProactiveSeq.Append(
			DOTween.To(
				ring.GetVoiceLerp,
				ring.SetVoiceLerp,
				0f,
				0.5f).SetEase(Ease.InSine));

		inputToProactiveSeq.Insert(0,
			DOTween.To(
				ring.GetWaveAmplitude,
				ring.SetWaveAmplitude,
				0f,
				0.5f).SetEase(Ease.InSine));
					
		inputToProactiveSeq.OnComplete(() =>
		{		
			ring.SetMaterialToDefault();
		});
		
		Debug.Log("Waiting for Input -> Proactive Timer");
	}

	public Tween HideProactivePrompt()
	{
		HideLabel();

		return ring.HideFancy();
	}
	
	#endregion

	public void AnchorInstructions(InstructionUI instructions)
	{
		_instructionUi = instructions;
		
		var anchorPoint = GetBestAnchorPoint();

		instructions.transform.parent = this.transform;
		instructions.transform.localScale = anchorPoint.localScale;
		instructions.transform.position = anchorPoint.position;
		instructions.transform.rotation = anchorPoint.rotation;

		instructions.Show(delay:0.35f);
	
	}

	public Transform GetBestAnchorPoint()
	{
		//if there hasn't been a best anchor yet, set it to default
		if (_lastBestAnchorPoint == null)
		{
			_lastBestAnchorPoint = instructionsMiddleAnchorPoint;
			_instructionUi.LookAtCamera = true;		
		}

		Transform bestAnchorPoint = _lastBestAnchorPoint;


		float userDistanceToBurner = float.MaxValue;
		
		if (MLHands.IsStarted)
		{
			var distToLeftHand = Vector3.Distance(transform.position, MLHands.Left.Center);
			var distToRightHand = Vector3.Distance(transform.position, MLHands.Right.Center);

			if (MLHands.Right.IsVisible) userDistanceToBurner = distToRightHand;
			if (MLHands.Left.IsVisible) userDistanceToBurner = Mathf.Min(userDistanceToBurner, distToLeftHand);
		}
		else
		{
			//fallback to head if we can't get hands
			userDistanceToBurner = Vector3.Distance(transform.position, _mainCamera.transform.position);
		}
		//check if there's a better point besides default
		if (userDistanceToBurner < SWITCH_TO_EDGE_DIST)
		{				
			_instructionUi.LookAtCamera = true;
			bestAnchorPoint = instructionsEdgeAnchorPoint;
		}
		else if (userDistanceToBurner > SWITCH_TO_CENTER_DIST)
		{
			_instructionUi.LookAtCamera = true;
			bestAnchorPoint = instructionsMiddleAnchorPoint;
		}

		if (bestAnchorPoint != _lastBestAnchorPoint)
		{
			//_instructionUi.transform.DOScale(bestAnchorPoint.localScale, 0.3f).SetEase(Ease.OutQuad);
			_lastBestAnchorPoint = bestAnchorPoint;
		}

		return bestAnchorPoint;
	}


	public void DeAnchor()
	{
		_instructionUi = null;
	}

	private Vector3 GetEdgeAnchorPosition()
	{
		Vector3 lookVecFlattened = ring.transform.position - _mainCamera.transform.position; 
		lookVecFlattened.y = ring.transform.position.y;

		Vector3 furthestRingPoint = ring.transform.position + lookVecFlattened.normalized * (ring.GetRingRadius() + 0.02f);
		furthestRingPoint.y = instructionsEdgeAnchorPoint.transform.position.y;

		return furthestRingPoint;
	}
}

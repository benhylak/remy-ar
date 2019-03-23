using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using BensToolBox.AR.Scripts;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using IBM.Watson.DeveloperCloud.Services.VisualRecognition.v3;
using UnityEngine.Serialization;

public class BurnerBehaviour : MonoBehaviour
{
	public Recipe _recipeInProgress;
	
	public Burner _model;
	public Burner.BurnerPosition _position;
	
	public BurnerTimer _Timer;

	public GameObject burnerMesh;
	public BurnerRingController ring;

	public Text FloatingLabel;
	
	private bool IsLabelVisible = false;	
	private bool showInput = false;

	private bool isMonitorBoiling;

	public NotificationManager.NotificationEventHandler OnBurnerNotification;

	public State _state;
	public GazeReceiver _gazeReceiver;
	
	public void Start()
	{
		FloatingLabel.DOFade(0, 0);
		
		_state = new BurnerStateMachine.AvailableState(this);
		_gazeReceiver = GetComponent<GazeReceiver>();
	}

	public void SetTimer(TimeSpan ts)
	{  
		_state = new BurnerStateMachine.WaitingForTimerState(ts, this);
	}
	
	public void WaitForBoil()
	{
		Debug.Log("Wait for Boil");
		
		_state = new BurnerStateMachine.WaitingForBoilState(this);
	}
	
	public async void WaitForBoil(int delayMs)
	{
		await Task.Delay(delayMs);
		await new WaitForUpdate();
		
		WaitForBoil();
	}

	public void Consume()
	{
		if (_state is BurnerStateMachine.AvailableState)
		{
			_state = new BurnerStateMachine.NotAvailableState(this);
		}
		else throw new Exception($"{_model.Position} Burner is not available");
	}

	public void Release()
	{
		_state = new BurnerStateMachine.AvailableState(this);
	}
	
	// Update is called once per frame
	void Update () {
		if (FloatingLabel.isActiveAndEnabled)
		{
			FloatingLabel.transform.rotation =
				Quaternion.LookRotation(FloatingLabel.transform.position - Camera.main.transform.position);
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
		if (FloatingLabel.gameObject.activeInHierarchy)
		{
			FloatingLabel
				.DOFade(0, duration)
				.SetEase(Ease.OutSine)
				.OnComplete(() =>
				{
					FloatingLabel.text = text;
					FloatingLabel.DOFade(1, duration).SetEase(Ease.InSine);
				});
		}
		else
		{
			FloatingLabel.gameObject.SetActive(true);
			FloatingLabel.text = text;
			FloatingLabel.DOFade(1, duration);
		}
	}

	#region Display Functions -- (they don't handle state)

	public Tween HideLabel(float duration = 0.3f)
	{
		return FloatingLabel
			.DOFade(0, duration)
			.SetEase(Ease.OutSine)
			.OnComplete(() =>
			{
				FloatingLabel.gameObject.SetActive(false);
			});
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
		ring.SetRingRadius(0);
		DOTween.To(ring.GetRingRadius, ring.SetRingRadius, ring.RING_RADIUS, 1f);
				
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
		float duration = 1f;
		
		FloatingLabel.DOFade(0, duration);

		var tween = DOTween.To(ring.GetRingRadius, ring.SetRingRadius, 0f, duration)
			.SetEase(Ease.OutSine);
			
		tween.OnComplete(() =>
		{
			FloatingLabel.gameObject.SetActive(false);
		});

		return tween;
	}
	
	#endregion
}

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
	public Burner _model;
	public Burner.BurnerPosition _position;
	
	public BurnerTimer _Timer;

	public GameObject burnerMesh;
	public BurnerRingController ring;

	public bool IsLookedAt;
	public Text FloatingLabel;
	
	private bool IsLabelVisible = false;	
	private bool showInput = false;

	private bool isMonitorBoiling;

	private bool hasNotifiedForTimer = false;
	private bool hasNotifiedForBoiling = false;

	public NotificationManager.NotificationEventHandler OnBurnerNotification;

	public void Start()
	{
		FloatingLabel.DOFade(0, 0);
	}

	public void SetTimer(TimeSpan ts)
	{
		_Timer.gameObject.SetActive(true);
		_Timer.SetTimer(ts);
		_Timer.SetRingTransparency(0);

		Task.Delay(900).ContinueWith(_ =>
			{
				DOTween.To(_Timer.GetRingTransparency, _Timer.SetRingTransparency, 1f, 1.4f)
					.SetEase(Ease.InSine);
			}
		);
	}
	
	// Update is called once per frame
	void Update () {
		if (FloatingLabel.isActiveAndEnabled)
		{
			FloatingLabel.transform.rotation =
				Quaternion.LookRotation(FloatingLabel.transform.position - Camera.main.transform.position);
		}
		
		CheckForNotifications();
	}

	//TODO: notification CSM in big kahuna to handle all this!
	
	private void CheckForNotifications()
	{
		if (_Timer.isSet && _Timer.isComplete && !hasNotifiedForTimer)
		{
			hasNotifiedForTimer = true;
			
			Debug.LogWarning("Notifying...");
			
			OnBurnerNotification(new NotificationManager.Notification(
				"Your timer has finished.", 
				this,
				DismissTimer));
			//notify
		}

		if (isMonitorBoiling && _model.IsBoiling.Value && hasNotifiedForBoiling)
		{
			hasNotifiedForBoiling = true;
			
			OnBurnerNotification(new NotificationManager.Notification(
				"Your water is boiling.", 
				this,
				DismissBoiling)			
			);
		}
	}

	private void DismissBoiling()
	{
		hasNotifiedForBoiling = false;
	}

	private async void DismissTimer()
	{
		hasNotifiedForTimer = false;
		await _Timer.Reset();
	}
	
	private void SetLabel(string text)
	{
		FloatingLabel
			.DOFade(0, 0.3f)
			.OnComplete(() =>
			{
				
				FloatingLabel.text = text;
				FloatingLabel.DOFade(1, 0.3f);
			});
	}

	#region Timer Prompt Functions
	
	public void ShowInputPrompt()
	{
		SetLabel( "Timer for how long?");
		
		ring.SetVoiceLerp(0);
		ring.SetMaterialToVoiceInput();
				
		ring.SetWaveAmplitude(0f);

		DOTween.To(ring.GetVoiceLerp, ring.SetVoiceLerp, 1f, 0.3f);
		DOTween.To(ring.GetWaveAmplitude, ring.SetWaveAmplitude, ring.WAVE_AMPLITUDE, 1.5f);
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

	public void HideProactiveTimer()
	{
		FloatingLabel.DOFade(0, 1f);

		DOTween.To(ring.GetRingRadius, ring.SetRingRadius, 0f, 1f)
			.SetEase(Ease.OutSine)
			.OnComplete(() =>
			{
				FloatingLabel.gameObject.SetActive(false);
			});
	}
	#endregion
}

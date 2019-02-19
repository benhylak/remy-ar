using System;
using System.Collections;
using System.Collections.Generic;
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
	public GameObject ring;

	public float RING_RADIUS;

	public bool IsLookedAt;
	public Text FloatingLabel;
	private bool IsLabelVisible = false;
	
	[FormerlySerializedAs("VoiceInput")] 
	public Material VoiceInputMat;
	public Material WhiteProactive;
	public float WAVE_AMPLITUDE = 0.005f; 
	private bool showInput = false;
	private float totalAmt = 0;

	private Color voicePrimaryColor;
	private Color voiceSecondaryColor;
	
	private string PRIMARY_COLOR_VOICE = "_PrimaryColor";
	private string SECONDARY_COLOR_VOICE = "_SecondaryColor";

	private static readonly float MAX_VOLUME = 0.15f;

	private float lerpAmt;

	private enum DisplayState
	{
		INPUT,
		PROACTIVE,
		HIDDEN
	};

	private DisplayState _currentDisplayState;
	
	public void Start()
	{
		FloatingLabel.DOFade(0, 0);
		voicePrimaryColor = VoiceInputMat.GetColor(PRIMARY_COLOR_VOICE);
		voiceSecondaryColor = VoiceInputMat.GetColor(SECONDARY_COLOR_VOICE);
		
		_currentDisplayState = DisplayState.HIDDEN;
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
	
	public void ShowInputPrompt()
	{
		lerpAmt = 0;

		SetLabel( "Timer for how long?");
	
		ring.GetComponent<MeshRenderer>().material = VoiceInputMat;
				
		SetWaveAmplitude(0f);

		DOTween.To(GetVoiceLerp, SetVoiceLerp, 1f, 0.3f);
		DOTween.To(GetWaveAmplitude, SetWaveAmplitude, WAVE_AMPLITUDE, 1.5f);
		
		_currentDisplayState = DisplayState.INPUT;
	}

	public void SetInputLevel(float volume)
	{
		if (_currentDisplayState == DisplayState.INPUT)
		{
			var volumeMapped = Utility.Map(volume, 0, MAX_VOLUME, 0, 1, clamp: true);
			var currentLevel = ring.GetComponent<Renderer>().material.GetFloat("_Volume");
			ring.GetComponent<Renderer>().material.SetFloat("_Volume", 
				Mathf.Lerp(currentLevel, (float)volumeMapped, 0.08f));
		}
	}

	public void ShowProactiveTimer()
	{
		switch (_currentDisplayState)
		{
			case DisplayState.HIDDEN:
			{
				ring.SetActive(true);
				SetRingRadius(0);

				FloatingLabel.DOFade(1, 1.5f).SetEase(Ease.InCubic);
				DOTween.To(GetRingRadius, SetRingRadius, RING_RADIUS, 1f);

				IsLabelVisible = true;
				
				break;
			}

			case DisplayState.INPUT:
			{
				SetLabel("Set a Timer");
				
				Sequence inputToProactiveSeq = DOTween.Sequence();
				
				inputToProactiveSeq.Append(
					DOTween.To(
						GetVoiceLerp, 
						SetVoiceLerp, 
						0f, 
						0.5f));

				inputToProactiveSeq.Insert(0,
					DOTween.To(
						GetWaveAmplitude,
						SetWaveAmplitude,
						0f,
						0.5f)
				);
					
				inputToProactiveSeq.OnComplete(() =>
				{		
					ring.GetComponent<MeshRenderer>().material = WhiteProactive;
				});
				
				break;
			}
		}

		_currentDisplayState = DisplayState.PROACTIVE;
		
		Debug.Log("Proactive Timer Activated");
	}

	public void SetTimer(TimeSpan ts)
	{
		_Timer.gameObject.SetActive(true);
		_Timer.SetRingTransparency(0);
		_Timer.Reset();
		_Timer.SetTimer(ts);

		DOTween.To(_Timer.GetRingTransparency, _Timer.SetRingTransparency, 1f, 5f)
			.SetEase(Ease.Linear)
			.SetDelay(0.5f);
	}

	public void HideProactiveTimer()
	{
		FloatingLabel.DOFade(0, 1.5f);
		DOTween.To(GetRingRadius, SetRingRadius, 0f, 1f)
			.SetEase(Ease.OutSine);

		IsLabelVisible = false;

		_currentDisplayState = DisplayState.HIDDEN;
	}

	// Update is called once per frame
	void Update () {
		if (IsLabelVisible)
		{
			FloatingLabel.transform.rotation =
				Quaternion.LookRotation(FloatingLabel.transform.position - Camera.main.transform.position);
		}
	}

	private void SetWaveAmplitude(float amt)
	{
		ring.GetComponent<Renderer>().material.SetFloat("_WaveAmp", amt);
	}

	private float GetWaveAmplitude()
	{
		return ring.GetComponent<Renderer>().material.GetFloat("_WaveAmp");
	}
	
	
	private void SetRingRadius(float radius)
	{
		ring.GetComponent<Renderer>().material.SetFloat("_Radius", radius);
	}
	
	private float GetRingRadius()
	{
		return ring.GetComponent<Renderer>().sharedMaterial.GetFloat("_Radius");
	}
	
	
	private void SetVoiceLerp(float amt)
	{
		lerpAmt = amt;
		
		ring.GetComponent<MeshRenderer>().material.SetColor(PRIMARY_COLOR_VOICE, Color.Lerp(Color.white, voicePrimaryColor, amt));
		ring.GetComponent<MeshRenderer>().material.SetColor(SECONDARY_COLOR_VOICE, Color.Lerp(Color.white, voiceSecondaryColor, amt));
	}

	private float GetVoiceLerp()
	{
		return lerpAmt;
	}


}

using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine.UI;

public class StoveTimer : MonoBehaviour
{
	private DrawCircle _circleRenderer;
	private LineRenderer _lineRenderer;
	
	[SerializeField]
	private TimeSpan _timerGoal = TimeSpan.Zero;
	private float _setTime;

	private float _previousSeconds;
	[SerializeField]
	private int _seconds;

	private float glow = 0.7f;

	public GameObject _pillLabel;

	private Text _labelText;
	
	// Use this for initialization
	void Start ()
	{
		_circleRenderer = GetComponentInChildren<DrawCircle>();
		_lineRenderer = _circleRenderer.GetComponent<LineRenderer>();
		
		_lineRenderer.SetTransparency(1.2f);

		_labelText = _pillLabel.GetComponentInChildren<Text>();
	}

	public void SetTimer(TimeSpan timeSpan)
	{
		Reset();

		_setTime = Time.time;
		_timerGoal = timeSpan;

//		_circleRenderer.GetComponent<LineRenderer>().materials[0]	
//			.DOFade(
//				0.4f,
//				0.7f)
//			.SetLoops(-1, LoopType.Yoyo)
//			.SetEase(Ease.InQuad);
	}

	void UpdatePill()
	{
		Vector3 direction = Camera.main.transform.position - _pillLabel.transform.position;
		Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
		
		_pillLabel.transform.rotation = Quaternion.Slerp(_pillLabel.transform.rotation, lookRot, 4*Time.deltaTime);
		
		if(_timerGoal == TimeSpan.Zero) return;
		
		TimeSpan remaining = new TimeSpan(0, 0, (int)(_timerGoal.TotalSeconds - (Time.time - _setTime)));

		string timeToStr = "";
		
		if (remaining.TotalSeconds <= 0)
		{
			timeToStr = "Done";
		}
		else
		{		
			var minutePrefix = remaining.Minutes < 10 ? "0" : "";
			var secondPrefix = remaining.Seconds < 10 ? "0" : "";
			
			timeToStr = minutePrefix + remaining.Minutes + ":" + secondPrefix + remaining.Seconds;
		}

		_labelText.text = timeToStr;
	}
	void Update ()
	{
		if (_previousSeconds != _seconds)
		{
			_previousSeconds = _seconds;
			SetTimer(new TimeSpan(0, 0, _seconds));
		}

		float progress = (Time.time - _setTime) / (float)_timerGoal.TotalSeconds;
		
		UpdatePill();
		
		if (_timerGoal == TimeSpan.Zero || progress <= 0) return;
		
		if (progress >= 1)
		{
			//timer is done. Congrats!
			var transitionSeq = DOTween.Sequence();

			transitionSeq.Append(
				DOTween.To(_lineRenderer.GetTransparency, _lineRenderer.SetTransparency,
					0f, 0.3f));

			transitionSeq.Append(
				_lineRenderer
					.material
					.DOColor(
						Color.red, 0.2f)
					.OnUpdate(() =>
						{
							_lineRenderer.SetTransparency(0f);
							_labelText.color = Color.red;
						}
					));

			transitionSeq.Append(
				DOTween.To(_lineRenderer.GetTransparency, _lineRenderer.SetTransparency,
					1f, 0.35f)
					.SetEase(Ease.InQuad)
			);
			
			
			transitionSeq.AppendInterval(0.35f);

			transitionSeq.OnComplete(() =>
			{

				var finishedSequence = DOTween.Sequence();

				finishedSequence.AppendInterval(0.35f);
				finishedSequence.Append(
					DOTween.To(_lineRenderer.GetTransparency, _lineRenderer.SetTransparency,
						0.3f, 0.6f)
						.SetEase(Ease.InQuad)
					);

				finishedSequence
					.SetLoops(-1, LoopType.Yoyo)
					.Play();
			}
			);

			transitionSeq.Play();
							
			_timerGoal = TimeSpan.Zero;
		}
		else
		{
			_circleRenderer.SetPercentFilled(progress);
		}
	}

	public void SetLineGlow(float newGlow)
	{
		glow = newGlow;
		_lineRenderer.material.SetEmissionGlow(glow);
	}

	public float GetLineGlow()
	{
		return glow;
	}

	public void Reset()
	{
		_circleRenderer.SetPercentFilled(0);
	}
	
	// Update is called once per frame
	
}

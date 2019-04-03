using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BensToolBox.AR.Scripts;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine.UI;
using UnityAsyncAwaitUtil;
using static IEnumeratorAwaitExtensions;

public class BurnerTimer : MonoBehaviour
{
	public DrawCircle _circleRenderer;
	public LineRenderer _lineRenderer;
	
	[SerializeField]
	private TimeSpan _timerGoal = TimeSpan.Zero;
	private float _setTime;

	private float _previousSeconds;
	[SerializeField]
	private int _seconds;

	private float glow = 0.7f;

	public GameObject _pillLabel;

	public Text _labelText;

	public GameObject torus;

	private float ringTransparency;
	private float pillTransparency;

	private float _progress;
	private float RING_END_TRANSPARENCY = .05f;

	public bool isSet => _timerGoal != TimeSpan.Zero;

	public bool isComplete => _progress >= 1;

	private Sequence _timerDoneSequence;

	private Camera _mainCamera;

	// Use this for initialization
	void Start ()
	{
		_lineRenderer
			.material
			.SetColor("_Color",RemyColors.RED);
		_lineRenderer.SetTransparency(0);
		
		SetPillTransparency(0);
		
		_mainCamera = Camera.main;
	}

	public async void SetTimer(TimeSpan timeSpan)
	{
		await Reset();

		_setTime = Time.time;
		_timerGoal = timeSpan;

		Debug.Log("Set timer called");
	}

	public void SetTransparency(float val)
	{
		SetPillTransparency(val);

		ringTransparency = val;

		_labelText.CrossFadeAlpha(val, 0, true);
		torus.GetComponent<MeshRenderer>().material.SetTransparency(Mathf.Lerp(0f, RING_END_TRANSPARENCY, val));
		_lineRenderer.SetTransparency(val);
		
//		_labelText.color = new Color(
//			_labelText.color.r, 
//			_labelText.color.g, 
//			_labelText.color.b, 
//			val);
	}
	
	
	public float GetTransparency()
	{
		return ringTransparency;
		//	return 
	}

	public void SetPillTransparency(float val)
	{
		pillTransparency = val;
		
		foreach (var r in _pillLabel.GetComponentsInChildren<MeshRenderer>(true))
		{
			r.material.SetTransparency(val);
		}
	}

	public float GetPillTransparency()
	{
		return pillTransparency;
	}


	void UpdatePill()
	{
		Vector3 direction = _mainCamera.transform.position - _pillLabel.transform.position;
		Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
		
		_pillLabel.transform.rotation = Quaternion.Slerp(_pillLabel.transform.rotation, lookRot, 4*Time.deltaTime);
		
		if(_timerGoal == TimeSpan.Zero) return;

		string timeToStr = "";
		
		if (_progress >= 1)
		{
			timeToStr = "Done";
		}
		else
		{				
			TimeSpan remaining = new TimeSpan(0, 0, (int)(_timerGoal.TotalSeconds - (Time.time - _setTime)));

			var minutePrefix = remaining.Minutes < 10 ? "0" : "";
			var secondPrefix = remaining.Seconds < 10 ? "0" : "";
			
			timeToStr = minutePrefix + remaining.Minutes + ":" + secondPrefix + remaining.Seconds;
		}

		_labelText.text = timeToStr;
	}

	public bool isDone()
	{
		return isSet && isComplete;
	}
	
	void Update ()
	{
		if (_timerGoal == TimeSpan.Zero) return;
		
		_progress = (Time.time - _setTime)*1000 / (float)_timerGoal.TotalMilliseconds;
		UpdatePill();
		
		if( _progress <= 0) return;
			
		if (_progress >= 1 && _timerDoneSequence == null)
		{
			//timer is done. Congrats!
			_timerDoneSequence = DOTween.Sequence();

			_timerDoneSequence.Append(
				DOTween.To(_lineRenderer.GetTransparency, _lineRenderer.SetTransparency,
					0f, 0.3f));

			_timerDoneSequence.Append(
				_lineRenderer
					.material
					.DOColor(
						RemyColors.GREEN, 0.2f)
					.OnUpdate(() =>
						{
							_lineRenderer.SetTransparency(0f);
							_labelText.color = Color.green;
						}
					));

			_timerDoneSequence.Append(
				DOTween.To(_lineRenderer.GetTransparency, _lineRenderer.SetTransparency,
					1f, 0.35f)
					.SetEase(Ease.InQuad)
			);
			
			
			_timerDoneSequence.AppendInterval(0.35f);

			_timerDoneSequence.OnComplete(() =>
			{

				var finishedSequence = DOTween.Sequence();

				finishedSequence.AppendInterval(0.2f);
				finishedSequence.Append(
					DOTween.To(_lineRenderer.GetTransparency, _lineRenderer.SetTransparency,
						0.15f, 0.6f)
						.SetEase(Ease.InQuad)
					);

				finishedSequence
					.SetLoops(-1, LoopType.Yoyo)
					.Play();
			}
			);

			_timerDoneSequence.Play();
		}
		else if (_progress <= 1)
		{
			_circleRenderer.SetPercentFilled(_progress);
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

	public IEnumerator WaitForKill()
	{
		yield return _timerDoneSequence.WaitForKill();
	}

	public IEnumerator WaitForCompletion(Tween tween)
	{
		yield return tween.WaitForCompletion();
	}

	public async Task Show()
	{
		await WaitForCompletion(DOTween.To(GetTransparency,
				SetTransparency,
				1f,
				0.5f)
			.SetEase(Ease.InSine));
	}

	public async Task Hide()
	{
		await WaitForCompletion(DOTween.To(GetTransparency, SetTransparency, 0f, 0.3f).SetEase(Ease.OutSine));
	}

	public async Task Reset()
	{
		if (_timerDoneSequence != null && _timerDoneSequence.active)
		{
			_timerDoneSequence.Kill(true);
			await WaitForKill();
		}
		
		await Hide();
		
		_lineRenderer
			.material
			.SetColor("_Color",
				RemyColors.RED);
		
		_lineRenderer.SetTransparency(0);

		_circleRenderer.SetPercentFilled(0);
		_timerGoal = TimeSpan.Zero;
		_progress = 0;
	
	}
	// Update is called once per frame
	
}

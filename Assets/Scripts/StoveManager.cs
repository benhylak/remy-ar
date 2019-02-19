using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoveManager : MonoBehaviour
{
	[SerializeField]
	private MLImageTrackerBehavior marker;

	private bool _hasBeenInit = false;

	public List<BurnerBehaviour> _burners;

	public List<GameObject> _orbs;

	public float _orbEleveation = 0.25f;

	public Totem _Totem;

	public String RecognizedText = "";

	private bool orbsEnabled = false;

	private bool fading = false;

	private float fadingStart = 0;

	private float transitionTime = 6f;

	private GameObject fadingIndicator;

	private Vector3 defaultScale;

	public SpeechRecognizer streamer;

	private bool recognizing = false;

	private GameObject mesh;

	private State _state;
	
	private enum State
	{
		NONE,
		SELECT,
		FADING,
		RECOG
	};

	[SerializeField]
	private Image mic;
	
	//private Totem _totem;
	 
	// Use this for initialization
	void Start () {
		DisableOrbs();

		MLInput.OnTriggerDown += TriggerDown;
		MLInput.OnControllerButtonUp += ButtonDown;
		
		mic.DOFade(0, .3f);
	}

	void ButtonDown(byte id, MLInputControllerButton button)
	{
		if (button == MLInputControllerButton.Bumper)
		{
			SceneManager.LoadScene(1);
		}
		else if (button == MLInputControllerButton.HomeTap)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	void TriggerDown(byte id, float amt)
	{
		if (_state == State.RECOG)
		{
			int i = _orbs.IndexOf(fadingIndicator);
			
			_burners[i]._Timer.gameObject.SetActive(true);
			_burners[i]._Timer.SetTimer(new TimeSpan(0, 5, 0));

			mic.DOFade(0, .3f);

			_state = State.NONE;

			mesh.transform.DOScale(Vector3.zero, 0.3f);
		}
		else DisplaySelectors(0, 0);
	}
	
	void DisableOrbs()
	{
		orbsEnabled = false;
		
		foreach (var orb in _orbs)
		{
			orb.SetActive(false);
		}

		defaultScale = _orbs[0].GetComponentInChildren<MeshRenderer>().gameObject.transform.localScale;
	}
	
	void EnableOrbs()
	{
		foreach (var orb in _orbs)
		{
			orb.SetActive(true);
		}
	}

	public void DisplaySelectors(byte id, float amt)
	{
		EnableOrbs();

		_state = State.SELECT;
		
		for (int i = 0; i < _burners.Count; i++)
		{
			_orbs[i].transform.position = _Totem.transform.position;
			
			var burner = _burners[i];
			var orbPos = burner.transform.position + new Vector3(0, _orbEleveation, 0);

			_orbs[i].transform.GetComponentInChildren<MeshRenderer>().gameObject
				.transform.localScale = defaultScale;
			
			_orbs[i].transform
				.DOMove(orbPos, 0.8f)
				.SetEase(Ease.InQuad);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{

		if (marker.IsTracking)
		{
			if (!_hasBeenInit)
			{
				_hasBeenInit = true;
				transform.position = marker.gameObject.transform.position;
				transform.rotation = marker.gameObject.transform.rotation;
			}
			else
			{
				transform.position = Vector3.Lerp(transform.position, marker.gameObject.transform.position, Time.deltaTime);
				transform.rotation = Quaternion.Slerp(transform.rotation, marker.gameObject.transform.rotation, Time.deltaTime);
			}	
		}
		
		switch (_state)
		{
			case State.NONE:
				
				break;
			
			case State.SELECT:
						
				RaycastHit hit;

				if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
				{
					if (hit.collider.gameObject.tag == "Indicator")
					{
						fadingStart = Time.time;
						fadingIndicator = hit.collider.gameObject;
						mesh = fadingIndicator.GetComponentInChildren<MeshRenderer>().gameObject;

						_state = State.FADING;
					}
				}

				break;
			
			case State.FADING:

				if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
				{
					if (hit.collider.gameObject.tag == "Indicator")
					{

						if (Time.time - fadingStart > 1.2f)
						{
							mesh.transform.localScale = Vector3.Lerp(mesh.transform.localScale,
								Vector3.zero,
								(Time.time - fadingStart - 1f) / (transitionTime - 1f));
						}

						if (Time.time - fadingStart > transitionTime)
						{
							mic.DOFade(1, .3f);
							_state = State.RECOG;

							mesh.transform.DOScale(defaultScale, 0.3f);
							FadeAllIndicatorsButActive();
						}

						break;
					}
				}
				
				mesh.transform.DOScale(defaultScale, 0.3f).SetEase(Ease.InQuad);
				_state = State.SELECT;

				break;
				
			case State.RECOG:
				
				
				
				break;
		}

	}
	
	void OnDestroy()
	{
		MLInput.OnControllerButtonDown -= ButtonDown;
		MLInput.OnTriggerDown -= DisplaySelectors;
	}

	public void FadeAllIndicatorsButActive()
	{
		foreach (var orb in _orbs)
		{
			if(orb == fadingIndicator) continue;
			
			var m = orb.GetComponentInChildren<MeshRenderer>().gameObject;
			
			m.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InQuad);
		}
	}
}

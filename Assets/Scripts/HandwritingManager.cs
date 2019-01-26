using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using DG;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.MagicLeap;

public class HandwritingManager : MonoBehaviour {

	public enum WritingState {DISABLED, NONE, APPROACHING, WRITING, WAITING}

	public float MIN_APPROACH_RANGE = 0.5f;
	public float MIN_WRITING_RANGE = 0.07f;
	public float MAX_WRITING_DIST = 0.13f;
	
	private WritingState _writingState;
	private MLHand _activeHand;

	private GameObject _thisTrailRenderer;

	[SerializeField] private GameObject _trailRendererPrefab;

	[SerializeField] private GameObject _halo;

	private Vector3 smoothedPosition = Vector3.zero;

	private GameObject _writingSurface;

	private SpriteRenderer _haloSprite;
	
	// Use this for initialization
	void Start ()
	{
		_writingState = WritingState.NONE;

		_haloSprite = _halo.GetComponentInChildren<SpriteRenderer>();

		MLInput.OnControllerButtonDown += ButtonDown;
	}
		
	void ButtonDown(byte id, MLInputControllerButton btn)
	{
		if (btn == MLInputControllerButton.Bumper)
		{
			SceneManager.LoadScene(0);
		}
	}

	// Update is called once per frame
	void Update () {
		
		switch (_writingState)
		{
			case WritingState.NONE:
				
				if (_thisTrailRenderer != null)
				{
					//_thisTrailRenderer.GetComponentInChildren<MeshRenderer>().gameObject.SetActive(false);
					_thisTrailRenderer = null;
				}
				
				_activeHand = null;
				smoothedPosition = Vector3.zero;

				if (IsApproachingDrawingSurface(MLHands.Right) ||
				    IsApproachingDrawingSurface(MLHands.Left))
				{
					EaseInHalo();
					_writingState = WritingState.APPROACHING;
				}
				
				break;
			
			case WritingState.APPROACHING:
						
				UpdateSmoothedFingerPosition();
				UpdateApproachIndicator(); // TODO decide when to change this state back to None
				
				break;
			
			case WritingState.WRITING:
						
				UpdateSmoothedFingerPosition();
				UpdateDrawing();

				break;
			
			default:
				
				break;			
				
		}
	}
	bool IsApproachingDrawingSurface(MLHand hand)
	{
		//raycast down
		//record distance
		//place something
		RaycastHit hit;

		List<MLKeyPoint> drawingKeypoints = hand.Index.KeyPoints;
		MLKeyPoint stylus = drawingKeypoints.Last();

		if (Physics.Raycast(stylus.Position, Vector3.down, out hit) && hit.distance < MIN_APPROACH_RANGE)
		{
			_activeHand = hand;
			return true;
		}
		
		return false;
	}

	void UpdateSmoothedFingerPosition()
	{
		if (_activeHand != null)
		{
			List<MLKeyPoint> drawingKeypoints = _activeHand.Index.KeyPoints;
			Vector3 currentPosition = drawingKeypoints.First().Position;

			if (smoothedPosition == Vector3.zero) smoothedPosition = currentPosition;
			else
			{
				smoothedPosition = Vector3.Lerp(smoothedPosition, currentPosition, 6.45f * Time.deltaTime);
			}
		}
	}

	void EaseInHalo()
	{
		DOTween.To(_haloSprite.GetTransparency, 
				_haloSprite.SetTransparency,
				0.35f,
				0.3f)
			.SetEase(Ease.InQuad);   
	}
	
	void OnDestroy()
	{
		MLInput.OnControllerButtonDown -= ButtonDown;
	}
	
	void UpdateDrawing()
	{
		RaycastHit hit;
		
		if (Physics.Raycast(smoothedPosition, Vector3.down, out hit, MAX_WRITING_DIST, 1 << LayerMask.NameToLayer("Drawable")))
		{
			if (_thisTrailRenderer == null)
			{
				_thisTrailRenderer = Instantiate(_trailRendererPrefab);
				_writingSurface = hit.collider.gameObject;
			}
			
			var surfacePos = smoothedPosition;
			surfacePos.y = _writingSurface.transform.position.y;

			_thisTrailRenderer.transform.position = surfacePos;
		}
		else
		{
			_writingState = WritingState.NONE;
		}
	}

	void UpdateApproachIndicator()
	{
		//raycast down
		//record distance
		//place something

		RaycastHit hit;
		
		if (Physics.Raycast(smoothedPosition, Vector3.down, out hit, MIN_APPROACH_RANGE, 1 << LayerMask.NameToLayer("Drawable")))
		{
			if (hit.distance < MIN_WRITING_RANGE)
			{
				_writingState = WritingState.WRITING;
				
				DOTween.To(_haloSprite.GetTransparency, 
						_haloSprite.SetTransparency,
						0f,
						0.3f)
					.SetEase(Ease.OutQuad);   
			}
			else
			{
				var normalizedDist = Mathf.InverseLerp(MIN_APPROACH_RANGE, 0f, hit.distance);
				var scale = Mathf.Lerp(1f, 0.1f, normalizedDist);
				var transparency = Mathf.Lerp(0.35f, 1f, normalizedDist);
				
				_haloSprite.SetTransparency(transparency);
			 
				_halo.transform.position = hit.point;
				_halo.transform.up = hit.normal;
				_halo.transform.localScale = new Vector3(scale,scale, scale);
			}		
		}
		else
		{		
			_writingState = WritingState.NONE;
		}
	}
}

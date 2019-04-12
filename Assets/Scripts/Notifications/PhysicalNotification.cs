using System.Collections.Generic;
using System.Threading.Tasks;
using BensToolBox.AR.Scripts;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PhysicalNotification : NotificationBehaviour
{
	[SerializeField]
	private SpriteRenderer labelImage;

	[SerializeField]
	private SpriteRenderer spotImage;
 
	public MeshRenderer mesh;

	public GameObject meshParent; 
	
	private Vector3 _spotEndScale;

	private float _raisedYVal = 0.019f;

	private float _sittingYVal = 0f;

	private Sequence _diminishSeq;

	private Tween _showBounceTween;

	private float rotationSpeed;

	private float MAX_ROTATION_SPEED = 45f;

	private Camera mainCamera;
	
	// Use this for initialization
	public virtual void Start ()
	{
		_spotEndScale = spotImage.transform.localScale;
		mainCamera = Camera.main;
		
		base.Start();
	}
	
	// Update is called once per frame
	public virtual void Update ()
	{
		if (_state != null)
			_state = _state.Update() ?? _state;
	}

	public override void UpdateShow()
	{
		labelImage.transform.rotation = Quaternion.Slerp(
			labelImage.transform.rotation,
			Quaternion.LookRotation(labelImage.transform.position-mainCamera.transform.position, Vector3.up),
			Time.deltaTime);

	//	rotationSpeed = Mathf.Lerp(rotationSpeed, MAX_ROTATION_SPEED, 3*Time.deltaTime);
		
	//	meshParent.transform.rotation *= Quaternion.Euler(Vector3.up * rotationSpeed* Time.deltaTime);
	}
	
	public override async Task Launch(int delay = 1000)
	{	spotImage.DOFade(0, 0);
		labelImage.DOFade(0, 0);
		spotImage.transform.localScale = 0.1f * _spotEndScale;
		meshParent.transform.SetLocalPosY(_raisedYVal);

		mesh.enabled = true;
		
		foreach (var mat in mesh.materials)
		{
			mat.SetTransparency(0);
		}

		await Task.Delay(delay);
        
		_state = new ShowState(this);
	}
	
	public override void HideToShow()
	{	
		var showSeq = DOTween.Sequence();
		
		 
		_showBounceTween = meshParent.transform.DOScale(0.95f, 0.65f).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo);

		showSeq.Append(
			spotImage.DOFade(0.8f, 0.3f)
				.SetEase(Ease.InQuad));

		showSeq.Insert(0,
			spotImage.transform.DOScale(_spotEndScale, 0.3f)
				.SetEase(Ease.OutBounce));

//		showSeq.Insert(0.5f,
//			mesh.transform.DOLocalMoveY(_raisedYVal, 0.75f)
//				.SetEase(Ease.InCubic));
		
		showSeq.Insert(0.3f,
			labelImage.DOFade(0.8f, 0.5f)
				.SetEase(Ease.InQuad));

		foreach (var mat in mesh.materials)
		{
			showSeq.Insert(0.45f,
					mat.DOFade(1f, 1.5f))
				.SetEase(Ease.InQuint);
		}		
	}

	public override void ShowToDiminish()
	{
		//rotationSpeed = 0;

	//var time = meshParent.transform.rotation.eulerAngles.y / (MAX_ROTATION_SPEED / 2);

		//meshParent.transform.DORotate(Vector3.zero, time).SetEase(Ease.OutQuad);
		
		_showBounceTween.Kill(true);
		
		_diminishSeq = DOTween.Sequence();

		var dist = Vector3.Magnitude(meshParent.transform.rotation.eulerAngles);
	
		_diminishSeq.Append(
			spotImage.DOFade(0f, 0.5f)
				.SetEase(Ease.OutCubic));
		
		_diminishSeq.Insert(0,
			labelImage.DOFade(0f, 0.5f)
				.SetEase(Ease.OutCubic));
		
		_diminishSeq.Insert(0,
			meshParent.transform.DOLocalMoveY(_sittingYVal, 0.65f)
				.SetEase(Ease.OutCubic));	
	}

	public override  void DiminishToShow()
	{
		_diminishSeq = DOTween.Sequence();		
		
		_diminishSeq.Append(
			spotImage.DOFade(1f, 0.8f)
				.SetEase(Ease.OutCubic));
		
		_diminishSeq.Insert(0,
			labelImage.DOFade(0.8f, 0.8f)
				.SetEase(Ease.OutCubic));
		
		_diminishSeq.Insert(0,
			meshParent.transform.DOLocalMoveY(_raisedYVal, 0.9f)
				.SetEase(Ease.OutCubic));	
	}

	public override Tween Hide()
	{
		spotImage.DOFade(0, 0.3f);
		labelImage.DOFade(0, 0.3f);
		spotImage.transform.DOScale(0.1f * _spotEndScale.x, 0.3f);
		mesh.enabled = false;

//		meshParent.transform.SetLocalPosY(_raisedYVal);

		return labelImage.DOFade(0, 0.3f);
	}
}

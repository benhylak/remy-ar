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

	[SerializeField] 
	private MeshRenderer mesh;

	private Vector3 _spotEndScale;

	private float _raisedYVal = 0.016f;

	private float _sittingYVal = 0f;

	private Sequence _diminishSeq;
	
	// Use this for initialization
	void Start ()
	{
		_spotEndScale = spotImage.transform.localScale;
		
		base.Start();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_state != null)
			_state = _state.Update() ?? _state;
	}
	
	public override async void Launch()
	{	spotImage.DOFade(0, 0);
		labelImage.DOFade(0, 0);
		spotImage.transform.localScale = 0.1f * _spotEndScale;
		mesh.transform.SetLocalPosY(_raisedYVal);
		
		foreach (var mat in mesh.materials)
		{
			mat.SetTransparency(0);
		}

		await Task.Delay(1000);
        
		_state = new ShowState(this);
	}
	
	public override void HideToShow()
	{	
		var showSeq = DOTween.Sequence();

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
		_diminishSeq = DOTween.Sequence();		
		
		_diminishSeq.Append(
			spotImage.DOFade(0f, 0.5f)
				.SetEase(Ease.OutCubic));
		
		_diminishSeq.Insert(0,
			labelImage.DOFade(0f, 0.5f)
				.SetEase(Ease.OutCubic));
		
		_diminishSeq.Insert(0,
			mesh.transform.DOLocalMoveY(_sittingYVal, 0.4f)
				.SetEase(Ease.OutCubic));	
	}

	public override  void DiminishToShow()
	{
		_diminishSeq = DOTween.Sequence();		
		
		_diminishSeq.Append(
			spotImage.DOFade(1f, 0.5f)
				.SetEase(Ease.OutCubic));
		
		_diminishSeq.Insert(0,
			labelImage.DOFade(0.8f, 0.5f)
				.SetEase(Ease.OutCubic));
		
		_diminishSeq.Insert(0,
			mesh.transform.DOLocalMoveY(_raisedYVal, 0.5f)
				.SetEase(Ease.OutCubic));	
		
		Debug.LogError("Diminish -> Show");
	}
    
	public override  Tween Hide()
	{
//		return transform
//			.DOLocalMoveX(START_X, 0.6f)
//			.SetEase(Ease.OutQuad);

		return null;
	}
}

using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Plugins.Options;
using UnityEngine;

[ExecuteInEditMode]
public class LineRendererWidthAdjuster : MonoBehaviour
{
	public LineRenderer _lineRenderer;

	private float _initialWidth;

	private Vector3 _initialScale;

	private Vector3 _previousScale;
	
	// Use this for initialization
	private void Awake()
	{
		_initialWidth = _lineRenderer.widthMultiplier;
		_initialScale = transform.localScale;
	}

	// Update is called once per frame
	void Update () {
		if (isActiveAndEnabled && transform.localScale != _previousScale)
		{
			var changePercent = transform.localScale.magnitude / _initialScale.magnitude;

			_lineRenderer.widthMultiplier = changePercent * _initialWidth;

			_previousScale = transform.localScale;
		}
	}
}

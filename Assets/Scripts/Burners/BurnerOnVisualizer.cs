using System.Collections;
using System.Collections.Generic;
using BensToolBox.AR.Scripts;
using DG.Tweening;
using UnityEngine;

public class BurnerOnVisualizer : MonoBehaviour
{
    private Renderer _renderer;
    public Renderer _edgeRenderer;
    private Sequence _pulseSequence;
  
    private static readonly int Color = Shader.PropertyToID("_Color");
    private static readonly int RimColor = Shader.PropertyToID("_RimColor");

    private bool _isActive;

    public bool IsActive => _isActive;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        
        _renderer.material.SetColor(Color, RemyColors.RED);
        _renderer.material.SetColor(RimColor, RemyColors.RED_RIM);
        
        _edgeRenderer.material.SetColor(Color, RemyColors.RED);
        _edgeRenderer.material.SetColor(RimColor, RemyColors.RED_RIM);
        
        _renderer.SetTransparency(0);
        _edgeRenderer.SetTransparency(0);
    }

    public void Show()
    {
        if (!_isActive)
        {
            _edgeRenderer.material.DOFade(1f, 0.45f);
            _renderer.material.DOFade(0.65f, 0.45f)
                .OnComplete(() =>
                {
                    _pulseSequence = DOTween.Sequence();

                    _pulseSequence.AppendInterval(0.35f);
                    
                    _pulseSequence.Append(
                        _renderer.material.DOFade(0f, 0.45f).SetEase(Ease.InOutSine)
                    );
    
                    _pulseSequence.SetLoops(-1, LoopType.Yoyo);
                    _pulseSequence.Play();
                    
                    _isActive = true;
                });
        }
    }

    public async void Hide()
    {
        if (_isActive)     
        {
            _pulseSequence.Kill(true);
            await _pulseSequence.WaitForKill();

            _renderer.material.DOFade(0, 0.25f).SetEase(Ease.InSine);
            _edgeRenderer.material.DOFade(0, 0.25f).SetEase(Ease.InSine);
                 
            _isActive = false;
        }
    }
}

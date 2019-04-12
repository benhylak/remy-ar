using System.Collections.Generic;
using System.Threading.Tasks;
using BensToolBox.AR.Scripts;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpNotification : NotificationBehaviour
{
    private static readonly float END_X = 0.202f;
    private static readonly float START_X = 0.68f;
    private static readonly float MID_X = 0.4f;
  
    public Text label;
 
    public override async Task Launch(int delay = 1000)
    {
        transform.SetLocalPosX(START_X);
        label.DOFade(1, 0);
        await Task.Delay(delay);
        
        _state = new NotificationBehaviour.ShowState(this);
    }

    private void Start()
    {
        transform.SetLocalPosX(START_X);
        base.Start();
    }

    public void SetText(string text)
    {
        label.text = text;
    }

    public override void ShowToDiminish()
    {
        label.DOFade(0, 0.6f).SetEase(Ease.OutCubic);

        transform
            .DOLocalMoveX(MID_X, 0.6f)
            .SetEase(Ease.OutCubic);
    }

    public override void DiminishToShow()
    {
        label.DOFade(1, 0.6f).SetEase(Ease.OutCubic);
        transform
            .DOLocalMoveX(END_X, 0.7f)
            .SetEase(Ease.OutCubic);
    }

    public override void HideToShow()
    {
        transform
            .DOLocalMoveX(END_X, 0.7f)
            .SetEase(Ease.OutCubic);
    }
    
    public override Tween Hide()
    {
        return transform
            .DOLocalMoveX(START_X, 0.6f)
            .SetEase(Ease.OutQuad);
    }

    public void Update()
    {
        if(_state != null)
            _state = _state.Update() ?? _state;
    }
}

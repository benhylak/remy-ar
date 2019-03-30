using System;
using DG.Tweening;
using UnityEngine;

namespace Burners.States
{
    public static class BoilStates
    {
        public class WaitingForBoilState : BurnerStateMachine.BurnerState
        {
            private Func<BurnerStateMachine.BurnerState> _doneStateBuilder;
            
            public WaitingForBoilState(BurnerBehaviour burner, Func<BurnerStateMachine.BurnerState> onDone) : base(burner)
            {   
                _burnerBehaviour.ring.gameObject.SetActive(true);
                _burnerBehaviour.ring.SetMaterialToIndeterminate();
                _burnerBehaviour.ring.SetAlpha(0);
                _burnerBehaviour.SetLabel("Waiting to Boil", 0.6f);
                _burnerBehaviour.ring.Show(0.6f);
                _doneStateBuilder = onDone;
            }

            public override State Update()
            {
                if (_burnerBehaviour._model.IsBoiling.Value)
                {
                    _burnerBehaviour.OnBurnerNotification(
                        new NotificationManager.Notification("Your Pot is Boiling",
                            _burnerBehaviour));
                    
                    return _doneStateBuilder.Invoke();
                }

                return null;
            }
        }
     
        
        public class BoilDoneTimerState : BurnerStateMachine.BurnerState
        {            
            public BoilDoneTimerState(BurnerBehaviour burner) : base(burner)
            {       
                _burnerBehaviour.ring.StartPulsing(RemyColors.RED, RemyColors.RED_RIM);
                _burnerBehaviour.SetLabel("Done");
            }

            public override State Update()
            {
                if (!_burnerBehaviour._model.IsPotDetected.Value)
                {
                    _burnerBehaviour.ring.StopPulsing().OnComplete( ()=>
                    {
                        _burnerBehaviour.ring.Hide()
                            .OnComplete(() => { _burnerBehaviour.ring.SetColor(Color.white); });

                        //TODO: NOT WORKING
                    });

                    return new BurnerStateMachine.AvailableState(_burnerBehaviour);
                }

                return null;
            }
        }
    }
}
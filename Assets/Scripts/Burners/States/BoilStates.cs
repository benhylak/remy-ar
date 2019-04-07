using System;
using DG.Tweening;
using UnityEngine;

namespace Burners.States
{
    public static class BoilStates
    {
        public class WaitingForBoilState : BurnerStates.IndeterminateState
        {
            public WaitingForBoilState(BurnerBehaviour burner, Func<BurnerStates.BurnerState> onDone) : base(burner, "Water is Boiling")
            {
                _burnerBehaviour.SetLabel("Waiting to Boil", 0.6f);
                _isDone = () => _burnerBehaviour.IsBoiling();
                _onFinished = onDone;
            }
        }
     
        
        public class BoilDoneTimerState : BurnerStates.BurnerState
        {            
            public BoilDoneTimerState(BurnerBehaviour burner) : base(burner)
            {       
                _burnerBehaviour.ring.StartPulsing(RemyColors.RED, RemyColors.RED_RIM);
                _burnerBehaviour.SetLabel("Done");
                _burnerBehaviour.PlayDoneWaitingSound();
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

                    return new BurnerStates.AvailableState(_burnerBehaviour);
                }

                return this;
            }
        }
    }
}
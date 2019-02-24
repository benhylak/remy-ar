using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public static class BurnerStateMachine
{
    public abstract class BurnerState : State
    {
        protected BurnerBehaviour _burnerBehaviour;
        
        protected BurnerState(BurnerBehaviour burner)
        {
            _burnerBehaviour = burner;
        }
    }
    
    public class WaitingState : BurnerState
    {
        public WaitingState(BurnerBehaviour burner) : base(burner) {}

        public override State Update()
        {
            return null;
        }
    }

    public class WaitingForBoilState : BurnerState
    {
        public WaitingForBoilState(BurnerBehaviour burner) : base(burner)
        {
            _burnerBehaviour.HideProactiveTimer().OnComplete(() =>
            {
                _burnerBehaviour.ring.SetMaterialToBoiling();
                _burnerBehaviour.ring.SetAlpha(0);
                
                _burnerBehaviour.SetLabel("Waiting to Boil");

                _burnerBehaviour.ring.Show(0.5f);
            }); 
        }

        public override State Update()
        {
            if (_burnerBehaviour._model.IsBoiling.Value)
            {
                return new BoilDoneState(_burnerBehaviour);
            }

            return null;
        }
        
    }
    
    public class BoilDoneState : BurnerState
    {
        private Sequence _pulseRedSequence;
        
        public BoilDoneState(BurnerBehaviour burner) : base(burner)
        {     
            _burnerBehaviour.OnBurnerNotification(
                new NotificationManager.Notification("Your Pot is Boiling",
                    _burnerBehaviour));

            var transitionSequence = DOTween.Sequence();
            
            transitionSequence.Append(
                _burnerBehaviour.ring.Hide()
                    .OnComplete(() =>
                    {
    
                        _burnerBehaviour.ring.SetMaterialToDefault();
                        _burnerBehaviour.ring.SetAlpha(0);
                        _burnerBehaviour.ring.SetColor(Color.red);
                    })
                );

            transitionSequence.Insert(
                0, 
                _burnerBehaviour.HideLabel()
            );

            transitionSequence.Append(
               _burnerBehaviour.ring.Show());

            transitionSequence.OnComplete(() =>
            {
                _pulseRedSequence = DOTween.Sequence();
                                      
                _pulseRedSequence.Append(
                    DOTween
                        .To(_burnerBehaviour.ring.GetAlpha, 
                            _burnerBehaviour.ring.SetAlpha,
                            0f, 
                            1.5f)
                        .SetEase(Ease.InSine)
                );
                
                _pulseRedSequence.AppendInterval(0.1f);
                
                _pulseRedSequence
                    .SetLoops(-1, LoopType.Yoyo)
                    .Play();
                
                _burnerBehaviour.SetLabel("Done");
            });          
        }

        public override State Update()
        {
            if (!_burnerBehaviour._model.IsPotDetected.Value)
            {
                _pulseRedSequence.Kill(true);
                _pulseRedSequence.OnComplete(() =>
                {
                    _burnerBehaviour.ring.Hide()
                        .OnComplete(() =>
                        {
                            _burnerBehaviour.ring.SetColor(Color.white);
                        });
                    
                    //TODO: NOT WORKING
                });
              
                return new WaitingState(_burnerBehaviour);
            }
            
            return null;
        }
    }
    
    public class WaitingForTimerState : BurnerState
    {
        public WaitingForTimerState(TimeSpan ts, BurnerBehaviour burner) : base(burner)
        {
            burner.HideProactiveTimer();
		
            burner._Timer.gameObject.SetActive(true);
            burner._Timer.SetTimer(ts);
            burner._Timer.SetTransparency(0);

            Task.Delay(900).ContinueWith(_ =>
                {
                    DOTween.To(burner._Timer.GetTransparency, 
                            burner._Timer.SetTransparency,
                            1f, 
                            1.4f)
                        .SetEase(Ease.InSine);
                }
            );
        }

        public override State Update()
        {
            if (_burnerBehaviour._Timer.isComplete)
            {
                return new TimerDoneState(_burnerBehaviour);
            }

            return null;
        }
    }
    
    public class TimerDoneState : BurnerState
    {
        public TimerDoneState(BurnerBehaviour burner) : base(burner)
        {
            _burnerBehaviour.OnBurnerNotification(
                new NotificationManager.Notification("Your Timer is Done",
                    _burnerBehaviour));
        }

        public override State Update()
        {
            return null;
            
            //if is dismissed, transition out, return waiting state.
        }
    }   
}

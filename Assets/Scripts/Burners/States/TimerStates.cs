using System;
using System.Threading.Tasks;
using DG.Tweening;

namespace Burners.States
{
    public static class TimerStates
    {        
        public class WaitingForTimerState : BurnerStates.BurnerState
        {
            public WaitingForTimerState(BurnerBehaviour burner, TimeSpan ts) : base(burner)
            {		
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

                return this;
            }
        }
    
        public class TimerDoneState : BurnerStates.BurnerState
        {
            public TimerDoneState(BurnerBehaviour burner) : base(burner)
            {
                _burnerBehaviour.OnBurnerNotification(
                    new NotificationManager.Notification("Your Timer is Done",
                        _burnerBehaviour));
            }

            public override State Update()
            {
                return this;
            
                //if is dismissed, transition out, return waiting state.
            }
        }   
    }
}
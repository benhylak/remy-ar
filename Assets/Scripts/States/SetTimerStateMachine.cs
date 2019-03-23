
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public static class SetTimerStateMachine
{

    public static State GetInitialState()
    {
        return new MonitoringState();
    }
    
    public class MonitoringState : State
    {
        public MonitoringState()
        {          
            Debug.Log("Entering Monitoring State...");
        }
        
        public override State Update()
        {
            foreach (var burner in BigKahuna.Instance._burnerBehaviours)
            {
                if (burner._state is BurnerStateMachine.AvailableState && burner._model.IsPotDetected.Value && !RecipeManager.Instance.IsWaitingForBurner())
                {
                    return new BufferState(
                        this, 
                        () => new ProactiveState(burner, this),
                        () => burner._model.IsPotDetected.Value,
                        .2f);
                }
            }
            
            return null;
        }
    }

    public class ProactiveState : State
    {
        public BurnerBehaviour _burner;
        
        public ProactiveState(BurnerBehaviour burner, State state)
        {
            this._burner = burner;
            //this._burner.ShowProactiveTimer();

            if (state is BufferState || state is MonitoringState)
            {
                _burner.HiddenToProactive();
            }
            else if (state is VoiceInputState)
            {
                _burner.InputToProactive();
            }
            
            Debug.Log("Entering Proactive State...");
        }

        public override State Update()
        {
            if (this._burner._model.IsPotDetected.Value == false)
            {
                _burner.HideProactivePrompt();
                
                return new MonitoringState();
            }
            else if (this._burner._gazeReceiver.isLookedAt)
            {
                return new VoiceInputState(_burner);
            }
            
            return null;
        }
    }
    
    public class VoiceInputState : State
    {
        public BurnerBehaviour _burner;

        public float _lastLookedAt;
        public float _timeOut = 1.8f;
        
        private static Regex timeRegex = new Regex(@"(?<minutes>\d+(?=\sminutes))|(?<seconds>\d+(?=\sseconds))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        private static Regex boilRegex = new Regex(@"(oil|boil|well)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //(?P<minutes>\d+(?=\sminutes))|(?P<seconds>\d+(?=\sseconds))
        
        
        public VoiceInputState(BurnerBehaviour burner)
        {
            this._burner = burner;
            Debug.Log("Voice input state");
            
            BigKahuna.Instance.speechRecognizer.Active = true;
            _burner.ShowInputPrompt();
        }
        
        public override State Update()
        {                    
            if (_burner._gazeReceiver.timeSinceLastGaze > _timeOut)
            {
                BigKahuna.Instance.speechRecognizer.Active = false;
                return new ProactiveState(_burner, this);
            }
            
            _burner.SetInputLevel(BigKahuna.Instance.speechRecognizer.audioLevel);

            if (BigKahuna.Instance.speechRecognizer.finalized)
            {
                MatchCollection matches;

                if (boilRegex.IsMatch(BigKahuna.Instance.speechRecognizer.recognizedText))
                {
                    _burner.HideProactivePrompt().OnComplete(()=>_burner.WaitForBoil());
                    
                    BigKahuna.Instance.speechRecognizer.Active = false;
                    
                    return new MonitoringState();
                }
                else if ((matches = timeRegex.Matches(BigKahuna.Instance.speechRecognizer.recognizedText)).Count > 0)
                {               
                    TimeSpan ts = new TimeSpan();
                    
                    foreach (Match m in matches)
                    {
                        GroupCollection groups = m.Groups;
                    
                        Debug.Log(m.Value);

                        Group minutes = groups["minutes"];            
                        Group seconds = groups["seconds"];
                    
                        var minutesVal = minutes.Success ? int.Parse(minutes.Value) : 0;       
                        var secondsVal = seconds.Success ? int.Parse(seconds.Value) : 0;
                        
                        ts = ts.Add(new TimeSpan(0, minutesVal, secondsVal));
                    }
                    
                    Debug.Log(ts.ToString());
                  
                    _burner.SetTimer(ts);
                    BigKahuna.Instance.speechRecognizer.Active = false;
                    
                    return new MonitoringState();
                }
                
                BigKahuna.Instance.speechRecognizer.recognizedText = "";
                BigKahuna.Instance.speechRecognizer.finalized = false;
            }

            return null;
        }

    }    
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public abstract class TimerState
{
    protected TimerState()
    {
        
    }
    
    public abstract TimerState Update();
    
    public class MonitoringState : TimerState
    {
        //get from singleton
        //private List<BurnerBehaviour> _burnerBehaviours;

        public MonitoringState()
        {          
            Debug.Log("Entering Monitoring State...");
        }
        
        public override TimerState Update()
        {
            foreach (var burner in BigKahuna.Instance._burnerBehaviours)
            {
                if (burner._model.IsPotDetected.Value)
                {
                    return new BufferState(burner);
                }
            }
            
            return null;
        }
    }

    public class BufferState : TimerState
    {
        //buffers a PotDetected value, makes sure that it lasts
        
        public BurnerBehaviour _burner;

        readonly float WAIT_TIME = 1f;
        float _startTime;
        public BufferState(BurnerBehaviour burner)
        {
            _burner = burner;
            _startTime = Time.time;
        }

        public override TimerState Update()
        {
            if (_burner._model.IsPotDetected.Value == false)
            {
                return new MonitoringState();
            }
            else if (Time.time - _startTime > WAIT_TIME)
            {
                return new ProactiveState(_burner);
            }
            else
            {
                return null;
            }
        }
    }

    public class ProactiveState : TimerState
    {
        public BurnerBehaviour _burner;
        
        public ProactiveState(BurnerBehaviour burner)
        {
            this._burner = burner;
            this._burner.ShowProactiveTimer();
            
            Debug.Log("Entering Proactive State...");
        }

        public override TimerState Update()
        {
            if (this._burner._model.IsPotDetected.Value == false)
            {
                _burner.HideProactiveTimer();
                
                return new MonitoringState();
            }
            else if (this._burner.IsLookedAt)
            {
                return new VoiceInputState(_burner);
            }
       
            return null;
        }
    }
    
    public class VoiceInputState : TimerState
    {
        public BurnerBehaviour _burner;

        public float _lastLookedAt;
        public float _timeOut = 1.8f;
        
        public static Regex rx = new Regex(@"(?<minutes>\d+(?=\sminutes))|(?<seconds>\d+(?=\sseconds))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //(?P<minutes>\d+(?=\sminutes))|(?P<seconds>\d+(?=\sseconds))
        
        
        public VoiceInputState(BurnerBehaviour burner)
        {
            this._burner = burner;
            Debug.Log("Voice input state");
            _burner.ShowInputPrompt();
        }
        
        public override TimerState Update()
        {                    
            if (_burner.IsLookedAt)
            {
                _lastLookedAt = Time.time;

            }         
            else if (Time.time - _lastLookedAt > _timeOut)
            {
                return new ProactiveState(_burner);
            }
            
            _burner.SetInputLevel(BigKahuna.Instance.speechRecognizer.audioLevel);

            if (BigKahuna.Instance.speechRecognizer.finalized)
            {
                MatchCollection matches = rx.Matches(BigKahuna.Instance.speechRecognizer.recognizedText);

                if (matches.Count > 0)
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
                    
                    _burner.HideProactiveTimer();
                    _burner.SetTimer(ts);
                }
                
                BigKahuna.Instance.speechRecognizer.recognizedText = "";
                BigKahuna.Instance.speechRecognizer.finalized = false;
            }

            return null;
        }

    }    
}

using System.Collections.Generic;
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

        readonly float WAIT_TIME = 2f;
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
        public float _timeOut = 10;
        
        public VoiceInputState(BurnerBehaviour burner)
        {
            this._burner = burner;
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
            
            return null;
        }
    }    
}

using System.Collections.Generic;
using UnityEngine;

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
                    return new ProactiveState(burner);
                }
            }
            
            return null;
        }
    }

    public class ProactiveState : TimerState
    {
        public BurnerBehaviour _burner;
        
        public ProactiveState(BurnerBehaviour burner)
        {
            this._burner = burner;
            this._burner.SuggestTimer();
            
            Debug.Log("Entering Proactive State...");
        }

        public override TimerState Update()
        {
            if (this._burner._model.IsPotDetected.Value == false)
            {
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
        
        public float _lastLookedAt = 0;
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
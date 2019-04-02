using System;
using System.Threading.Tasks;
using Burners.States;
using DG.Tweening;
using UnityEngine;
using UnitySDK.WebSocketSharp;

public static class BurnerStates
{
    public abstract class BurnerState : State
    {
        protected BurnerBehaviour _burnerBehaviour;
        
        protected BurnerState(BurnerBehaviour burner)
        {
            _burnerBehaviour = burner;
        }
    }
    
    public class IndeterminateState : BurnerStates.BurnerState
    {
        public Func<BurnerStates.BurnerState> _onFinished;
        public Func<bool> _isDone;
        public string _notificationMessage;
        
        protected IndeterminateState(BurnerBehaviour burnerBehaviour, string notificationMessage = "") : base(burnerBehaviour)
        {
            _notificationMessage = notificationMessage;
            _burnerBehaviour.ring.gameObject.SetActive(true);
            _burnerBehaviour.ring.SetMaterialToIndeterminate();
            _burnerBehaviour.ring.SetColor(RemyColors.RED);
            _burnerBehaviour.ring.SetAlpha(0);
            _burnerBehaviour.ring.Show(0.6f);
        }
            
        public IndeterminateState(BurnerBehaviour burner, Func<bool> isDone, Func<BurnerStates.BurnerState> onDone, string notification = "") : this(burner, notification)
        {
            _isDone = isDone;
            _onFinished = onDone;
        }

        public override State Update()
        {
            if (_isDone.Invoke())
            {
                if (!_notificationMessage.IsNullOrEmpty())
                {
                    _burnerBehaviour.RaiseBurnerNotification(text: _notificationMessage);
                }
                
                _burnerBehaviour.ring.Hide();
                                  
                return _onFinished.Invoke();
            }

            return this;
        }
    }


    public class BurnerTransitionState : BurnerState
    {
        private Task _taskToWaitOn;
        private Tween _tweenToWaitOn;
        private Func<BurnerState> _stateBuilder;
        private float _additionalDelaySeconds;
        private float _finishedTime;
        
        public BurnerTransitionState(BurnerBehaviour burner, Tween tween, Func<BurnerState> nextStateBuilder, float additionalDelaySeconds = 0) : base(burner)
        {
            _stateBuilder = nextStateBuilder;
            _tweenToWaitOn = tween;
            _additionalDelaySeconds = additionalDelaySeconds;
        }
        
        public BurnerTransitionState(BurnerBehaviour burner, Task taskToWaitOn, Func<BurnerState> nextStateBuilder) : base(burner)
        {
            _stateBuilder = nextStateBuilder;
            _taskToWaitOn = taskToWaitOn;
        }

        public bool IsComplete()
        {
            bool isComplete = false;

            if (_tweenToWaitOn != null)
            {
                isComplete = !_tweenToWaitOn.IsActive();
            }
            else if (_taskToWaitOn!=null)
            {
                isComplete = _taskToWaitOn.IsCompleted;
            }

            if (isComplete &&_finishedTime.Equals(default(float)))
            {
                //just finished, so record finishtime
                _finishedTime = Time.time;
            }

            return isComplete;
        }
        
        public override State Update()
        {
            if (IsComplete() && Time.time - _finishedTime > _additionalDelaySeconds)
            {
                return _stateBuilder.Invoke();
            }

            return this;
        }
    }
    public class AvailableState : BurnerState
    {
        public AvailableState(BurnerBehaviour _burner) : base(_burner)
        {          
            Debug.Log(_burner + "is Available");
        }
        
        public override State Update()
        {
            State nextState = null;
            
            if (_burnerBehaviour._model != null && _burnerBehaviour._model.IsPotDetected.Value)
            {
                if (RecipeManager.Instance.IsWaitingForBurner())
                {
                    var recipe = RecipeManager.Instance.UseBurner(_burnerBehaviour);
           
                    return new RecipeStates.UseForRecipeState(_burnerBehaviour, recipe);
                    //transition to recipe stater
                }
                else
                {
                    nextState = new InputStates.TimerPromptState(_burnerBehaviour, this);
                }
            }

            return nextState;
        }
    }
}

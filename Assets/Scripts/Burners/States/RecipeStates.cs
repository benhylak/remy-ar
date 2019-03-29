using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Burners.States
{
    public static class RecipeStates
    {
        public class UseForRecipeState : BurnerStateMachine.BurnerState
        {
            private Recipe _recipe;
            private Recipe.RecipeStep _currentStep;
            private State _subState; //has it's own lil states it manages. isn't that cute?
            
            public UseForRecipeState(BurnerBehaviour _burner, Recipe recipe) : base(_burner)
            {
                _recipe = recipe;
                _burnerBehaviour._model.RecipeInProgress = _recipe;
            }

            public override State Update()
            {
                var nextState = _subState?.Update();
                _subState = nextState ?? _subState;

                //if a done waiting state is done indicating that it's done waiting, end the state 
                var state = _subState as DoneWaitingState;
                
                if (state != null && state._done)
                {
                    _subState = null;
                }
                                     
                if (_currentStep != _recipe.CurrentStep.Value)
                {
                    _currentStep = _recipe.CurrentStep.Value;
             
                    if (_currentStep.RequiresBurner)
                    {
                        _subState = CheckForTriggers() ?? _subState;
                    }
                    else
                    {
                        _recipe._burner = null;
                        _burnerBehaviour._model.RecipeInProgress = null;
                    }
                }

                return null; //eventually return the right sub states
            }

            public BurnerStateMachine.BurnerState CheckForTriggers()
            {
                if (_currentStep.NextStepTrigger == _recipe.BurnerIsBoiling)
                {
                    return new BoilStates.WaitingForBoilState(_burnerBehaviour, () => new DoneWaitingState(_burnerBehaviour));
                }
                else if (_burnerBehaviour._Timer.isSet)
                {
                    //timer was set -- move to timer waiting state
                }

                return null;
            }
        }

        public class DoneWaitingState : BurnerStateMachine.BurnerState
        {
            public bool _done;
            
            public DoneWaitingState(BurnerBehaviour burner) : base(burner)
            {
                _burnerBehaviour.ring.SetColor(RemyColors.RED);
                _burnerBehaviour.ring.StartPulsing(RemyColors.RED, RemyColors.RED_RIM);
                _burnerBehaviour.HideLabel();
            }

            public override State Update()
            {
                if (_burnerBehaviour._gazeReceiver.isLookedAt &&
                    _burnerBehaviour._gazeReceiver.currentGazeDuration > 0.35f)
                {
                    _burnerBehaviour.ring.StopPulsing();
//                    _burnerBehaviour.ring.StopPulsing().onKill(() =>
                   // {
                        _burnerBehaviour.ring.Hide().OnComplete(() =>
                        {
                            _burnerBehaviour.ring.SetColor(Color.white);
                            _burnerBehaviour.ring.Show();
                        });
                   // });

                    _done = true;
                    
                    return null;
                }

                return this;
            }
        }
    }
}
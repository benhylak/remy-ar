using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnitySDK.WebSocketSharp;

namespace Burners.States
{
    public static class RecipeStates
    {
        public class UseForRecipeState : BurnerStates.BurnerState
        {
            private Recipe _recipe;
            private Recipe.RecipeStep _currentStep;
            private State _subState; //has it's own lil states it manages. isn't that cute?
            private bool _lastStepWasIndeterminate;
            
            public UseForRecipeState(BurnerBehaviour burner, Recipe recipe) : base(burner)
            {
                _recipe = recipe;
                _burnerBehaviour._model.RecipeInProgress = _recipe;
                
                Debug.Log("State change: Using for Recipe");
            }

            public override State Update()
            {
                _subState = _subState?.Update();
                //if a done waiting state is done indicating that it's done waiting, end the state 
                
                if (_currentStep != _recipe.CurrentStep.Value)
                {
                    var lastStep = _currentStep;
                    _currentStep = _recipe.CurrentStep.Value;

                    if (_currentStep.RequiresBurner)
                    {
                        if (!_currentStep.NextStepTrigger.Invoke()
                            && _currentStep.getAnchor?.Invoke() == (InstructionsAnchorable) _burnerBehaviour)
                        {
                            if (_currentStep.Timer != default(TimeSpan))
                            {
                                _subState = new TimerStates.WaitingForTimerState(_burnerBehaviour, _currentStep.Timer);
                            }
                            else
                            {
                                _subState = new WaitingState(_burnerBehaviour, _currentStep,
                                    lastStep?.IsIndeterminateWait() == true);
                            }                    
                        }
                    }         
                    else
                    {            
                        _recipe._burner = null;
                        _burnerBehaviour._model.RecipeInProgress = null;
                    }
                }

                return this;
            }
        }

        public class WaitingState : BurnerStates.BurnerState
        {
          //  public bool _done;
          private Recipe.RecipeStep _recipeStep;
          
            public WaitingState(BurnerBehaviour burner, Recipe.RecipeStep step, bool lastStepWasIndeterminate) : base(burner)
            {
                _recipeStep = step;
               
                if (_recipeStep.IsIndeterminateWait()) //indeterminate wait (red swirling circles)
                {
                    IndeterminateMode();
                    _burnerBehaviour.SetLabel(_recipeStep.WaitExplanation, 0.5f);
                    
                    Debug.Log("Indeterminate Wait");
                } 
                else if (lastStepWasIndeterminate) //pulse green
                {
                     _burnerBehaviour.ring.StartPulsing(RemyColors.GREEN, RemyColors.GREEN_RIM);
                }
                else // standard white ring
                {
                    _burnerBehaviour.ring.Show(0.6f);
                }
            }

            public void IndeterminateMode()
            {
                _burnerBehaviour.ring.gameObject.SetActive(true);
                _burnerBehaviour.ring.Show(0.4f)
                    .OnPlay(() =>
                    {
                        _burnerBehaviour.ring.SetRingRadius(_burnerBehaviour.ring.RING_RADIUS);
                        _burnerBehaviour.ring.SetMaterialToIndeterminate();
                        _burnerBehaviour.ring.SetColor(RemyColors.RED);
                        _burnerBehaviour.ring.SetAlpha(0);
                    });
            }

            public override State Update()
            {
                if (_recipeStep.NextStepTrigger.Invoke())
                {
                    Debug.LogError("Done Waiting");
                    _burnerBehaviour.HideLabel();
                    _burnerBehaviour.ring.Hide();
                    
                    if(_recipeStep.IsIndeterminateWait())
                        _burnerBehaviour.PlayDoneWaitingSound();
                    
                    return null;
                }
//                if (_burnerBehaviour._gazeReceiver.isLookedAt &&
//                    _burnerBehaviour._gazeReceiver.currentGazeDuration > 0.35f)
//                {
//                   // _burnerBehaviour.ring.StopPulsing();
////                    _burnerBehaviour.ring.StopPulsing().onKill(() =>
//                   // {
//                        _burnerBehaviour.ring.Hide().OnComplete(() =>
//                        {
//                            _burnerBehaviour.ring.SetColor(Color.white);
//                            _burnerBehaviour.ring.Show();
//                        });
//                   // });
//                    
//                    return null;
//                }

                return this;
            }
        }
    }
}
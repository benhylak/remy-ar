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
            
            public UseForRecipeState(BurnerBehaviour _burner, Recipe recipe) : base(_burner)
            {
                _recipe = recipe;
                _burnerBehaviour._model.RecipeInProgress = _recipe;
            }

            public override State Update()
            {
                _subState = _subState?.Update();
                //if a done waiting state is done indicating that it's done waiting, end the state 
                
                if (_currentStep != _recipe.CurrentStep.Value)
                {
                    _currentStep = _recipe.CurrentStep.Value;
             
                    if (_currentStep.RequiresBurner)
                    {
                        if (_currentStep.TargetTemperature != null)
                        {
                            _burnerBehaviour.targetTemperature = _currentStep.TargetTemperature;
                        }

                        var anchor = _currentStep.getAnchor.Invoke();
                        Debug.Log($"anchor: {anchor}");

                        if(anchor!=null && anchor == (InstructionsAnchorable)_burnerBehaviour)
                        {
                            Debug.Log("Waiting State");
                            _subState = new WaitingState(_burnerBehaviour, _currentStep);
                        }
                    }
                    else
                    {
                        _burnerBehaviour.targetTemperature = null;
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
          
            public WaitingState(BurnerBehaviour burner, Recipe.RecipeStep step) : base(burner)
            {
                _recipeStep = step;
                
                _burnerBehaviour.ring.Show(0.6f);
                
                if (!_recipeStep.WaitExplanation.IsNullOrEmpty())
                {
                    _burnerBehaviour.ring.gameObject.SetActive(true);
                    _burnerBehaviour.ring.SetMaterialToIndeterminate();
                    _burnerBehaviour.ring.SetColor(RemyColors.RED);
                    _burnerBehaviour.ring.SetAlpha(0);
                    _burnerBehaviour.ring.Show(0.6f);

                    //_burnerBehaviour.ring.SetAlpha(0);
                    _burnerBehaviour.SetLabel(_recipeStep.WaitExplanation, 0.5f);
                    
                    Debug.Log("Indeterminate Wait");
                }
                
          
            }

            public override State Update()
            {
                if (_recipeStep.NextStepTrigger.Invoke())
                {
                    Debug.LogError("Done!");
                 //   _burnerBehaviour.ring.Hide();
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
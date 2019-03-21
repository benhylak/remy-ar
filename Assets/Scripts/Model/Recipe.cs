
using System;
using System.Collections.Generic;

public class Recipe
{
    private Queue<RecipeStep> _recipeSteps;

    public BurnerBehaviour burner;
    
    public Recipe()
    {
        
    }
    public bool hasBurner()
    {
        return burner != null;
    }
    
    public bool hasBurnerWithPot()
    {
        return hasBurner() && burner._model.IsPotDetected.Value;
    }

    public void AddRecipeSteps(params RecipeStep[] recipeSteps)
    {
        _recipeSteps = new Queue<RecipeStep>();
    }
    
    public Recipe(params RecipeStep[] recipeSteps)
    {
        _recipeSteps = new Queue<RecipeStep>(recipeSteps);
    }
    public void Update()
    {
       bool finishedStep = _recipeSteps.Peek().Update();
       if (finishedStep) _recipeSteps.Dequeue();
    }
    
    public class RecipeStep
    {
        private Func<bool> isStepComplete;
        private Action onUpdate;
        private Action _onComplete;
        private Action _onEnter;

        private bool _justEntered = true;
        
        public RecipeStep(Action onEnter =null, Action onUpdate = null, Func<bool> isStepComplete =null, Action onComplete=null)
        {
            this._onEnter = onEnter;
            this.onUpdate = onUpdate;
            this.isStepComplete = isStepComplete;
            this._onComplete = onComplete;
            
        }

        public bool Update()
        {
            if (_justEntered)
            {
                _onEnter?.Invoke();
                _justEntered = false;
            }
            else if (isStepComplete())
            {
                _onComplete?.Invoke();
                return true;
            }
            else
            {
                onUpdate?.Invoke();
            }

            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Recipe
{
    private List<RecipeStep> _recipeSteps;

    public ReactiveProperty<int> _currentStepIndex = new ReactiveProperty<int>(-1);
    public IReadOnlyReactiveProperty<RecipeStep> CurrentStep;
    public IReadOnlyReactiveProperty<int> StepsLeftCount;

    public BurnerBehaviour _burner;

    public readonly IReadOnlyReactiveProperty<bool> RecipeComplete;

    public readonly string Name;

    public void AssignBurner(BurnerBehaviour burner)
    {
        _burner = burner;
        _burner.Consume();
    }
    
    public void UnassignBurner()
    {
        _burner.Release();
        _burner = null;
    }
    
    public int GetTotalSteps()
    {
        return _recipeSteps.Count;
    }
    
    public Recipe(string name)
    {
        _recipeSteps = new List<RecipeStep>();
        _currentStepIndex.Subscribe(i => Debug.Log($"Step index changed to: {i}"));
        
        Name = name;
        
        CurrentStep = _currentStepIndex
            .Where(i => _recipeSteps.Count > 0 && i < _recipeSteps.Count)
            .Select(i =>
            {       
                return _recipeSteps[i];
            })
            .ToReactiveProperty();
        
        StepsLeftCount = _currentStepIndex
            .Where(i => _recipeSteps.Count > 0 && i < _recipeSteps.Count)
            .Select(stepIndex =>
            {
                return _recipeSteps.Count - (stepIndex + 1);
            })
            .ToReactiveProperty();
        
        RecipeComplete = StepsLeftCount
                .Select(stepsLeft => stepsLeft == 0)
                .ToReactiveProperty();
    }

    public BurnerBehaviour GetBurner()
    {
        return _burner;
    }
    
    public bool HasBurner()
    {
        return _burner != null;
    }
    
    public bool HasAssignedBurner()
    {
        return HasBurner() && _burner._model.IsPotDetected.Value;
    }

    protected void SetRecipeSteps(params RecipeStep[] recipeSteps)
    {
        _recipeSteps.Clear();
        _recipeSteps.AddRange(recipeSteps);
        
        Debug.Log("num of steps: " + recipeSteps.Length);
        Debug.Log("num of steps list: " + _recipeSteps.Count);
        Debug.Log(GetTotalSteps());

        _currentStepIndex.Value = 0;
    }
   
    
    public bool Update()
    {
        if (!RecipeComplete.Value)
        {
            bool stepIsFinished = CurrentStep.Value.Update();

            if (stepIsFinished)
            {
                _currentStepIndex.Value++;
            }
        }

        return RecipeComplete.Value;
    }
    
    public class RecipeStep
    {
        private Func<bool> isStepComplete;
        private Action onUpdate;
        private Action _onComplete;
        private Action _onEnter;
        public string Instruction;

        private bool _justEntered = true;
        
        public RecipeStep(string instruction = "", Action onEnter =null, Action onUpdate = null, Func<bool> isStepComplete =null, Action onComplete=null)
        {
            this.Instruction = instruction;
            this._onEnter = onEnter;
            this._onComplete = onComplete;
            this.onUpdate = onUpdate;
            this.isStepComplete = isStepComplete;
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
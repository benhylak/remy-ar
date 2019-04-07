
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnitySDK.WebSocketSharp;

public class Recipe
{
    private List<RecipeStep> _recipeSteps;

    public ReactiveProperty<int> _currentStepIndex = new ReactiveProperty<int>(-1);
    public IReadOnlyReactiveProperty<RecipeStep> CurrentStep;
    public IReadOnlyReactiveProperty<int> StepsLeftCount;

    public BurnerBehaviour _burner;

    public readonly string Name;

    private string _status = "";
    
    public string Status => _status;

    public void UpdateStatus(string status)
    {
        _status = status;       
        Debug.Log($"{Name} status updated to {status}");
    }

    public int GetTotalSteps()
    {
        return _recipeSteps.Count;
    }

    public bool IsOnLastStep => StepsLeftCount.Value == 0;
    
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
            .Select(stepIndex => _recipeSteps.Count - (stepIndex + 1))
            .ToReactiveProperty();
    }

    public BurnerBehaviour GetBurner()
    {
        return _burner;
    }

    public virtual void FreeResources()
    {
        if (GetBurner() != null)
        {
            GetBurner().SetStateToDefault();
        }
    }
    
    public bool BurnerIsBoiling()
    {
        return GetBurner().IsBoiling();
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
        bool stepIsFinished = CurrentStep.Value.Update();
        bool recipeIsComplete = false;
        
        if (stepIsFinished && IsOnLastStep)
        {
            recipeIsComplete = true;
        }
        else if (stepIsFinished)
        {
            _currentStepIndex.Value++;
        }

        return recipeIsComplete;
    }
    
    public bool IsTimerDone()
    {
        return GetBurner().IsTimerDone();
    }
    
    public class RecipeStep
    {
        public Func<bool> NextStepTrigger;
        private Action _onEnter;
        public string Instruction;
        public readonly bool RequiresBurner;
        public readonly string WaitExplanation;
        public TimeSpan Timer;
        
        public Func<InstructionsAnchorable> getAnchor;
        public Action _onComplete;
        
        private bool _justEntered = true;
        
        public RecipeStep(string instruction = "", 
            string waitExplanation ="",
            Action onEnter =null, 
            Action onComplete =null, 
            Func<bool> nextStepTrigger =null, 
            Func<InstructionsAnchorable> getAnchor = null, 
            TimeSpan timer = default(TimeSpan),
            bool requiresBurner = false)
        {
            this.Instruction = instruction;

            this.Timer = timer;
            this._onEnter = onEnter;
            this._onComplete = onComplete;
            
            this.NextStepTrigger = nextStepTrigger;
            this.getAnchor = getAnchor;
            
            RequiresBurner = requiresBurner;
            this.WaitExplanation = waitExplanation;
        }

        public bool Update()
        {
            if (NextStepTrigger())
            {
                _onComplete?.Invoke();
                Debug.Log("Done Step");
                return true;  
            }
            else if (_justEntered)
            {
                _onEnter?.Invoke();
                _justEntered = false;
                return false;
            }

            return false;
        }

        public bool IsIndeterminateWait()
        {
            return !WaitExplanation.IsNullOrEmpty();
        }
    }
}
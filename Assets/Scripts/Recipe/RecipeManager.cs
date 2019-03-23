using System;
using System.Collections.Generic;
using System.Linq;
using BensToolBox;
using UniRx;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class RecipeManager : Singleton<RecipeManager>
{
    private Recipe _recipeInProgress;
    private bool _waitingForBurner;
    public InstructionUI _instructionUi;
    
    public void StartRecipe(Recipe recipe)
    {
        _recipeInProgress = recipe;
        
        _instructionUi.SetRecipe(recipe);
    }
    
    public bool IsWaitingForBurner()
    {
        return _waitingForBurner;
    }
    
    public bool IsBurnerInUse(BurnerBehaviour b)
    {
        return _recipeInProgress._burner == b;
    }
    
    /**
     * Returns an observable that emits a single value when any burner has a pot placed on it.
     *
     * Also sets a "waiting" flag that other classes (like set timer) can check.
     */
    public IObservable<BurnerBehaviour> OnPotPlaced()
    {
        IEnumerable<IObservable<BurnerBehaviour>> potDetectedSources =
            BigKahuna.Instance._burnerBehaviours.Select(burner =>
                burner._model.IsPotDetected
                    .Where(detected => detected == true)
                    .Select(_ => burner));

        _waitingForBurner = true;

        var potDetectedWatcher =
            potDetectedSources
                .Merge()
                .Where(b => 
                    b._state is BurnerStateMachine.AvailableState)
                .Take(1);

        potDetectedWatcher.Subscribe(_ =>
        {
            Debug.Log(_);
            _waitingForBurner = false;
        });

        return potDetectedWatcher;
    }
    
    private void Update()
    {
        if(_recipeInProgress != null)
            _recipeInProgress.Update();
    }
}
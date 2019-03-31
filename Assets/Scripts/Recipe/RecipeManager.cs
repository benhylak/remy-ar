using System;
using System.Collections.Generic;
using System.Linq;
using BensToolBox;
using UniRx;
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

    public Recipe UseBurner(BurnerBehaviour _burner)
    {
        _recipeInProgress._burner = _burner;
        return _recipeInProgress;
    }

    public bool IsRecipeInProgress => _recipeInProgress != null;

    public void EndRecipe()
    {
        Debug.Log("========RECIPE ENDED========");
        
        _instructionUi.Hide();
        _instructionUi.transform.parent = this.transform;
        
        _recipeInProgress.FreeResources();
    }
    public bool IsWaitingForBurner()
    {
        //if we are waiting for an assigned burner before moving to the next step, then we are waiting for a burner.
        return _recipeInProgress != null &&_recipeInProgress.CurrentStep.Value.NextStepTrigger == _recipeInProgress.HasAssignedBurner;
    }
    
    private void Update()
    {
        if(_recipeInProgress != null)
            _recipeInProgress.Update();
    }
}
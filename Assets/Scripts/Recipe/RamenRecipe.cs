using System;
using UniRx;
using BensToolBox.AR.Scripts;
using BensToolBox;
using BensToolBox.AR;
using UnityEngine;
using UnityEngine.UI;

public class RamenRecipe : Recipe
{
    private readonly string NOODLES_ADDED_STATUS = "NOODLES_ADDED";
    
    public RamenRecipe(RamenUI ramen) : base("Ramen")
    {
        SetRecipeSteps(
            new RecipeStep(
                getAnchor: ()=>ramen,
                instruction: "Boil <b>2 Cups</b> of water.",
                nextStepTrigger: HasAssignedBurner
            ),

            new RecipeStep(
                nextStepTrigger: BurnerIsBoiling,
                requiresBurner: true
            ), //in future, can wait for gaze before moving on

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "<b>Add noodles</b>",
                nextStepTrigger: () => Status == NOODLES_ADDED_STATUS,
                requiresBurner: true,
                onComplete: () => GetBurner().ring.Hide()
            ),
            
            new RecipeStep(
                onEnter:() =>
                {
                    GetBurner().SetTimer(new TimeSpan(0, 5, 0));
                },
                nextStepTrigger: () => !GetBurner()._model.IsPotDetected.Value,
                requiresBurner: true
            ),
            
            new RecipeStep(
                requiresBurner: false,
                
                onEnter: null,
                
                nextStepTrigger: () =>
                {
                    //delay and then...
                    return true;                 
                },
                
                onComplete: ()=>
                {
                    Debug.Log("Done!");
                    //hide success message
                }          
            )                   
        );
    }
}
using System;
using UniRx;
using BensToolBox.AR.Scripts;
using BensToolBox;
using BensToolBox.AR;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PancakeRecipe : Recipe
{
    private readonly string PANCAKE_FLIPPED = "PANCAKE_FLIPPED";
    private readonly string PANCAKE_READY_TO_FLIP = "PANCAKE_READY_TO_FLIP";
    private readonly string PANCAKE_POURED = "PANCAKE_POURED";
    
    private readonly float PAN_PREHEAT_TEMP = 60;
    
    public PancakeRecipe() : base("Pancakes")
    {

        SetRecipeSteps(
            new RecipeStep(
                // getAnchor: () => ramen,
                //instruction: "Boil <b>2 Cups</b> of water.",
                nextStepTrigger: HasAssignedBurner
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                targetTemp: PAN_PREHEAT_TEMP,
                waitExplanation: "Heating...",
                nextStepTrigger: () => GetBurner().HasReachedTargetTemp(),
                requiresBurner: true,
                onComplete: () =>
                {
                    GetBurner().RaiseBurnerNotification($"Pan is Preheated ({(int)GetBurner()._model.Temperature.Value} C)");
                }
            ), //in future, can wait for gaze before moving on

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Pour <b>1/4 Cup</b> of Mix",
                nextStepTrigger: () => Status == PANCAKE_POURED,
                requiresBurner: true
            ),

            new RecipeStep(
                waitExplanation: "Cooking...",
                nextStepTrigger: () => Status == PANCAKE_READY_TO_FLIP,
                requiresBurner: true,
                onComplete: () =>
                {
                   GetBurner().RaiseBurnerNotification("Flip Pancake");
                }
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

    public override void FreeResources()
    {
     
        base.FreeResources();
    }
}
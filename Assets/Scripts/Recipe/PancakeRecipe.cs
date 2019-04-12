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
    private readonly string NO_PANCAKE = "NO_PANCAKE";
    
    private readonly float PAN_PREHEAT_TEMP = 60;
    
    public PancakeRecipe() : base("Pancakes")
    {
        //Dialog on Box "make pancakes"   

        SetRecipeSteps(
            new RecipeStep(
                // getAnchor: () => ramen,
                //instruction: "Boil <b>2 Cups</b> of water.",
                nextStepTrigger: HasAssignedBurner
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Turn On Burner",
                nextStepTrigger: () => GetBurner()._model.IsOn.Value,
                requiresBurner: true
            ), //in future, can wait for gaze before moving on

            new RecipeStep(
                getAnchor: GetBurner,
                waitExplanation: "Heating...",
                nextStepTrigger: () => GetBurner()._model.Temperature > PAN_PREHEAT_TEMP,
                requiresBurner: true,
                onComplete: () =>
                {
                    GetBurner().RaiseBurnerNotification($"Pan is Preheated ({(int) GetBurner()._model.Temperature} C)");
                }
            ), //in future, can wait for gaze before moving on

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Pour <b>1/4 Cup</b> of Mix",
                nextStepTrigger: () => Status == PANCAKE_POURED,
                requiresBurner: true
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                waitExplanation: "Cooking...",
                nextStepTrigger: () => Status == PANCAKE_READY_TO_FLIP,
                requiresBurner: true,
                onComplete: () => { GetBurner().RaiseBurnerNotification("Flip Pancake Now"); }
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Flip Pancake",
                nextStepTrigger: () => Status == PANCAKE_FLIPPED,
                requiresBurner: true
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                timer: new TimeSpan(0, 1, 30),
                nextStepTrigger: IsTimerDone,
                onComplete: () => { GetBurner()._Timer.Reset(disableAfter: true); },
                requiresBurner: true
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Remove Pancake",
                nextStepTrigger: () => Status == NO_PANCAKE,
                requiresBurner: true
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Pour <b>1/4 Cup</b> of Mix",
                nextStepTrigger: () => Status == PANCAKE_POURED,
                requiresBurner: true
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                waitExplanation: "Cooking...",
                nextStepTrigger: () => Status == PANCAKE_READY_TO_FLIP,
                requiresBurner: true,
                onComplete: () => { GetBurner().RaiseBurnerNotification("Flip Pancake Now"); }
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Flip Pancake",
                nextStepTrigger: () => Status == PANCAKE_FLIPPED,
                requiresBurner: true
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                timer: new TimeSpan(0, 1, 30),
                nextStepTrigger: IsTimerDone,
                onComplete: () => { GetBurner()._Timer.Reset(disableAfter: true); },
                requiresBurner: true
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Remove Pancake",
                nextStepTrigger: () => Status == NO_PANCAKE,
                requiresBurner: true
            ),
            
            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Turn Burner Off",
                nextStepTrigger: () => !_burner._model.IsOn.Value,
                    requiresBurner: true
            )            
        );
    }

    public override void FreeResources()
    {
     
        base.FreeResources();
    }
}
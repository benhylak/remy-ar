using System;
using UniRx;
using BensToolBox.AR.Scripts;
using BensToolBox;
using BensToolBox.AR;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RamenRecipe : Recipe
{
    private readonly string NOODLES_ADDED_STATUS = "NOODLES_ADDED";
    private RamenUI _ramen;
    
    public RamenRecipe(RamenUI ramen) : base("Ramen")
    {
        _ramen = ramen;

        SetRecipeSteps(
            new RecipeStep(
                getAnchor: () => ramen,
                instruction: "Boil <b>2 Cups</b> of water.",
                nextStepTrigger: HasAssignedBurner,
                requiresBurner: true
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Turn On Burner",
                nextStepTrigger: () => GetBurner()._model.IsOn.Value,
                requiresBurner: true
            ),

            new RecipeStep(
                nextStepTrigger: BurnerIsBoiling,
                waitExplanation: "Waiting to Boil",
                getAnchor: GetBurner,
                requiresBurner: true
            ), //in future, can wait for gaze before moving on

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "<b>Add noodles</b>",
                nextStepTrigger: () => Status == NOODLES_ADDED_STATUS,
                requiresBurner: true
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                timer: new TimeSpan(0, 1, 0),
                nextStepTrigger: IsTimerDone,
                onComplete: () => { GetBurner()._Timer.Reset(disableAfter: true); },
                requiresBurner: true
            ),

            new RecipeStep(
                getAnchor: GetBurner,
                instruction: "Remove Pot",
                nextStepTrigger: () => GetBurner()._model.IsPotDetected.Value,
                requiresBurner: true
            )         
        );
    }

    public override void FreeResources()
    {
        _ramen.inputIsEnabled = true;
        base.FreeResources();
    }
}
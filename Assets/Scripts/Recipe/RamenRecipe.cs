using System;
using UniRx;

public class RamenRecipe : Recipe
{
    public RamenRecipe(RamenUI ramen) : base("Ramen")
    {
        SetRecipeSteps( 
             new RecipeStep(
                instruction: "Boil <b>2 Cups</b> of water.",
                onEnter: () => RecipeManager.Instance
                    .OnPotPlaced()
                    .Subscribe(AssignBurner),  
                isStepComplete: HasAssignedBurner
            ),
             
            new RecipeStep(
                onEnter: () => GetBurner().WaitForBoil(350),
                isStepComplete: () => GetBurner()._model.IsBoiling.Value), //in future, can wait for gaze before moving on
            
            new RecipeStep(
                instruction: "<b>Add noodles</b>",
                isStepComplete: () =>
                {
                    return false;
                    //ramenRecipe.burner._model.recipeStatus.ramenDetected;
                }        
            ),
            
            new RecipeStep(
                onEnter:() =>
                {
                    GetBurner().SetTimer(new TimeSpan(0, 5, 0));
                },
                isStepComplete: () =>
                {
                    return !GetBurner()._model.IsPotDetected.Value; //pot removed
                    //engages with eye contact or distance? or removes pot
                    //ramenRecipe.burner._model.recipeStatus.ramenDetected;
                },
                onComplete: UnassignBurner
            ),
            
            new RecipeStep(
                
                onEnter:() =>
                {
                    //success over coaster
                },
                
                isStepComplete: () =>
                {
                    //delay and then...
                    return true;                 
                },
                
                onComplete: ()=>
                {
                    //hide success message
                }          
            )                   
        );
    }
}
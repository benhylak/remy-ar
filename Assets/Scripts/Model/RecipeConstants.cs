using System;

public static class RecipeConstants
{
    public static Recipe makeRamenRecipe()
    {
        var ramenRecipe = new Recipe();

        ramenRecipe.AddRecipeSteps(
            
            new Recipe.RecipeStep(
                () =>
                {
                    //ramen -> show instruction 1
                    //recipe manager -> waitingForPot(this.parentRecipe)                
                },
                isStepComplete: () => ramenRecipe.hasBurnerWithPot(),
                onComplete: () =>
                {
                    //ramen -> hide instruction 1
                }
            ),
            new Recipe.RecipeStep(
                () => ramenRecipe.burner.WaitForBoil(),
                isStepComplete: () => ramenRecipe.burner._model.IsBoiling.Value), //in future, can wait for gaze before moving on
            
            new Recipe.RecipeStep(
                () =>
                {
                    //ramenRecipe.burner.ShowRamenStep1();
                },
                isStepComplete: () =>
                {
                    return false;
                    //ramenRecipe.burner._model.recipeStatus.ramenDetected;
                }        
            ),
            
            new Recipe.RecipeStep(
                () =>
                {
                    ramenRecipe.burner.SetTimer(new TimeSpan(0, 5, 0));
                },
                isStepComplete: () =>
                {
                    return !ramenRecipe.burner._model.IsPotDetected.Value; //pot removed
                    //engages with eye contact or distance? or removes pot
                    //ramenRecipe.burner._model.recipeStatus.ramenDetected;
                }        
            ),
            
            new Recipe.RecipeStep(
                () =>
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

        return ramenRecipe;

    }
}
using BensToolBox;
using UnityEngine;

public class RecipeManager : Singleton<RecipeManager>
{
    private Recipe _recipeInProgress;
    private bool _waitingForBurner;

    public bool IsWaitingForBurner()
    {
        return _waitingForBurner;
    }
    public bool IsBurnerInUse(BurnerBehaviour b)
    {
        return _recipeInProgress.burner == b;
    }
    
    public void WaitForPotPlaced()
    {
        _waitingForBurner = true;
    }
    
    private void Update()
    {
        throw new System.NotImplementedException();
    }
}
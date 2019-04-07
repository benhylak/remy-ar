using System.Timers;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecipeManager))]
public class RecipeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RecipeManager recipeManager = (RecipeManager) target;
        
        if(GUILayout.Button("Make Pancakes"))
        {
            recipeManager.StartRecipe(new PancakeRecipe());
        }
   
        if(GUILayout.Button("Make Ramen"))
        {
            recipeManager.StartRecipe(new RamenRecipe(BigKahuna.Instance.ramenUI));
        }
        
        if (GUILayout.Button("End Recipe"))
        {
            recipeManager.ClearRecipe();
        }
  
    }
}
using System.Timers;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BurnerOnVisualizer))]
public class BurnerOnVisualizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        BurnerOnVisualizer visualizer = (BurnerOnVisualizer)target;

        string showOrHide = visualizer.IsActive ? "Hide" : "Show";
        
        if(GUILayout.Button(showOrHide))
        {
            if(visualizer.IsActive) visualizer.Hide();
            else visualizer.Show();
        }        
    }
}
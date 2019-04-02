using System.Timers;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

[CustomEditor(typeof(RamenUI))]
public class RamenUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RamenUI ramenUi = (RamenUI) target;
        
        if(GUILayout.Button("Make Ramen"))
        {
           ramenUi.MakeRamen();
        }
        
        if(GUILayout.Button("Pick Up"))
        {
            var moveTo = Camera.main.transform.position;
            moveTo += Camera.main.transform.forward * 0.5f;
            moveTo -= Camera.main.transform.up * 0.2f;
            moveTo -= Camera.main.transform.right * 0.15f;

            var lookDir = Camera.main.transform.position - moveTo;

            var lookRot = Quaternion.LookRotation(lookDir, Vector3.right);
            lookRot *= Quaternion.Euler(180, -90f, -90f);

//            var eulerLook = Quaternion.FromToRotation(ramenUi.transform.up, lookDir).eulerAngles;
            
            ramenUi.transform.DOMove(moveTo, 0.35f);
            ramenUi.transform.DORotate(lookRot.eulerAngles, 0.35f);
        }
        
        if(GUILayout.Button("Hide Instructions"))
        {
            RecipeManager.Instance._instructionUi.Hide();
        }
    }
}
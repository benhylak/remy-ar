using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NotificationBehaviour), true)]
public class NotificationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var notif = (NotificationBehaviour) target;
        
        if(GUILayout.Button("Launch"))
        {
           notif.Launch();
        }
    }
}

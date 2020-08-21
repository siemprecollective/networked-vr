using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PlayerInitTransform))]
public class PlayerInitTransformEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerInitTransform myScript = (PlayerInitTransform)target;
        if (GUILayout.Button("Save Current Transforms"))
        {
            myScript.SaveCurrentTransform();
        }
        if (GUILayout.Button("Apply Saved Transforms As Init"))
        {
            myScript.WriteSavedTransform();
        }
    }
}
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SavePlayMode))]
public class SomeScriptEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SavePlayMode myScript = (SavePlayMode)target;
        if (GUILayout.Button("Save Transforms"))
        {
            myScript.SaveTransforms();
        }
        if (GUILayout.Button("Apply Saved Transforms"))
        {
            myScript.SetTransform();
        }
    }
}
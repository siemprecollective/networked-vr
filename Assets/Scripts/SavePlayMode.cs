using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePlayMode: MonoBehaviour
{
    public GameObject[] toSaveObjects;
    private string savepath = @".\saveplaymode.txt";

    public void SaveTransforms()
    {
        string[] towrite = new string[toSaveObjects.Length * 2];
        for (int i = 0; i < toSaveObjects.Length; i++)
        {
            towrite[2 * i] = Utils.GetFullName(toSaveObjects[i]);
            towrite[2 * i + 1] = Utils.TransformToString(toSaveObjects[i].transform);
        }
        System.IO.File.WriteAllLines(savepath, towrite);
    }
    
    public void SetTransform()
    {
        string[] lines = System.IO.File.ReadAllLines(savepath);

        if (lines.Length % 2 != 0)
        {
            Debug.LogError("Number of lines in save file not even");
            return;
        }

        List<GameObject> newListOfGameObjects = new List<GameObject>();

        for (int i = 0; i < lines.Length / 2; i++)
        {
            GameObject saveObject = GameObject.Find(lines[2 * i]);
            if (saveObject == null)
            {
                Debug.LogError("could not find gameobject with name: " + lines[2 * i]);
                continue;
            }

            var result = Utils.StringToTransform(lines[2 * i + 1]);
            saveObject.transform.localPosition = result.Position;
            saveObject.transform.localRotation = result.Rotation;
            saveObject.transform.localScale = result.Scale;
            newListOfGameObjects.Add(saveObject);
        }
        toSaveObjects = newListOfGameObjects.ToArray();
    }

}

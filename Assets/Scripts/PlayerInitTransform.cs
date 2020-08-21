using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInitTransform: MonoBehaviour
{
    public Vector3 phillipInitPos;
    public Quaternion phillipInitRot;
    public Vector3 cyrusInitPos;
    public Quaternion cyrusInitRot;
    public Vector3 kumailInitPos;
    public Quaternion kumailInitRot;

    void Start()
    {
        WhichPlayer whichPlayer = Object.FindObjectOfType<WhichPlayer>();
        switch (whichPlayer.localPlayer)
        {
            case Players.Phillip:
                transform.localPosition = phillipInitPos;
                transform.localRotation = phillipInitRot;
                break;
            case Players.Cyrus:
                transform.localPosition = cyrusInitPos;
                transform.localRotation = cyrusInitRot;
                break;
            case Players.Kumail:
                transform.localPosition = kumailInitPos;
                transform.localRotation = kumailInitRot;
                break;
        }
    }

    public void SaveCurrentTransform()
    {
        string savepath = @".\savedplaymodes\" + Utils.GetFullName(gameObject).Replace("/", "");
        System.IO.File.WriteAllText(savepath, Utils.TransformToString(transform));
    }

    public void WriteSavedTransform()
    {
        string savepath = @".\savedplaymodes\" + Utils.GetFullName(gameObject).Replace("/", "");
        string savedText = System.IO.File.ReadAllText(savepath);
        var savedTransform = Utils.StringToTransform(savedText);
        switch (WhichPlayer.returnLocalPlayer())
        {
            case Players.Phillip:
                phillipInitPos = savedTransform.Position;
                phillipInitRot = savedTransform.Rotation;
                break;
            case Players.Cyrus:
                cyrusInitPos = savedTransform.Position;
                cyrusInitRot = savedTransform.Rotation;
                break;
            case Players.Kumail:
                kumailInitPos = savedTransform.Position;
                kumailInitRot = savedTransform.Rotation;
                break;
        }
    }
}
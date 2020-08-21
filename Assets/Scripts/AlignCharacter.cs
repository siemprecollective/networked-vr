using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class AlignCharacter : MonoBehaviour
{
    public GameObject phillipDeskObject;
    public GameObject phillipDeskObjectRoom;
    public GameObject cyrusDeskObject;
    public GameObject kumailDeskObject;
    public GameObject leftHandObject;
    public GameObject rightHandObject;
    public GameObject OVRPlayerControllerObject;
    [HideInInspector]
    public bool isEnabled = false;

    private GameObject deskObject;
    private Vector3 offset;
    private Vector3 phillipOffset = new Vector3(0.7492f, 0.7048f, 0.3737f);
    private Vector3 phillipOffsetRoom = new Vector3(0.4979f, 0.7033f, 0.298f);
    private Vector3 cyrusOffset = new Vector3(0.75f, 0.703105f, 0.3746504f);
    private Vector3 kumailOffset = new Vector3(0.75f, 0.703105f, 0.37465f);

    private InputDevice rightHandInputDevice;
    bool primaryDown;
    bool secondaryDown;

    WhichPlayer whichPlayer;

    // Start is called before the first frame update
    void Start()
    {
        whichPlayer = GetComponent<WhichPlayer>();
        if (whichPlayer != null)
        {
            switch (whichPlayer.localPlayer)
            {
                case Players.Phillip:
                    if (whichPlayer.localSetup == Setups.Room)
                    {
                        deskObject = phillipDeskObjectRoom;
                        offset = phillipOffsetRoom;
                    }
                    else
                    {
                        deskObject = phillipDeskObject;
                        offset = phillipOffset;
                    }
                    break;
                case Players.Cyrus:
                    deskObject = cyrusDeskObject;
                    offset = cyrusOffset;
                    break;
                case Players.Kumail:
                    deskObject = kumailDeskObject;
                    offset = kumailOffset;
                    break;
            }
        }

        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            //Debug.Log(string.Format("Device name {0} with characteristics {1}", device.name, device.characteristics));
            var flag = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            if ((device.characteristics & flag) == flag)
            {
                rightHandInputDevice = device;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isEnabled)
        {
            return;
        }

        if (deskObject == null || offset == null)
        {
            Debug.LogError("Players not set");
            return;
        }

        bool secondaryPressedDown = !secondaryDown;
        bool secondaryPressedUp = secondaryDown;
        bool primaryPressedDown = !primaryDown;
        bool primaryPressedUp = primaryDown;
        rightHandInputDevice.TryGetFeatureValue(CommonUsages.gripButton, out secondaryDown);
        rightHandInputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryDown);
        secondaryPressedDown = secondaryPressedDown & secondaryDown;
        secondaryPressedUp = secondaryPressedUp & !secondaryDown;
        primaryPressedDown = primaryPressedDown & primaryDown;
        primaryPressedUp = primaryPressedUp & !primaryDown;

        Vector3 centerPoint = deskObject.transform.TransformPoint(new Vector3(0, offset.y, offset.z));
        Vector3 leftPos = leftHandObject.transform.position;
        Vector3 rightPos = rightHandObject.transform.position;
        Vector3 leftPoint = deskObject.transform.TransformPoint(new Vector3(offset.x, offset.y, offset.z));

        if (primaryPressedDown || secondaryPressedDown || primaryPressedUp || secondaryPressedUp)
        {
            Utils.ToggleMeshVisibilities(leftHandObject, rightHandObject);
        }

        if (primaryPressedUp)
        {
            Vector3 translateVector = centerPoint - (leftPos + rightPos) / 2;

            //Utils.createDebugCube(centerPoint, "centerpoint");
            //Utils.createDebugCube((leftPos + rightPos) / 2, "between hands");

            OVRPlayerControllerObject.transform.position += translateVector;
        }
        else if (secondaryPressedUp)
        {
            double rotateAngle = Utils.GetAngle(leftPoint, centerPoint, leftPos);
            OVRPlayerControllerObject.transform.RotateAround(centerPoint, Vector3.up, (float)rotateAngle * -1);
        }
    }
}

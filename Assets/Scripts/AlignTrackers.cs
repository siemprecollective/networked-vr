using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class AlignTrackers : MonoBehaviour
{
    public GameObject parentObject;
    public GameObject leftHandObject;
    public GameObject rightHandObject;
    [HideInInspector]
    public bool isEnabled = false;

    private List<XRNodeState> _nodeStates = new List<XRNodeState>();
    private Transform rightHandAnchor;
    private InputDevice rightHandInputDevice;
    bool primaryDown;
    bool secondaryDown;

    int currentIndex = -1;
    public GameObject[] currentCubes = new GameObject[] {null, null, null, null};
    private int[] numReferenceSamples = new int[4];

    // Start is called before the first frame update
    void Start()
    {
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            Debug.Log(string.Format("Device name {0} with characteristics {1} and serial {2}", device.name, device.characteristics, device.serialNumber));
            var flag = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            if ((device.characteristics & flag) == flag)
            {
                Debug.Log("Found the right hand controller");
                rightHandInputDevice = device;
            }
        }
        rightHandAnchor = rightHandObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isEnabled || currentIndex == -1)
        {
            return;
        }
        GameObject currentCube = currentCubes[currentIndex];

        _nodeStates.Clear();
        InputTracking.GetNodeStates(_nodeStates);

        bool secondaryPressedDown = !secondaryDown;
        bool secondaryPressedUp = secondaryDown;
        bool primaryPressedDown = !primaryDown;
        bool primaryPressedUp = primaryDown;
        rightHandInputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryDown);
        rightHandInputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryDown);
        secondaryPressedDown = secondaryPressedDown & secondaryDown;
        secondaryPressedUp = secondaryPressedUp & !secondaryDown;
        primaryPressedDown = primaryPressedDown & primaryDown;
        primaryPressedUp = primaryPressedUp & !primaryDown;

        foreach (var nodeState in _nodeStates) 
        {
            if (nodeState.nodeType == XRNode.LeftHand)
            {
                if (secondaryPressedDown || secondaryPressedUp || primaryPressedDown || primaryPressedUp)
                {
                    Utils.ToggleMeshVisibilities(leftHandObject, rightHandObject);
                }

                if (primaryPressedUp)
                {
                    Debug.Log("numReferenceSamples length: " + numReferenceSamples.Length + " currentIndex: " + currentIndex);
                    numReferenceSamples[currentIndex]++;
                    currentCube.transform.position = ((numReferenceSamples[currentIndex] - 1) * currentCube.transform.position + rightHandAnchor.position) / numReferenceSamples[currentIndex];
                }
                else if (secondaryPressedUp)
                {
                    currentCube.transform.LookAt(rightHandAnchor.position);
                }
            }
        }
    }

    public void selectCube(int i)
    {
        if (i < 0 || i >= currentCubes.Length)
        {
            return;
        }

        currentIndex = i;
        if (currentCubes[currentIndex] == null)
        {
            currentCubes[currentIndex] = Utils.CreateDebugCube(Vector3.zero, "Lighthouse " + currentIndex, 0.08f);
        }
    }
}

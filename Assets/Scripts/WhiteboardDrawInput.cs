using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Normal.Realtime;

public class WhiteboardDrawInput : MonoBehaviour
{
    public RealtimeDrawerManager _drawerManager;
    private WhiteboardDrawer _whiteboardDrawer;

    public GameObject[] whiteboards;
    public LaserPointer laserPointer;
    public Transform rightHandTransform;

    private InputDevice rightHandInputDevice;
    private float lastY = -1;
    private float lastZ = -1;

    // Start is called before the first frame update
    void Start()
    {
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            //Debug.Log(string.Format("Device name {0} with characteristics {1} and serial {2}", device.name, device.characteristics, device.serialNumber));
            var flag = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
            if ((device.characteristics & flag) == flag)
            {
                Debug.Log("Found the right hand controller");
                rightHandInputDevice = device;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_whiteboardDrawer == null)
        {
            _whiteboardDrawer = _drawerManager.getLocalDrawer();
            if (_whiteboardDrawer == null)
                return;
        }

        bool gripDown;
        rightHandInputDevice.TryGetFeatureValue(CommonUsages.gripButton, out gripDown);
        RaycastHit hit;
        if(Physics.Raycast(rightHandTransform.position, rightHandTransform.forward, out hit))
        {
            for (int i = 0; i < whiteboards.Length; i++)
            {
                if (hit.transform.gameObject == whiteboards[i])
                {
                    laserPointer.SetCursorStartDest(rightHandTransform.position, hit.point, hit.normal);
                    _whiteboardDrawer.SetWhiteboardID(i);
                    if (gripDown)
                    {
                        Vector3 hitPosition = whiteboards[i].transform.InverseTransformPoint(hit.point);
                        _whiteboardDrawer.SetDrawPoints(new float[] {hitPosition.y, hitPosition.z});
                    }
                }
            }
        }
        else
        {
            laserPointer.SetCursorRay(rightHandTransform);
        }
        if (!gripDown)
        {
            _whiteboardDrawer.ResetLastPoints();
        }
    }
}
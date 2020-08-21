using System.Collections;
using System.Collections.Generic;
using RockVR.Video;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class MenuManager : MonoBehaviour
{
    public GameObject canvasObject;
    public AlignCharacter alignCharacter;
    public AlignTrackers alignTrackers;

    public GameObject mainPage;
    public GameObject alignCharacterPage;
    public GameObject alignTrackersPage;
    public GameObject recordPage;
    public Text recordText;

    private InputDevice leftHandInputDevice;
    bool primaryDown;

    private VideoCaptureCtrl videoControl;

    // Start is called before the first frame update
    void Start()
    {
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            var flag = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            if ((device.characteristics & flag) == flag)
            {
                leftHandInputDevice = device;
            }
        }

        videoControl = GetComponent<VideoCaptureCtrl>();
        if (videoControl == null)
        {
            Debug.LogError("could not find video control!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool primaryPressedDown = primaryDown;
        leftHandInputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryDown);
        primaryPressedDown = primaryPressedDown & !primaryDown;

        // Debug.Log(string.Format("is primary pressed: {0}", primaryPressedDown));
        if (primaryPressedDown)
        {
            if (canvasObject.activeSelf)
            {
                DisableAll();
            }
            else
            {
                mainPage.SetActive(true);
            }
            canvasObject.SetActive(!canvasObject.activeSelf);
        }
    }

    public void OnClickAlignCharacter()
    {
        mainPage.SetActive(false);
        alignCharacterPage.SetActive(true);
        alignCharacter.isEnabled = true;
    }

    public void OnClickAlignTrackers()
    {
        mainPage.SetActive(false);
        alignTrackersPage.SetActive(true);
        alignTrackers.isEnabled = true;
    }

    public void OnClickTrackerIndex(int index)
    {
        alignTrackers.selectCube(index);
    }

    public void OnClickRecord()
    {
        mainPage.SetActive(false);
        recordPage.SetActive(true);

        foreach(var videoCapture in videoControl.videoCaptures)
        {
            videoCapture.gameObject.SetActive(true);
        }
    }

    public void OnClickDone()
    {
        DisableAll();
        mainPage.SetActive(true);
    }

    public void ToggleRecording()
    {
        if (videoControl.status == VideoCaptureCtrlBase.StatusType.STARTED)
        {
            recordText.text = "Start Recording";
            videoControl.StopCapture();
        }
        else
        {
            recordText.text = "Stop Recording";
            videoControl.StartCapture();
        }
    }

    void DisableAll()
    {
        alignCharacter.isEnabled = false;
        alignTrackers.isEnabled = false;
        if (videoControl.status == VideoCaptureCtrlBase.StatusType.STARTED)
        {
            videoControl.StopCapture();
        }
        foreach(var videoCapture in videoControl.videoCaptures)
        {
            videoCapture.gameObject.SetActive(false);
        }
        alignCharacterPage.SetActive(false);
        alignTrackersPage.SetActive(false);
        recordPage.SetActive(false);
    }
}

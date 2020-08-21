using Normal.Realtime;
using OVRTouchSample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMultiplayerAvatar : MonoBehaviour
{
    RealtimeAvatar realtimeAvatar;

    // Start is called before the first frame update
    void Start()
    {
        realtimeAvatar = gameObject.GetComponent<RealtimeAvatar>();
        if (realtimeAvatar.hasCustomHands && !realtimeAvatar.realtimeView.isOwnedLocally)
        {
            OVRGrabber[] grabberComponents = gameObject.GetComponentsInChildren<OVRGrabber>();
            Hand[] handComponents = gameObject.GetComponentsInChildren<Hand>();

            foreach(OVRGrabber grabberComponent in grabberComponents)
            {
                grabberComponent.enabled = false;
            }
            foreach(Hand handComponent in handComponents)
            {
                handComponent.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

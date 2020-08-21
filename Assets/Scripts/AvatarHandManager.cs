using Leap.Unity;
using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarHandManager : MonoBehaviour
{
    public RiggedHand m_handLeft;
    public RiggedHand m_handRight;
    private RealtimeIKAvatar avatarComponent;

    // Start is called before the first frame update
    void Start()
    {
        avatarComponent = gameObject.GetComponent<RealtimeIKAvatar>();
        if (avatarComponent != null)
        {
            var handModelManager = avatarComponent.localPlayer.leapHandModelManager.GetComponent<HandModelManager>();
            if (handModelManager != null)
            {
                handModelManager.AddNewGroup("localPlayer", m_handLeft, m_handRight);
            }
        }

        // NOTE: This looks for all realtimetransforms, not just the ones in the hands
        RealtimeTransform[] allTransforms = gameObject.GetComponentsInChildren<RealtimeTransform>();
        foreach(var transform in allTransforms)
        {
            transform.RequestOwnership();
        }
    }

    private void Update()
    {
        if (avatarComponent != null)
        {
            avatarComponent.SetFingerPoses();
            avatarComponent.SetIKTargets();
            avatarComponent.SetVisemes();
            avatarComponent.SetOculusBlendshapes();
            avatarComponent.SetEyeData();
            avatarComponent.SetLipData();
        }
    }
}

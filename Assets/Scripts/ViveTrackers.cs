using Normal.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

public class ViveTrackers : MonoBehaviour
{
    public GameObject parentObject;
    [Tooltip("Start with the four lighthouses, and then the trackers 1-12")]
    public GameObject[] trackedObjects = new GameObject[16];
    public Transform[] lighthouseTargets = new Transform[4];
    public Transform centerEyeAnchor;
    public Transform viveCoordinateSpace;
    private Dictionary<ulong, int> trackerIDS;
    private List<XRNodeState> _nodeStates;

    private Vector3 origin = Vector3.zero;
    private Quaternion originRot = Quaternion.identity;
    private bool aligned = false;
    private bool alignedSecond = false;

    void Start()
    {
        _nodeStates = new List<XRNodeState>();
        trackerIDS = new Dictionary<ulong, int>
        {
            { 31628832369, 0 },
            { 32637070632, 1 },
            { 33299742229, 2 },
            { 30065481399, 3 },
            { 37510893349, 4 },
            { 37556337732, 5 },
            { 35951395168, 6 },
            { 34648218348, 7 },
            { 37127521156, 8 },
            { 35702446844, 9 },
            { 37732245346, 10 },
            { 37789037866, 11 },
            { 37302631348, 12 },
            { 36124594263, 13 },
            { 34360659629, 14 },
            { 35930516319, 15 }
        };

        foreach (var tracked in trackedObjects)
        {
            tracked.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _nodeStates.Clear();
        InputTracking.GetNodeStates(_nodeStates);

        Vector3 sumLighthousePositions = Vector3.zero;
        Vector3 avgTargetPosition = Vector3.zero;
        int numLighthouses = 0;

        // first, pick a lighthouse to be the origin
        if (!aligned)
        {
            foreach (var nodeState in _nodeStates)
            {
                if (nodeState.nodeType != XRNode.TrackingReference) continue;
                if (!(nodeState.TryGetPosition(out origin) && nodeState.TryGetRotation(out originRot))) continue;
                if (!(trackerIDS.ContainsKey(nodeState.uniqueID))) continue;
                Transform target = lighthouseTargets[trackerIDS[nodeState.uniqueID]];

                viveCoordinateSpace.transform.position = target.position;
                viveCoordinateSpace.transform.Rotate(new Vector3(0, target.rotation.eulerAngles.y - originRot.eulerAngles.y, 0));
                break;
            }
            if (origin == Vector3.zero)
            {
                // Debug.LogError("Couldn't find lighthouses!");
                return;
            }
            aligned = true;
        }

        foreach (var nodeState in _nodeStates)
        {
            //Debug.Log(string.Format("Found {1}, is unique ID {0}", nodeState.uniqueID, nodeState.nodeType));
            /*
            if (nodeState.nodeType == XRNode.CenterEye)
            {
                Vector3 position;
                Quaternion rotation;
                if (!(nodeState.TryGetPosition(out position) && nodeState.TryGetRotation(out rotation))) continue;

                Transform player = centerEyeAnchor.parent.parent.parent;
                Vector3 desiredPosition = viveCoordinateSpace.transform.position + position - origin;
                Vector3 actualPosition = centerEyeAnchor.transform.position;
                player.transform.position += desiredPosition - actualPosition;
                // Debug.Log($"centerEye: {centerEyeAnchor.transform.position}");
                // Debug.Log($"desired: {desiredPosition}");
                GameObject cube = Utils.CreateDebugCube(Vector3.zero);
                cube.transform.parent = viveCoordinateSpace;
                cube.transform.localPosition = position - origin;
            }
            */
            if (nodeState.nodeType == XRNode.HardwareTracker || nodeState.nodeType == XRNode.TrackingReference)
            {
                Vector3 position;
                Quaternion rotation;
                if (!(nodeState.TryGetPosition(out position) && nodeState.TryGetRotation(out rotation))) continue;
                if (!(trackerIDS.ContainsKey(nodeState.uniqueID))) continue;
                GameObject tracked = trackedObjects[trackerIDS[nodeState.uniqueID]];
                if (tracked == null) continue;
                
                RealtimeTransform rtTrans = tracked.GetComponent<RealtimeTransform>();
                if (rtTrans != null && !rtTrans.isOwnedLocally)
                {
                    rtTrans.RequestOwnership();
                }
                tracked.transform.localPosition = position - origin;
                tracked.transform.localRotation = rotation;

                if (trackerIDS[nodeState.uniqueID] <= 3)
                {
                    sumLighthousePositions += position;
                    avgTargetPosition += lighthouseTargets[trackerIDS[nodeState.uniqueID]].transform.position;
                    numLighthouses++;
                }
            }
        }

        if (!aligned && WhichPlayer.returnLocalPlayer() == Players.Phillip)
        {
            Transform target = lighthouseTargets[0];
            parentObject.transform.position = target.position;
            parentObject.transform.Rotate(new Vector3(0, target.rotation.eulerAngles.y, 0));

            aligned = true;
        }

        foreach(var tracked in trackedObjects)
        {
            if (tracked.transform.localPosition != Vector3.zero && tracked.transform.localRotation != Quaternion.Euler(0, 0, 0))
            {
                if (!tracked.activeSelf)
                {
                    tracked.SetActive(true);
                }
            }
        }
        /*
        if (aligned && !alignedSecond)
        {
            Vector3 avgPosition = parentObject.transform.TransformPoint(sumLighthousePositions / numLighthouses);
            avgTargetPosition /= numLighthouses;
            Utils.CreateDebugCube(avgPosition, "sum of my lighthouse positions");
            Utils.CreateDebugCube(avgTargetPosition, "average target position");
            parentObject.transform.position += avgTargetPosition - avgPosition;

            alignedSecond = true;
        }
        */
    }
}

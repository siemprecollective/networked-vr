using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

[RequireComponent(typeof(Realtime))]
public class RealtimeDrawerManager : MonoBehaviour
{
    public GameObject drawPrefab;

    public Dictionary<int, WhiteboardDrawer> drawers { get; private set; }

    private Realtime _realtime;

    void Awake()
    {
       _realtime = GetComponent<Realtime>();
       _realtime.didConnectToRoom += DidConnectToRoom; 

       drawers = new Dictionary<int, WhiteboardDrawer>();
    }

    void DidConnectToRoom(Realtime room)
    {
        CreateDrawObject();
    }

    public void RegisterDrawer(int clientID, WhiteboardDrawer drawer)
    {
        drawers[clientID] = drawer;
    }

    public void UnregisterDrawer(WhiteboardDrawer drawer)
    {
        List<KeyValuePair<int, WhiteboardDrawer>> matchingDrawers = drawers.Where(keyValuePair => keyValuePair.Value == drawer).ToList();
        foreach (KeyValuePair<int, WhiteboardDrawer> matchingDrawer in matchingDrawers)
        {
            int avatarClientID = matchingDrawer.Key;
            drawers.Remove(avatarClientID);
        }
    }

    void CreateDrawObject()
    {
        if (!_realtime.connected)
        {
            Debug.LogError("RealtimeDrawManager: Unable to create draw. Realtime is not connected to a room.");
            return;
        }

        if (drawPrefab == null)
        {
            Debug.LogWarning("Draw prefab is null. No prefab will be instantiated.");
            return;
        }

        GameObject drawGameObject = Realtime.Instantiate(drawPrefab.name, true, true, true, _realtime);
        if (drawGameObject == null)
        {
            Debug.LogError("RealtimeDrawManager: Failed to instantiate prefab for the local player.");
            return;
        }
    }

    public WhiteboardDrawer getLocalDrawer()
    {
        if (drawers.ContainsKey(_realtime.clientID))
            return drawers[_realtime.clientID];
        return null;
    }
}

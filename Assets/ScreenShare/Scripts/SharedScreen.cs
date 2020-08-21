using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HMONITOR = System.IntPtr;
using HWND = System.IntPtr;

class SharedScreen : MonoBehaviour {
    public Players owner = Players.Kumail;
    public uint desiredWidth;
    public uint desiredHeight;
    public int sessionNumber;
    public int monitorNumber;

    private HMONITOR monitorHandle = IntPtr.Zero;
    private HWND windowHandle = IntPtr.Zero;
    
    private Dictionary<string, string> kumailSessions = new Dictionary<string, string>(){
        ["cdaca222e87cdf10bf486002e4fea8b6e5681c3570e52fda37a1333911110b2a"] = "1X2wo6cgLnnhEF5HVElAw8cvp8C"
    };
    private Dictionary<string, string> phillipSessions = new Dictionary<string, string>(){
        ["5b98f87f05af1e1a880e6cbaae520218ba056ae53c616cf67f197c7ceea1e583"] = "1X6nrIOPPVlwvtexoBee6y2FVVk",
        ["1897a47070bb44eecc10791e136ce7aba76c9a0b02765ca97815c87241b0465f"] = "1Yj8Ch94YzUvHpTKwqZ4eSYS2Kb"
    };
    private Dictionary<string, string> cyrusSessions = new Dictionary<string, string>(){
        ["7da0f64a7cc8e434c44ba8e0957bedfb71e6c0faea783782e1ba8839acdf251a"] = "1X6nqByCYGkFAD2ce03UvKGPYGY",
        ["29f0c7c5f2da438957c115d85d8c44af25978b4300116551f1d072fbe87dc64a"] = "1YdUjKBtVn9dI7pn4gH7yML2Rnf",
        ["d61a2c7155379da8a3f8df15cb0543cc007bdd387205d47a42a279b96d9e42c4"] = "1Yj7oCYWepyVb3PnhG3pntoJZr2"
    };
    
    private RenderTexture texture;
    private RemoteScreen remoteScreen;
    private LocalScreen localScreen;

    public void OnEnable() {
        texture = new RenderTexture((int)desiredWidth, (int)desiredHeight, 24, RenderTextureFormat.Default, 3);
        texture.autoGenerateMips = true;
        texture.anisoLevel = 16;
        GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
        
        string sessionId = "";
        string peerId = "";
        Players player = WhichPlayer.returnLocalPlayer();
        
        switch (owner)
        {
            case Players.Phillip:
                sessionId = phillipSessions.Keys.ToArray()[sessionNumber];
                peerId = phillipSessions[sessionId];
                break;
            case Players.Kumail:
                sessionId = kumailSessions.Keys.ToArray()[sessionNumber];
                peerId = kumailSessions[sessionId];
                break;
            case Players.Cyrus:
                sessionId = cyrusSessions.Keys.ToArray()[sessionNumber];
                peerId = cyrusSessions[sessionId];
                break;
        }
        
        if (player == owner) {
            monitorHandle = LocalScreenUtility.GetMonitors()[monitorNumber];
            localScreen = new LocalScreen(sessionId, windowHandle, monitorHandle);
        } else {
            remoteScreen = new RemoteScreen(sessionId, peerId, desiredWidth, desiredHeight);
        }

        StartCoroutine("CallPluginAtEndOfFrames");
    }

    public void OnDisable() {
        StopCoroutine("CallPluginAtEndOfFrames");
        
        if (remoteScreen != null) remoteScreen.Shutdown();;
        if (localScreen != null) localScreen.Dispose();
    }

    public void OnDestroy() {
        if (remoteScreen != null) remoteScreen.Dispose();
    }

    private IEnumerator CallPluginAtEndOfFrames()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            RenderTexture.active = texture;
            if (localScreen != null) {
                localScreen.Render();
            } else if (remoteScreen != null) {
                remoteScreen.Render();
            }
        }
    }
}
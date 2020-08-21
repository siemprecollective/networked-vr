using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

class RemoteScreen : IDisposable
{
    [DllImport("ScreenShare")]
    private static extern IntPtr NewRemoteScreen(
        string sessionId, string peerId, uint desiredWidth, uint desiredHeight
    );
    [DllImport("ScreenShare")]
    private static extern void DestroyRemoteScreen(IntPtr rs);
    [DllImport("ScreenShare")]
    private static extern IntPtr ShutdownRemoteScreen(IntPtr rs);
    [DllImport("ScreenShare")]
    private static extern int RemoteScreenToEvent(IntPtr remoteScreen);
    [DllImport("ScreenShare")]
    private static extern IntPtr GetRemoteScreenRenderEventFunc();

    private IntPtr nativeScreen;

    public RemoteScreen(string sessionId, string peerId, uint desiredWidth, uint desiredHeight)
    {
        nativeScreen = NewRemoteScreen(
            sessionId, peerId, desiredWidth, desiredHeight
        );
    }

    public void Dispose() {
        if (nativeScreen == IntPtr.Zero) return;
        DestroyRemoteScreen(nativeScreen);
        nativeScreen = IntPtr.Zero;
    }

    public void Shutdown()
    {
        if (nativeScreen == IntPtr.Zero) return;
        ShutdownRemoteScreen(nativeScreen);
    }

    public void Render()
    {
        GL.IssuePluginEvent(GetRemoteScreenRenderEventFunc(), RemoteScreenToEvent(nativeScreen));
    }
}
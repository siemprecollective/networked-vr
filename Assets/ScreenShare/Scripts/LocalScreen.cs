using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using HWND = System.IntPtr;
using HMONITOR = System.IntPtr;
class LocalScreen : IDisposable
{
    [DllImport("ScreenShare")]
    private static extern IntPtr NewLocalScreen(string sessionId, HWND hWnd, HMONITOR hMon);
    [DllImport("ScreenShare")]
    private static extern void DestroyLocalScreen(IntPtr rs);
    [DllImport("ScreenShare")]
    private static extern int LocalScreenToEvent(IntPtr localScreen);
    [DllImport("ScreenShare")]
    private static extern IntPtr GetLocalScreenRenderEventFunc();

    private IntPtr nativeScreen = IntPtr.Zero;

    public LocalScreen(string sessionId, HWND hWND, HMONITOR hMon)
    {
        nativeScreen = NewLocalScreen(sessionId, hWND, hMon);
    }

    public void Dispose()
    {
        if (nativeScreen == IntPtr.Zero) return;
        DestroyLocalScreen(nativeScreen);
        nativeScreen = IntPtr.Zero;
    }

    public void Render() {
        if (nativeScreen == IntPtr.Zero) return;
        GL.IssuePluginEvent(GetLocalScreenRenderEventFunc(), LocalScreenToEvent(nativeScreen));
    }
}
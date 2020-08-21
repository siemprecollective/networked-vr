using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

using HWND = System.IntPtr;
using HMONITOR = System.IntPtr;

public class LocalScreenUtility 
{
    [DllImport("USER32.DLL")]
    private static extern IntPtr GetShellWindow();
    private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
    [DllImport("USER32.DLL")]
    private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

    [DllImport("USER32.DLL")]
    private static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("USER32.DLL")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    delegate bool EnumMonitorsProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);
    [DllImport("USER32.DLL")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsProc lpfnEnum, IntPtr dwData);

    public static Dictionary<string, HWND> GetWindows()
    {
        HWND shellWindow = GetShellWindow();
        Dictionary<string, HWND> windows = new Dictionary<string, HWND>();

        EnumWindows(delegate (HWND hWnd, int lParam)
        {
            if (hWnd == shellWindow) return true;
            if (!IsWindowVisible(hWnd)) return true;

            int length = GetWindowTextLength(hWnd);
            if (length == 0) return true;

            StringBuilder builder = new StringBuilder(length);
            GetWindowText(hWnd, builder, length + 1);
            windows[builder.ToString()] = hWnd;
            return true;
        }, 0);

        return windows;
    }

    public static List<HMONITOR> GetMonitors()
    {
        List<HMONITOR> monitors = new List<HMONITOR>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
        delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
        {
            monitors.Add(hMonitor);
            return true;
        }, IntPtr.Zero);

        return monitors;
    }
}
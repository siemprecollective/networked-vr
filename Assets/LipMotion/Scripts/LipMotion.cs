using System;
using System.Runtime.InteropServices;
public class LipMotion : IDisposable {

    public const int NUM_POINTS = 11;

    [DllImport("LipMotion.dll")]
    private static extern IntPtr LipMotion_init(int camera, bool debugWindow);
    [DllImport("LipMotion.dll")]
    private static extern void LipMotion_destroy(IntPtr native);
    [DllImport("LipMotion.dll")]
    private static extern IntPtr LipMotion_processFrame();

    private IntPtr nativeLipMotion = IntPtr.Zero;
    public LipMotion(int camera, bool debugWindow) {
        nativeLipMotion = LipMotion_init(camera, debugWindow);
    }
    
    public void Dispose() {
        LipMotion_destroy(nativeLipMotion);
        nativeLipMotion = IntPtr.Zero;
    }

    [Serializable]
    public struct Point {
        public int x, y;
    }
    public Point[] processFrame() {
        var points = new Point[NUM_POINTS];
        if (nativeLipMotion == IntPtr.Zero) return points;
        
        IntPtr nativeShapes = LipMotion_processFrame();
        for (int i=0; i < NUM_POINTS; ++i) {
            points[i].x = (int) Marshal.ReadIntPtr(nativeShapes, i*2*sizeof(int));
            points[i].y = (int) Marshal.ReadIntPtr(nativeShapes, (i*2+1)*sizeof(int));
        }
        return points;
    }
}
using UnityEngine;
using System;
using System.Linq;

public class LipMotionInterface : MonoBehaviour {
    public int cvCamera = 0;
    public bool debugWindow = false;

    private LipMotion lm;

    public void OnEnable() {
        lm = new LipMotion(cvCamera, debugWindow);
    }

    public void OnDisable() {
        lm.Dispose();
    }

    public LipMotion.Point[] processFrame() {
        return lm.processFrame();
    }

    /*
    public void Update() {
        if (lipBones.Length < 11) return;

        var points = lm.processFrame();
        var upperLip0 = points[0];
        var upperLip1 = points[1];
        var upperLip2 = points[3];
        var upperLip3 = points[4];
        var lowerLip0 = points[8];
        var lowerLip1 = points[7];
        var lowerLip2 = points[6];
        var lowerLip3 = points[5];
        var chin0 = points[9];
        var chin1 = points[10];
        var chin2 = points[2];

        var upperLipX = upperLip3.x - upperLip0.x;
        var upperLipXWorld = (lipBones[4].transform.position - lipBones[0].transform.position).magnitude;
        var renorm = upperLipXWorld / upperLipX;

        var upperLip1World = lipBones[0].position;
        var origin = upperLip1World;

        for (int i=0; i<11; ++i) {
            var xOffset = ((float) (points[i].x - upperLip0.x)) * renorm;
            var yOffset = ((float) (points[i].y - upperLip0.y)) * renorm;
            lipBones[i].localPosition = lipBones[i].localPosition + (new Vector3(xOffset, 0, -yOffset));
        }
    }
    */
}
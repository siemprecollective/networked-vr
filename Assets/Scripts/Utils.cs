using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static void ToggleMeshVisibilities(GameObject leftHandObject, GameObject rightHandObject)
    {
        Component[] leftRenderers = leftHandObject.GetComponentsInChildren(typeof(MeshRenderer));
        Component[] rightRenderers = rightHandObject.GetComponentsInChildren(typeof(MeshRenderer));
        List<Component> renderers = new List<Component>(leftRenderers);
        renderers.AddRange(rightRenderers);
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = !renderer.enabled;
        }
    }

    public static GameObject CreateDebugCube(Vector3 position, string label = "", float scale=0.05f)
    {
        if (!label.Equals(""))
        {
            Debug.Log(string.Format("{0} position: {1}", label, position.ToString("F7")));
        }

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.localScale = new Vector3(scale, scale, scale);
        Destroy(cube.GetComponent<BoxCollider>());
        return cube;
    }

    // Returns angle between AB and BC using Law of Cosines
    public static double GetAngle(Vector3 a, Vector3 b, Vector3 c)
    {
        return Mathf.Rad2Deg * (Mathf.Atan2(a.z - b.z, a.x - b.x) - Mathf.Atan2(c.z - b.z, c.x - b.x));
    }

    public static double GetAngleY(Vector3 a, Vector3 b, Vector3 c)
    {
        return Mathf.Rad2Deg * (Mathf.Atan2(a.y - b.y, a.x - b.x) - Mathf.Atan2(c.y - b.y, c.x - b.x));
    }

    public static string TransformToString(Transform transform)
    {
        Vector3 pos = transform.localPosition;
        Vector3 rot = transform.localRotation.eulerAngles;
        Vector3 scale = transform.localScale;
        return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}", pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, scale.x, scale.y, scale.z);
    }

    public static (Vector3 Position, Quaternion Rotation, Vector3 Scale) StringToTransform(string format)
    {
        List<string> tokens = new List<string>(format.Split(new string[] {", "}, System.StringSplitOptions.RemoveEmptyEntries));
        List<float> values = tokens.ConvertAll<float>(token => float.Parse(token));
        Vector3 pos = new Vector3(values[0], values[1], values[2]);
        Quaternion rot = Quaternion.Euler(values[3], values[4], values[5]);
        Vector3 scale = new Vector3(values[6], values[7], values[8]);
        return (pos, rot, scale);
    }

    public static string GetFullName (GameObject go) {
		string name = go.name;
		while (go.transform.parent != null) {
			go = go.transform.parent.gameObject;
			name = go.name + "/" + name;
		}
		return name;
	}
}
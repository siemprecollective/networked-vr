using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanAnimation : MonoBehaviour
{
    public GameObject fanBladesObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        fanBladesObject.transform.Rotate(0, 0, 1.0f, Space.Self);
    }
}

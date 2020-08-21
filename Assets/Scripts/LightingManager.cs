using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingManager : MonoBehaviour
{
    //====================================================
    //timing variables

    public bool useArtificialTime;
    public int artificialHour;
    public int artificialMinute;
    public float timeScale;

    //6:56
    public int sunriseHour;
    public int sunriseMinute;
    private float sunriseTime;

    //17:13
    public int sunsetHour;
    public int sunsetMinute;
    private float sunsetTime;

    private int currentHour;
    private int currentMinute;

    //====================================================
    //materials, objects for the ceiling fan

    public GameObject fanLight1;
    public GameObject fanLight2;
    public GameObject fanLight3;
    public GameObject fanLight4;
    public Material fanLightMaterialOn;
    public Material fanLightMaterialOff;

    public GameObject fanBulb1;
    public GameObject fanBulb2;
    public GameObject fanBulb3;
    public GameObject fanBulb4;
    public Material fanBulbMaterialOn;
    public Material fanBulbMaterialOff;

    public GameObject fanLightGroup;

    public bool fanLightState;
    private bool fanLightPrevState;

    //====================================================
    //adjusting the sun

    public GameObject sunGroup;
    public Light[] sunLights;
    private float[] maxIntensity;
    public float sunDistance;

    //====================================================

    public Material skyMaterial;

    // Start is called before the first frame update
    void Start()
    {
        fanLightPrevState = !fanLightState;

        sunriseTime = ConvertTimeToFloat(sunriseHour, sunriseMinute);
        sunsetTime = ConvertTimeToFloat(sunsetHour, sunsetMinute);

        maxIntensity = new float[sunLights.Length];
        for (int i = 0; i < sunLights.Length; i++)
        {
            maxIntensity[i] = sunLights[i].intensity;
        }
    }
    //returns between [0.0, 24.0)
    float ConvertTimeToFloat(int hour, int minute)
    {
        return hour + (1.0f * minute / 60.0f);
    }

    // Update is called once per frame
    void Update()
    {
        //retrieve current time
        if (useArtificialTime)
        {
            currentHour = (artificialHour + (artificialMinute/60) % 24);
            currentMinute = (artificialMinute % 60);
        }
        else
        {
            System.DateTime current = System.DateTime.Now;
            currentHour = current.Hour;
            currentMinute = current.Minute;
        }

        //calculate angle based on current time
        float currentTime = ConvertTimeToFloat(currentHour, currentMinute);
        float angle = 0.0f;
        float blend = 0.0f;
        if((currentTime >= sunriseTime)&&(currentTime < sunsetTime))
        {
            angle = ((currentTime - sunriseTime) / (sunsetTime - sunriseTime));
        }
        else
        {
            if(currentTime >= sunsetTime)
            {
                angle = (1.0f + ((currentTime - sunsetTime) / (sunriseTime + 24.0f - sunsetTime)));
            }
            else // currentTime < sunriseTime
            {
                angle = (1.0f + ((currentTime + 24.0f - sunsetTime) / (sunriseTime + 24.0f - sunsetTime)));
            }
        }

        //apply angle to sun rotation
        sunGroup.transform.position = new Vector3(0, sunDistance * Mathf.Sin(Mathf.PI * angle), sunDistance * Mathf.Cos(Mathf.PI * angle));
        sunGroup.transform.LookAt(Vector3.zero);
        
        for(int i = 0; i < sunLights.Length; i++)
        {
            sunLights[i].intensity = maxIntensity[i] * (Mathf.Max(0.0f, Mathf.Sin(Mathf.PI * angle)));
        }

        //convert angle into the shader parameter
            //0 is Daylight
            //1 is Nighttime
        float temp = -Mathf.Sin(Mathf.PI * angle);
        if (temp > 0)
        {
            blend = 0.75f + (0.25f * temp);
        }
        else
        {
            blend = 0.75f * (1.0f + temp);
        }

        skyMaterial.SetFloat("_Blend", blend);

        if (fanLightState != fanLightPrevState)
        {
            if (fanLightState) //turn ON
            {
                fanLightGroup.SetActive(true);
                
                //change the fan light shell materials
                fanLight1.GetComponent<Renderer>().material = fanLightMaterialOn;
                fanLight2.GetComponent<Renderer>().material = fanLightMaterialOn;
                fanLight3.GetComponent<Renderer>().material = fanLightMaterialOn;
                fanLight4.GetComponent<Renderer>().material = fanLightMaterialOn;

                //change the fan light bulb materials
                fanBulb1.GetComponent<Renderer>().material = fanBulbMaterialOn;
                fanBulb2.GetComponent<Renderer>().material = fanBulbMaterialOn;
                fanBulb3.GetComponent<Renderer>().material = fanBulbMaterialOn;
                fanBulb4.GetComponent<Renderer>().material = fanBulbMaterialOn;
            }
            else //turn OFF
            {
                fanLightGroup.SetActive(false);
                //change the fan light shell materials
                fanLight1.GetComponent<Renderer>().material = fanLightMaterialOff;
                fanLight2.GetComponent<Renderer>().material = fanLightMaterialOff;
                fanLight3.GetComponent<Renderer>().material = fanLightMaterialOff;
                fanLight4.GetComponent<Renderer>().material = fanLightMaterialOff;

                //change the fan light bulb materials
                fanBulb1.GetComponent<Renderer>().material = fanBulbMaterialOff;
                fanBulb2.GetComponent<Renderer>().material = fanBulbMaterialOff;
                fanBulb3.GetComponent<Renderer>().material = fanBulbMaterialOff;
                fanBulb4.GetComponent<Renderer>().material = fanBulbMaterialOff;
            }
            fanLightPrevState = fanLightState;
        }
    }
}

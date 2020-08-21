using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum Players
{
    Phillip = 0,
    Cyrus = 1,
    Kumail = 2
}

public enum Setups
{
    Office = 0,
    Room = 1
}

public class WhichPlayer : MonoBehaviour
{
    public GameObject[] officeObjects;
    public GameObject[] roomObjects;

    [HideInInspector]
    public Players localPlayer;
    [HideInInspector]
    public Setups localSetup;

    private Vector3[] roomPositions = {
        Vector3.zero,
        new Vector3(3.3135f, -0.514f, 0.0275f),
        new Vector3(2.956775f, 1.73802f, 2.434563f),
    };

    private Quaternion[] roomRotations = {
        Quaternion.identity,
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(18.87028f, 310.9713f, 0),
    };

    void Awake()
    {
        localPlayer = returnLocalPlayer();
        localSetup = returnSetup(); 

        if (!(officeObjects.Length == roomObjects.Length && roomObjects.Length == roomPositions.Length
              && roomPositions.Length == roomRotations.Length))
        {
            Debug.LogError("Not the same lenght arrays! which player awake");
        }
        else
        {
            if (localSetup == Setups.Room)
            {
                for (int i = 0; i < officeObjects.Length; i++)
                {
                    officeObjects[i].SetActive(false);
                    roomObjects[i].SetActive(true);
                    if (!roomPositions[i].Equals(Vector3.zero))
                    {
                        roomObjects[i].transform.localPosition = roomPositions[i];
                    }
                    if (roomRotations[i].Equals(Quaternion.identity))
                    {
                        roomObjects[i].transform.localRotation = roomRotations[i];
                    }
                }
            }
        }
    }

    public static Players returnLocalPlayer()
    {
        string path = @".\player.txt";
        if (File.Exists(path))
        {
            using (StreamReader sr = File.OpenText(path))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string processed = s.Replace(" ", "").ToLower();
                    if (processed.Equals("phillip"))
                    {
                        return Players.Phillip;
                    }
                    else if (processed.Equals("cyrus"))
                    {
                        return Players.Cyrus;
                    }
                    else if (processed.Equals("kumail"))
                    {
                        return Players.Kumail;
                    }
                    else
                    {
                        Debug.LogError("Did not recognize player in player.txt file!");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Could not find player.txt file!");
        }
        return Players.Phillip;
    }

    public static Setups returnSetup()
    {
        string path = @".\setup.txt";
        if (File.Exists(path))
        {
            using (StreamReader sr = File.OpenText(path))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    string processed = s.Replace(" ", "").ToLower();
                    if (processed.Equals("office"))
                    {
                        return Setups.Office;
                    }
                    else if (processed.Equals("room"))
                    {
                        return Setups.Room;
                    }
                    else
                    {
                        Debug.LogError("Did not recognize player in player.txt file!");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Could not find player.txt file!");
        }
        return Setups.Office;
    }
}

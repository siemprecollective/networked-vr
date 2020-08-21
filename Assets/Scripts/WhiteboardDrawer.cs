using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Normal.Realtime;
using System;

public class WhiteboardDrawer : RealtimeComponent
{
    private RealtimeDrawerManager _realtimeDrawerManager;

    private GameObject[] whiteboards;

    private uint drawUpdateFrames = 3;
    private uint numMegaPixel = 5;
    private uint currentFrame = 0;
    private Texture2D lastTexture;
    private bool pendingWrite = false;
    private Color[] brush;
    private float lastY = -1;
    private float lastZ = -1;

    private WhiteboardDrawModel _model;
    private WhiteboardDrawModel model
    {
        set
        {
            if (_model != null)
            {
                _model.drawPointsDidChange -= DrawPointsDidChange;
                _model.whiteboardIDDidChange -= WhiteboardIDDidChange;
            }

            _model = value;

            if (_model != null)
            {
                _model.drawPointsDidChange += DrawPointsDidChange;
                _model.whiteboardIDDidChange += WhiteboardIDDidChange;
                if (whiteboards != null)
                {
                    lastTexture = (Texture2D)whiteboards[_model.whiteboardID].GetComponent<Renderer>().materials[0].mainTexture;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //lastTexture = new Texture2D(3221, 2520);
        brush = new Color[numMegaPixel * numMegaPixel];
        for (int i = 0; i < numMegaPixel * numMegaPixel; i++)
        {
            brush[i] = Color.black;
        }

        WhiteboardDrawInput whiteboardDrawComponent = FindObjectOfType<WhiteboardDrawInput>();
        if (whiteboardDrawComponent == null)
        {
            Debug.LogError("could not find whiteboard draw component!");
        }
        else
        {
            whiteboards = whiteboardDrawComponent.whiteboards;
            if (_model != null)
            {
                lastTexture = (Texture2D)whiteboards[_model.whiteboardID].GetComponent<Renderer>().materials[0].mainTexture;
            }
        }

        try
        {
            _realtimeDrawerManager = realtime.GetComponent<RealtimeDrawerManager>();
            _realtimeDrawerManager.RegisterDrawer(realtimeView.ownerID, this);
        }
        catch (NullReferenceException)
        {
            Debug.LogError("RealtimeIKAvatar failed to register with RealtimeIKAvatarManager component. Was this avatar prefab instantiated by RealtimeIKAvatarManager?");
        }
    }

    void OnDestroy()
    {
        // Unregister with RealtimeIKAvatarManager
        if (_realtimeDrawerManager != null)
            _realtimeDrawerManager.UnregisterDrawer(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentFrame % drawUpdateFrames == 0 && pendingWrite)
        {
            lastTexture.Apply();
            pendingWrite = false;
        }
        currentFrame = (currentFrame + 1) % drawUpdateFrames;
    }

    private void DrawMegaPixel(float y, float z)
    {
        if (lastTexture == null)
        {
            Debug.LogError("last texture is null");
            return;
        }

        int width = lastTexture.width;
        int height = lastTexture.height;
        int actualY = (int)((y + 1) * (width / 2));
        int actualZ = (int)((z + 1) * (height / 2));

        if (lastY == -1)
        {
            lastY = actualY;
            lastZ = actualZ;
        }

        float distY = Mathf.Abs(actualY - lastY);
        float distZ = Mathf.Abs(actualZ - lastZ); 
        if (distY > distZ)
        {
            for (int i = 0; i <= distY; i++)
            {
                int blockY = (int)((i * lastY + (distY - i) * actualY) / distY);
                int blockZ = (int)((i * lastZ + (distY - i) * actualZ) / distY);
                int blockYStart = (int)Mathf.Clamp(blockY - 2, 0, width - numMegaPixel); 
                int blockZStart = (int)Mathf.Clamp(blockZ - 2, 0, height - numMegaPixel);
                lastTexture.SetPixels(blockYStart, blockZStart, 5, 5, brush);
            }
        }
        else
        {
            for (int i = 0; i <= distZ; i++)
            {
                int blockY = (int)((i * lastY + (distZ - i) * actualY) / distZ);
                int blockZ = (int)lastZ;
                if (distZ != 0)
                {
                    blockZ = (int)((i * lastZ + (distZ - i) * actualZ) / distZ);
                }
                int blockYStart = (int)Mathf.Clamp(blockY - 2, 0, width - numMegaPixel); 
                int blockZStart = (int)Mathf.Clamp(blockZ - 2, 0, height - numMegaPixel);
                lastTexture.SetPixels(blockYStart, blockZStart, 5, 5, brush);
            }
        }
        lastY = actualY;
        lastZ = actualZ;
        pendingWrite = true;                        
    }

    private void DrawPointsDidChange(WhiteboardDrawModel model, byte[] drawPoint)
    {
        float[] points = ByteArrayToFloat(drawPoint);
        if (points.Length == 2)
        {
            DrawMegaPixel(points[0], points[1]);
        }
    }
    
    private void WhiteboardIDDidChange(WhiteboardDrawModel model, int whiteboardID)
    {
        Debug.Log("whitebaord ID change to : " + whiteboardID);
        lastTexture = (Texture2D)whiteboards[whiteboardID].GetComponent<Renderer>().materials[0].mainTexture;
    }

    public void SetDrawPoints(float[] points)
    {
        _model.drawPoints = FloatArrayToByte(points);
    }

    public void SetWhiteboardID(int id)
    {
        Debug.Log("setting whiteboard id to: " + id);
        _model.whiteboardID = id;
    }

    public void ResetLastPoints()
    {
        lastY = -1;
        lastZ = -1;
    }

    public byte[] FloatArrayToByte(float[] input)
    {
        var byteArray = new byte[input.Length * sizeof(float)];
        Buffer.BlockCopy(input, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    public float[] ByteArrayToFloat(byte[] input)
    {
        var floatArray = new float[input.Length / sizeof(float)];
        Buffer.BlockCopy(input, 0, floatArray, 0, input.Length);
        return floatArray;
    }
}

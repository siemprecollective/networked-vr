using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;
using Normal.Realtime;
using RootMotion.FinalIK;
using ViveSR.anipal.Eye;
public class RealtimeIKAvatar : RealtimeComponent
{
    // Local Player
    [Serializable]
    public class LocalPlayer
    {
        public Transform root;
        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        public Transform centerEye;

        public GameObject leapHandModelManager;
        public OVRLipSyncContext lipSyncContext;
        public SkinnedMeshRenderer oculusBlendshapeMesh;
        public LipMotionInterface lipMotionInterface;
    }
    public LocalPlayer localPlayer { get { return _localPlayer; } set { SetLocalPlayer(value); } }
#pragma warning disable 0649 // Disable variable is never assigned to warning.
    private LocalPlayer _localPlayer;
#pragma warning restore 0649

    // Prefab
    public Transform head { get { return _head; } }
    public Transform leftHand { get { return _leftHand; } }
    public Transform rightHand { get { return _rightHand; } }
    public Transform leftHandTransform;
    public Transform rightHandTransform;

    public Transform leftEye;
    public Transform rightEye;
    public SkinnedMeshRenderer[] visemeTargets;
    public SkinnedMeshRenderer[] oculusBlendshapeTargets;
    public SkinnedMeshRenderer viveBlendshapeTarget;
    public Transform[] lipBones;
#pragma warning disable 0649 // Disable variable is never assigned to warning.
    [SerializeField] private Transform _head;
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;
#pragma warning restore 0649

    private RealtimeIKAvatarManager _realtimeAvatarManager;

    private Transform[] fingerTransforms;
    private LeapHandModel _model;

    private Transform _chairParentObject;
    public Transform chairParentObject { get { return _chairParentObject; } set { _chairParentObject = value; } }

    private Transform _chairHeadTarget;
    private Transform _chairPelvisTarget;
    private Transform _chairLeftLegTarget;
    private Transform _chairRightLegTarget;
    private Transform _chairLeftBendTarget;
    private Transform _chairRightBendTarget;
    private IKSolverVR solver;
    private float sitThreshold = 0.5f;
    private float pelvisWeight = 0.7f;
    private float footWeight = 1f;
    private float bendWeight = 0.368f;

    private Vector3[] lipBoneBasePositions = new Vector3[LipMotion.NUM_POINTS];
    bool printedDebug = false;

    private static readonly string[] OCULUS_BLENDSHAPE_NAMES = {
        "lookLeft_lEye",
        "lookRight_lEye",
        "lookLeft_rEye",
        "lookRight_rEye",
        "lookDown",
        "lookUp",
        "eyesClosed",
    };

    void Start()
    {
        if (lipBones != null && lipBones.Length == LipMotion.NUM_POINTS)
        {
            for (int i = 0; i < LipMotion.NUM_POINTS; ++i)
            {
                lipBoneBasePositions[i] = lipBones[i].localPosition;
            }
        }
        // Register with RealtimeAvatarManager
        try
        {
            _realtimeAvatarManager = realtime.GetComponent<RealtimeIKAvatarManager>();
            Debug.Log("realtimeAvatarManager: " + _realtimeAvatarManager.ToString());
            _realtimeAvatarManager._RegisterAvatar(realtimeView.ownerID, this);

            List<Transform> temp = GetAllChildTransforms(leftHandTransform, false);
            temp.AddRange(GetAllChildTransforms(rightHandTransform, false));
            fingerTransforms = temp.ToArray();

            if (_chairParentObject == null)
            {
                Debug.LogError("chair parent object is null!");
            }
            _chairHeadTarget = _chairParentObject.Find("Head_Target");
            _chairPelvisTarget = _chairParentObject.Find("Pelvis_Target");
            _chairLeftLegTarget = _chairParentObject.Find("Left_Foot_Target");
            _chairRightLegTarget = _chairParentObject.Find("Right_Foot_Target");
            _chairLeftBendTarget = _chairParentObject.Find("Left_Bend_Target");
            _chairRightBendTarget = _chairParentObject.Find("Right_Bend_Target");

            VRIK vrik = GetComponentInChildren<VRIK>();
            if (vrik != null)
            {
                solver = vrik.solver;
            }
        }
        catch (NullReferenceException)
        {
            Debug.LogError("RealtimeIKAvatar failed to register with RealtimeIKAvatarManager component. Was this avatar prefab instantiated by RealtimeIKAvatarManager?");
        }
    }

    void OnDestroy()
    {
        // Unregister with RealtimeIKAvatarManager
        if (_realtimeAvatarManager != null)
            _realtimeAvatarManager._UnregisterAvatar(this);

        // Unregister for events
        localPlayer = null;
    }

    void FixedUpdate()
    {
        UpdateAvatarTransformsForLocalPlayer();
        UpdateSeatedPosition();
    }

    void Update()
    {
        UpdateAvatarTransformsForLocalPlayer();
        UpdateSeatedPosition();
    }

    void LateUpdate()
    {
        UpdateAvatarTransformsForLocalPlayer();
        UpdateSeatedPosition();
    }

    void SetLocalPlayer(LocalPlayer localPlayer)
    {
        if (localPlayer == _localPlayer)
            return;

        _localPlayer = localPlayer;

        if (_localPlayer != null)
        {
            // TODO: Technically this shouldn't be needed. The RealtimeViewModel is created and locked to a user. The owner of that should progagate to children...
            RealtimeTransform rootRealtimeTransform = GetComponent<RealtimeTransform>();
            RealtimeTransform headRealtimeTransform = _head != null ? _head.GetComponent<RealtimeTransform>() : null;
            RealtimeTransform leftHandRealtimeTransform = _leftHand != null ? _leftHand.GetComponent<RealtimeTransform>() : null;
            RealtimeTransform rightHandRealtimeTransform = _rightHand != null ? _rightHand.GetComponent<RealtimeTransform>() : null;
            if (rootRealtimeTransform != null) rootRealtimeTransform.RequestOwnership();
            if (headRealtimeTransform != null) headRealtimeTransform.RequestOwnership();
            if (leftHandRealtimeTransform != null) leftHandRealtimeTransform.RequestOwnership();
            if (rightHandRealtimeTransform != null) rightHandRealtimeTransform.RequestOwnership();
        }
    }

    void UpdateAvatarTransformsForLocalPlayer()
    {
        // Make sure this avatar is a local player
        if (_localPlayer == null)
            return;

        // Root
        if (_localPlayer.root != null)
        {
            transform.position = _localPlayer.root.position;
            transform.rotation = _localPlayer.root.rotation;
            transform.localScale = _localPlayer.root.localScale;
        }

        // Head
        if (_localPlayer.head != null)
        {
            _head.position = _localPlayer.head.position;
            _head.rotation = _localPlayer.head.rotation;
        }

        // Left Hand
        if (_leftHand != null && _localPlayer.leftHand != null)
        {
            _leftHand.position = _localPlayer.leftHand.position;
            _leftHand.rotation = _localPlayer.leftHand.rotation;
        }

        // Right Hand
        if (_rightHand != null && _localPlayer.rightHand != null)
        {
            _rightHand.position = _localPlayer.rightHand.position;
            _rightHand.rotation = _localPlayer.rightHand.rotation;
        }
    }

    void UpdateSeatedPosition()
    {
        if (_chairHeadTarget == null || _chairLeftLegTarget == null || _chairRightLegTarget == null || _head == null)
        {
            if (printedDebug)
            {
                Debug.LogError("At least one of the chair targets is null!");
                printedDebug = true;
            }
            return;
        }
        //Debug.Log(string.Format("In updated seated position, distance {0}", Vector3.Distance(_chairHeadTarget.position, _head.position)));
        //Debug.Log(string.Format("{0}, {1}", _chairHeadTarget.position == null, _head.position == null));
        if (Vector3.Distance(_chairHeadTarget.position, _head.position) <= sitThreshold)
        {
            solver.spine.pelvisTarget = _chairPelvisTarget;
            solver.leftLeg.target = _chairLeftLegTarget;
            solver.leftLeg.bendGoal = _chairLeftBendTarget;
            solver.rightLeg.target = _chairRightLegTarget;
            solver.rightLeg.bendGoal = _chairRightBendTarget;

            solver.spine.pelvisPositionWeight = pelvisWeight;
            solver.leftLeg.positionWeight = footWeight;
            solver.leftLeg.bendGoalWeight = bendWeight;
            solver.rightLeg.positionWeight = footWeight;
            solver.rightLeg.bendGoalWeight = bendWeight;
        }
        else
        {
            solver.spine.pelvisTarget = null;
            solver.leftLeg.target = null;
            solver.leftLeg.bendGoal = null;
            solver.rightLeg.target = null;
            solver.rightLeg.bendGoal = null;

            solver.spine.pelvisPositionWeight = 0;
            solver.leftLeg.positionWeight = 0;
            solver.leftLeg.bendGoalWeight = 0;
            solver.rightLeg.positionWeight = 0;
            solver.rightLeg.bendGoalWeight = 0;
        }
    }

    private LeapHandModel model
    {
        set
        {
            if (_model != null)
            {
                _model.fingerPosesDidChange -= FingersDidChange;
                _model.headTargetDidChange -= HeadDidChange;
                _model.leftHandTargetDidChange -= LeftHandDidChange;
                _model.rightHeadTargetDidChange -= RightHandDidChange;
                _model.visemesDidChange -= VisemesDidChange;
                _model.oculusBlendshapesDidChange -= OculusBlendshapesDidChange;
                _model.eyeDataDidChange -= EyeDataDidChange;
                _model.lipDataDidChange -= LipDataDidChange;
            }

            _model = value;

            if (_model != null)
            {
                _model.fingerPosesDidChange += FingersDidChange;
                _model.headTargetDidChange += HeadDidChange;
                _model.leftHandTargetDidChange += LeftHandDidChange;
                _model.rightHeadTargetDidChange += RightHandDidChange;
                _model.visemesDidChange += VisemesDidChange;
                _model.oculusBlendshapesDidChange += OculusBlendshapesDidChange;
                _model.eyeDataDidChange += EyeDataDidChange;
                _model.lipDataDidChange += LipDataDidChange;
            }
        }
    }

    private void FingersDidChange(LeapHandModel model, byte[] fingerPoses)
    {
        if (realtime.clientID != realtimeView.ownerID && _model != null && _model.fingerPoses != null && fingerTransforms != null)
        {
            int arraySize = _model.fingerPoses.Length;
            if (arraySize / sizeof(float) != 4 * fingerTransforms.Length)
            {
                Debug.Log("Wrong size fingerPoses array!");
                return;
            }

            var x = ByteArrayToFloat(_model.fingerPoses);
            for (int i = 0; i < fingerTransforms.Length; i++)
            {
                fingerTransforms[i].rotation = new Quaternion(x[4 * i], x[4 * i + 1], x[4 * i + 2], x[4 * i + 3]);
            }
        }
    }

    private void HeadDidChange(LeapHandModel model, byte[] input)
    {
        if (realtime.clientID != realtimeView.ownerID && _model != null && _model.headTarget != null)
        {
            float[] poseInfo = ByteArrayToFloat(_model.headTarget);
            Vector3 newPos = new Vector3(poseInfo[0], poseInfo[1], poseInfo[2]);
            Quaternion newRot = new Quaternion(poseInfo[3], poseInfo[4], poseInfo[5], poseInfo[6]);
            _head.SetPositionAndRotation(newPos, newRot);
        }
    }

    private void LeftHandDidChange(LeapHandModel model, byte[] input)
    {
        if (realtime.clientID != realtimeView.ownerID && _model != null && _model.leftHandTarget != null)
        {
            float[] poseInfo = ByteArrayToFloat(_model.leftHandTarget);
            Vector3 newPos = new Vector3(poseInfo[0], poseInfo[1], poseInfo[2]);
            Quaternion newRot = new Quaternion(poseInfo[3], poseInfo[4], poseInfo[5], poseInfo[6]);
            _leftHand.SetPositionAndRotation(newPos, newRot);
        }
    }

    private void RightHandDidChange(LeapHandModel model, byte[] input)
    {
        if (realtime.clientID != realtimeView.ownerID && _model != null && _model.rightHeadTarget != null)
        {
            float[] poseInfo = ByteArrayToFloat(_model.rightHeadTarget);
            Vector3 newPos = new Vector3(poseInfo[0], poseInfo[1], poseInfo[2]);
            Quaternion newRot = new Quaternion(poseInfo[3], poseInfo[4], poseInfo[5], poseInfo[6]);
            _rightHand.SetPositionAndRotation(newPos, newRot);
        }
    }

    private void VisemesDidChange(LeapHandModel model, byte[] visemeBytes)
    {
        if (_model == null) return;
        if (_model.visemes == null) return;

        float[] visemes = ByteArrayToFloat(_model.visemes);
        foreach (SkinnedMeshRenderer mesh in visemeTargets)
        {
            for (int i = 0; i < visemes.Length; ++i)
            {
                // TODO this assumes that the visemes are in the same order in both the mesh
                // and the array from OVRLipSyncContext
                mesh.SetBlendShapeWeight(i, visemes[i] * 100.0f);
            }
        }
    }

    private void OculusBlendshapesDidChange(LeapHandModel model, byte[] blendshapeBytes)
    {
        // names in the target mesh must match names in the oculus mesh
        if (_model == null) return;
        if (_model.visemes == null) return;

        float[] blendshapes = ByteArrayToFloat(_model.oculusBlendshapes);
        foreach (SkinnedMeshRenderer target in oculusBlendshapeTargets)
        {
            for (int i = 0; i < blendshapes.Length; ++i)
            {
                int idx = target.sharedMesh.GetBlendShapeIndex(OCULUS_BLENDSHAPE_NAMES[i]);
                if (idx != -1)
                {
                    target.SetBlendShapeWeight(idx, blendshapes[i]);
                }
            }
        }
    }

    private void EyeDataDidChange(LeapHandModel model, byte[] eyeData)
    {
        if (_model == null) return;
        if (_model.eyeData == null) return;
        if (_model.eyeData.Length < 5) return;

        float[] vals = ByteArrayToFloat(_model.eyeData);
        Vector3 target = new Vector3(vals[0], vals[1], vals[2]);
        leftEye.LookAt(target);
        leftEye.Rotate(90, 0, 0);
        rightEye.LookAt(target);
        rightEye.Rotate(90, 0, 0);

        float leftEyeBlink = vals[3] * 100f;
        float rightEyeBlink = vals[4] * 100f;

        int lIdx = viveBlendshapeTarget.sharedMesh.GetBlendShapeIndex("lidClosed_lEye");
        int rIdx = viveBlendshapeTarget.sharedMesh.GetBlendShapeIndex("lidClosed_rEye");
        if (lIdx != -1 && rIdx != -1)
        {
            viveBlendshapeTarget.SetBlendShapeWeight(lIdx, leftEyeBlink);
            viveBlendshapeTarget.SetBlendShapeWeight(rIdx, rightEyeBlink);
        }
    }

    private void LipDataDidChange(LeapHandModel model, byte[] lipData)
    {
        if (_model == null) return;
        if (_model.lipData == null) return;
        if (_model.lipData.Length == 0) return;
        if (lipBones.Length != LipMotion.NUM_POINTS) return;
        var f = new BinaryFormatter();
        var points = (LipMotion.Point[])f.Deserialize(new MemoryStream(_model.lipData));
        if (points.Length != LipMotion.NUM_POINTS) return;
        if (points[0].x == 0 && points[0].y == 0) return;

        var upperLipLeft = points[0];
        var upperLipRight = points[4];
        var upperLipX = upperLipRight.x - upperLipLeft.x;
        var upperLipXWorld = Math.Abs(lipBoneBasePositions[4].x - lipBoneBasePositions[0].x);
        var renorm = upperLipXWorld / upperLipX;
        Debug.Log($"{upperLipXWorld}, {renorm}");

        for (int i = 0; i < LipMotion.NUM_POINTS; ++i)
        {
            var xOffset = ((float)(points[i].x - upperLipLeft.x)) * renorm;
            var yOffset = ((float)(points[i].y - upperLipLeft.y)) * renorm;
            lipBones[i].localPosition = lipBoneBasePositions[0] + (new Vector3(-xOffset, -yOffset, 0));
        }

        //Debug.Log(String.Join(" ", points.Select(p => $"({p.x}, {p.y})")));
    }

    public void SetFingerPoses()
    {
        List<float> rotationValues = new List<float>();
        foreach (Transform temp in fingerTransforms)
        {
            Quaternion tempRotation = temp.rotation;
            float[] tempArray = { tempRotation.x, tempRotation.y, tempRotation.z, tempRotation.w };
            rotationValues.AddRange(tempArray);
        }
        float[] floatArray = rotationValues.ToArray();

        _model.fingerPoses = FloatArrayToByte(floatArray);
    }

    public void SetIKTargets()
    {
        _model.headTarget = ConvertTransform(_head);
        _model.leftHandTarget = ConvertTransform(_leftHand);
        _model.rightHeadTarget = ConvertTransform(_rightHand);
    }

    public void SetVisemes()
    {
        if (_localPlayer == null) return;
        if (_localPlayer.lipSyncContext == null) return;

        OVRLipSync.Frame frame = _localPlayer.lipSyncContext.GetCurrentPhonemeFrame();
        _model.visemes = FloatArrayToByte(frame.Visemes);
    }

    public void SetOculusBlendshapes()
    {
        // don't worry about blendshapes if we have eye tracking
        if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING) return;
        if (_localPlayer == null) return;
        if (_localPlayer.oculusBlendshapeMesh == null) return;

        var mesh = _localPlayer.oculusBlendshapeMesh;
        float[] blendshapes = new float[OCULUS_BLENDSHAPE_NAMES.Length];
        for (int i = 0; i < OCULUS_BLENDSHAPE_NAMES.Length; ++i)
        {
            int idx = mesh.sharedMesh.GetBlendShapeIndex(OCULUS_BLENDSHAPE_NAMES[i]);
            blendshapes[i] = mesh.GetBlendShapeWeight(idx);
        }
        _model.oculusBlendshapes = FloatArrayToByte(blendshapes);
    }

    public void SetEyeData()
    {
        if (_localPlayer == null) return;
        if (_localPlayer.centerEye == null) return;
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING) return;

        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal = Vector3.zero;
        if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
        else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
        else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal)) { }
        Vector3 gazeTarget = _localPlayer.centerEye.TransformPoint(GazeDirectionCombinedLocal);

        Dictionary<EyeShape, float> eyeWeightings = new Dictionary<EyeShape, float>();
        SRanipal_Eye.GetEyeWeightings(out eyeWeightings);
        float leftEyeBlink = eyeWeightings[EyeShape.Eye_Left_Blink];
        float rightEyeBlink = eyeWeightings[EyeShape.Eye_Right_Blink];

        float[] gazeInfoArr = { gazeTarget.x, gazeTarget.y, gazeTarget.z, leftEyeBlink, rightEyeBlink };
        _model.eyeData = FloatArrayToByte(gazeInfoArr);
    }

    public void SetLipData()
    {
        if (_localPlayer == null) return;
        if (_localPlayer.lipMotionInterface == null) return;
        if (!_localPlayer.lipMotionInterface.enabled) return;

        var frame = _localPlayer.lipMotionInterface.processFrame();
        using (var stream = new MemoryStream())
        {
            var f = new BinaryFormatter();
            f.Serialize(stream, frame);
            _model.lipData = stream.ToArray();
        }
    }

    private byte[] ConvertTransform(Transform transform)
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        float[] values = { pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w };
        return FloatArrayToByte(values);
    }

    private byte[] FloatArrayToByte(float[] input)
    {
        var byteArray = new byte[input.Length * sizeof(float)];
        Buffer.BlockCopy(input, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    private float[] ByteArrayToFloat(byte[] input)
    {
        var floatArray = new float[input.Length / sizeof(float)];
        Buffer.BlockCopy(input, 0, floatArray, 0, input.Length);
        return floatArray;
    }

    private List<Transform> GetAllChildTransforms(Transform root, bool includeParent)
    {
        List<Transform> transformList = new List<Transform>();
        if (includeParent) transformList.Add(root);
        foreach (Transform child in root)
        {
            transformList.AddRange(GetAllChildTransforms(child, true));
        }
        return transformList;
    }
}

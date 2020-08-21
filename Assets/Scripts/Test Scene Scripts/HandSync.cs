using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;

public class HandSync : RealtimeComponent
{
    private const string ANIM_LAYER_NAME_POINT = "Point Layer";
    private const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
    private const string ANIM_PARAM_NAME_FLEX = "Flex";
    private const string ANIM_PARAM_NAME_POSE = "Pose";

    private int m_animLayerIndexThumb = -1;
    private int m_animLayerIndexPoint = -1;
    private int m_animParamIndexFlex = -1;
    private int m_animParamIndexPose = -1;

    private Animator m_animator = null;
    private HandSyncModel _model;

    private void Start()
    {
        m_animator = gameObject.GetComponentInChildren<Animator>();
        if (m_animator == null)
        {
            Debug.LogError("No animator found for hand!");
        }
        else
        {
            m_animLayerIndexPoint = m_animator.GetLayerIndex(ANIM_LAYER_NAME_POINT);
            m_animLayerIndexThumb = m_animator.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
            m_animParamIndexFlex = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);
            m_animParamIndexPose = Animator.StringToHash(ANIM_PARAM_NAME_POSE);
        }
    }

    private HandSyncModel model
    {
        set
        {
            if (_model != null)
            {
                _model.handPoseIdDidChange -= HandPoseDidChange;
                _model.flexDidChange -= FlexDidChange;
                _model.pointDidChange -= PointDidChange;
                _model.thumbsUpDidChange -= ThumbsUpDidChange;
                _model.pinchDidChange -= PinchDidChange;
            }

            _model = value;

            if (_model != null)
            {
                UpdateHandAnimator();
                _model.handPoseIdDidChange += HandPoseDidChange;
                _model.flexDidChange += FlexDidChange;
                _model.pointDidChange += PointDidChange;
                _model.thumbsUpDidChange += ThumbsUpDidChange;
                _model.pinchDidChange += PinchDidChange;
            }
        }
    }

    private void HandPoseDidChange(HandSyncModel model, int handPoseId)
    {
        if (m_animator != null)
        {
            m_animator.SetInteger(m_animParamIndexPose, handPoseId);
        }
    }

    private void FlexDidChange(HandSyncModel model, float flex)
    {
        if (m_animator != null)
        {
            m_animator.SetFloat(m_animParamIndexFlex, flex);
        }
    }

    private void PointDidChange(HandSyncModel model, float point)
    {
        if (m_animator != null)
        {
            m_animator.SetLayerWeight(m_animLayerIndexPoint, point);
        }
    }
    private void ThumbsUpDidChange(HandSyncModel model, float thumbsUp)
    {
        if (m_animator != null)
        {
            m_animator.SetLayerWeight(m_animLayerIndexThumb, thumbsUp);
        }
    }
    private void PinchDidChange(HandSyncModel model, float pinch)
    {
        if (m_animator != null)
        {
            m_animator.SetFloat("Pinch", pinch);
        }
    }

    private void UpdateHandAnimator()
    {
        if (m_animator != null)
        {
            m_animator.SetInteger(m_animParamIndexPose, _model.handPoseId);
            m_animator.SetFloat(m_animParamIndexFlex, _model.flex);
            m_animator.SetLayerWeight(m_animLayerIndexPoint, _model.point);
            m_animator.SetLayerWeight(m_animLayerIndexThumb, _model.thumbsUp);
            m_animator.SetFloat("Pinch", _model.pinch);
        }
    }

    public void setHandAnimator(int handPoseId, float flex, float point, float thumbsUp, float pinch)
    {
        _model.handPoseId = handPoseId;
        _model.flex = flex;
        _model.point = point;
        _model.thumbsUp = thumbsUp;
        _model.pinch = pinch;
    }
}

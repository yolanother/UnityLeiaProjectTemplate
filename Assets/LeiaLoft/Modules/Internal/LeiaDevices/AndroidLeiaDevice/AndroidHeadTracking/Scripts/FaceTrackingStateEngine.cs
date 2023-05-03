using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class handles transitions between eye tracking states and accompanying baseline and camera shift animations

//When a face is found:
//1. Shift the cameras towards the face
//2. Once camera shift is close enough to correct position, increase the baseline scaling (while continuing camera shift towards correct position)

//When a face is lost:
//1. Reduce baseline scaling

//When the primary face being tracked changes: (for example, one viewer moves away from the display and another walks closer to it and becomes the new primary viewer)
//1. Reduce baseline scaling
//2. Shift the cameras towards the new tracked face
//3. Once camera shift is close enough to correct position, increase the baseline scaling (while continuing camera shift towards correct position)


public class FaceTrackingStateEngine : Singleton<FaceTrackingStateEngine>
{
    public enum FaceTransitionState { NoFace, FaceLocked, ReducingBaseline, SlidingCameras, IncreasingBaseline };

    FaceTransitionState _faceTransitionState = FaceTransitionState.NoFace;

    public FaceTransitionState faceTransitionState
    {
        get
        {
            return _faceTransitionState;
        }
        set
        {
            _faceTransitionState = value;
        }
    }

    float _eyeTrackingAnimatedBaselineScalar; //smoothly animates to 0 when no faces detected, and to 1 when faces detected, used to scale baseline
    public float eyeTrackingAnimatedBaselineScalar //smoothly animates to 0 when no faces detected, and to 1 when faces detected, used to scale baseline
    {
        get
        {
            return _eyeTrackingAnimatedBaselineScalar;
        }
        set
        {
            _eyeTrackingAnimatedBaselineScalar = value;
        }
    }

    EyeTracking eyeTracking
    {
        get
        {
            return LeiaDisplay.Instance.tracker;
        }
    }

    void Update()
    {
        switch (faceTransitionState)
        {
            case FaceTransitionState.FaceLocked:
                HandleFaceLockedState();
                break;
            case FaceTransitionState.NoFace:
                NoFaceState();
                break;
            case FaceTransitionState.ReducingBaseline:
                HandleReducingBaselineState();
                break;
            case FaceTransitionState.SlidingCameras:
                HandleSlidingCamerasState();
                break;
            case FaceTransitionState.IncreasingBaseline:
                HandleIncreasingBaselineState();
                break;
            default:
                Debug.LogError("Unhandled FaceTransitionState: "+faceTransitionState);
                break;
        }
    }

    void HandleFaceLockedState()
    {
        //Face locked, shift cameras towards the viewer
        eyeTrackingAnimatedBaselineScalar = 1;
    }
    void NoFaceState()
    {
        //No face detected, do nothing
        if (eyeTracking.NumFaces > 0)
        {
            faceTransitionState = FaceTransitionState.SlidingCameras;
        }
    }
    void HandleReducingBaselineState()
    {
        //No face detected or primary face has changed, reduce baseline to zero
        eyeTrackingAnimatedBaselineScalar += (0 - eyeTrackingAnimatedBaselineScalar) * Mathf.Min((Time.deltaTime * 5f), 1f);
        if (eyeTrackingAnimatedBaselineScalar < .1f)
        {
            faceTransitionState = FaceTransitionState.SlidingCameras;
        }
    }
    void HandleSlidingCamerasState()
    {
        //Baseline is 0, slide cameras 
        eyeTrackingAnimatedBaselineScalar = 0;
    }
    void HandleIncreasingBaselineState()
    {
        //Increase baseline to 1
        eyeTrackingAnimatedBaselineScalar += (1 - eyeTrackingAnimatedBaselineScalar) * Mathf.Min((Time.deltaTime * 5f), 1f);

        if (Mathf.Abs(eyeTrackingAnimatedBaselineScalar - 1) < .1f)
        {
            faceTransitionState = FaceTransitionState.FaceLocked;
        }
    }
}

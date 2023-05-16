using Leia;
using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTrackingAndroid : EyeTracking
{
    private Leia.FaceDetectorBackend faceTrackingBackend;

    public FaceDetectorBackend FaceTrackingBackend { get => faceTrackingBackend; set => faceTrackingBackend = value; }

    protected override void UpdateCameraConnectedStatus()
    {
        _cameraConnectedPrev = _cameraConnected;
        _cameraConnected = false;
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
            // TODO: do we need to support realsense on Android?
            if (devices[i].name.Contains("Camera 1"))
            {
                _cameraConnected = true;
                break;
            }
        }

        base.UpdateCameraConnectedStatus();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    public override void InitHeadTracking()
    {
        Leia.FaceDetectorConfig config = new Leia.FaceDetectorConfig();
        config.backend = faceTrackingBackend;
        config.inputType = Leia.FaceDetectorInputType.Unknown;
        LeiaDisplay.Instance.CNSDK.SetFaceTrackingConfig(config);
        IsTracking = LeiaDisplay.Instance.CNSDK.EnableFacetracking(true) == Leia.SDK.Status.Success;
    }
#endif

    public override void StartTracking()
    {
        base.StartTracking();
#if UNITY_ANDROID && !UNITY_EDITOR

        if (LeiaDisplay.Instance.CNSDK == null)
        {
            this.InitHeadTracking();
        }

        if (LeiaDisplay.Instance.CNSDK != null)
        {
            if(!IsTracking)
            {
                LeiaDisplay.Instance.CNSDK.Resume();
                IsTracking = true;
            }
        }
        else
        {
            IsTracking = false;
#if !UNITY_EDITOR
            Debug.LogError("LeiaDisplay.Instance.CNSDK is null!");
#endif
        }
#endif
    }
    
    public override void StopTracking()
    {
        IsTracking = false;
#if UNITY_ANDROID && !UNITY_EDITOR
        if (LeiaDisplay.Instance.CNSDK != null)
        {
            LeiaDisplay.Instance.CNSDK.Pause();
        }
#endif
    }

    protected override void TerminateHeadTracking()
    {
        base.TerminateHeadTracking();
#if UNITY_ANDROID && !UNITY_EDITOR
        if (LeiaDisplay.Instance.CNSDK != null)
        {
            LeiaDisplay.Instance.CNSDK.EnableFacetracking(false);
            IsTracking = false;
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    public override void SetProfilingEnabled(bool enabed)
    {
        LeiaDisplay.Instance.CNSDK.SetProfiling(enabed);
        _isProfilingEnabled = enabed;
    }

    AndroidJavaClass javaClockClass = null;

    double GetSystemTimeMs()
    {
        if (javaClockClass == null)
        {
            javaClockClass = new AndroidJavaClass("android.os.SystemClock");
        }
        long timeNs = javaClockClass.CallStatic<long>("elapsedRealtimeNanos");
        return (double)(timeNs) * 1e-6;
    }

    long GetSystemTimeNs()
    {
        if (javaClockClass == null)
        {
            javaClockClass = new AndroidJavaClass("android.os.SystemClock");
        }
        long timeNs = javaClockClass.CallStatic<long>("elapsedRealtimeNanos");
        return timeNs;
    }
    private bool _isProfilingEnabled = true;
#endif

    public override AbstractTrackingResult GetDeviceTrackingResult()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Leia.Vector3 predictedFacePosition;
        Leia.Vector3 nonPredictedFacePosition;

        if (LeiaDisplay.Instance.CNSDK != null)
        {
            LeiaDisplay.Instance.CNSDK.GetPrimaryFace(out predictedFacePosition);
            bool nonPredFaceFound = LeiaDisplay.Instance.CNSDK.GetNonPredictedPrimaryFace(out nonPredictedFacePosition) == Leia.SDK.Status.Success;

            if (IsProfilingEnabled)
            {
                LeiaHeadTracking.FrameProfiling frameProfiling;

                LeiaDisplay.Instance.CNSDK.GetFaceTrackingProfiling(out frameProfiling);
                EyeTrackingProcessingTime = (float)(frameProfiling.faceDetectorEndTime - frameProfiling.faceDetectorStartTime) * 1e-6f;
            }

            AbstractTrackingResult abstractTrackingResult = new AbstractTrackingResult();

            int FaceIndex = 0;
            
            UnityEngine.Vector3 PredictedFacePosition = new UnityEngine.Vector3(
                predictedFacePosition.x,
                predictedFacePosition.y,
                predictedFacePosition.z
                );
    
            UnityEngine.Vector3 NonPredictedFacePosition = UnityEngine.Vector3.zero;
            
            if (nonPredFaceFound)
            {
            NonPredictedFacePosition = new UnityEngine.Vector3(
                nonPredictedFacePosition.x,
                nonPredictedFacePosition.y,
                nonPredictedFacePosition.z
                );
            }

            UnityEngine.Vector3 Velocity = new UnityEngine.Vector3(
                0,
                0,
                0
            );
            UnityEngine.Vector3 Angle = new UnityEngine.Vector3(
                0,
                0,
                0
            );
            Face face = new Face(FaceIndex, NonPredictedFacePosition, PredictedFacePosition, Velocity, Angle);
            abstractTrackingResult.AddFace(face);

            return abstractTrackingResult;
        }
        else
        {
            Debug.LogError("CNSDK not found!");
        }
#endif
        return null;
    }
    
    protected override void OnDisable()
    {
        if (!ApplicationQuitting)
        {
#if UNITY_ANDROID
            if (Instance == this)
            {
                StopTracking();
            }
#endif

            base.OnDisable();
        }
    }
}

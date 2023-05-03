using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTrackingWindows : EyeTracking
{
    
    const int maxAvgFaces = 7;
    Leia.Vector3[] lastAvgFaces;
    private bool isPrimaryFaceSet = false;
    private Leia.Vector3 primaryFacePosition;
    private bool nonPredFaceFound = false;
    private Leia.Vector3 nonPredictedFace;

    public bool IsPrimaryFaceSet { get => isPrimaryFaceSet; set => isPrimaryFaceSet = value; }
    public Leia.Vector3 PrimaryFacePosition { get => primaryFacePosition; set => primaryFacePosition = value; }
    public bool NonPredFaceFound { get => nonPredFaceFound; set => nonPredFaceFound = value; }
    public Leia.Vector3 NonPredictedFace { get => nonPredictedFace; set => nonPredictedFace = value; }

    protected override void UpdateCameraConnectedStatus()
    {
        Debug.Log("UpdateCameraConnectedStatus Windows");
        _cameraConnectedPrev = _cameraConnected;
        _cameraConnected = false;
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].name.Contains("Intel(R) RealSense(TM)"))
            {
                _cameraConnected = true;
                break;
            }
        }

        base.UpdateCameraConnectedStatus();
    }

    protected override void Start()
    {
        base.Start();
        lastAvgFaces = new Leia.Vector3[maxAvgFaces];
    }

    public override void InitHeadTracking()
    {
        Debug.Log("Calling InitHeadTracking Windows");
        
    }

    public override void StartTracking()
    {
        Debug.Log("Called StartTracking Windows");

        base.StartTracking();
        
    }

    public override void StopTracking()
    {
        //Implement for windows
        IsTracking = false;
    }
    
    protected override void TerminateHeadTracking()
    {
        base.TerminateHeadTracking();
        
    }
    
#if UNITY_STANDALONE_WIN
    public override void AddTestFace(float x = 0, float y = 0, float z = 800) //A useful method for adding a virtual test face, which can be used for multi-face testing when you don't have other people available to help you test
    {
        //Currently only supported on Windows
        //TODO: Implement for Android in the EyeTrackingAndoid.cs class
        
    }
#endif

#if UNITY_STANDALONE_WIN
    public override AbstractTrackingResult GetDeviceTrackingResult()
    {
        if (windowsTrackingResult.faces == null)
        {
            Debug.Log("windowsTrackingResult.faces is null!");
            return null;  //tracking result not fetched yet
        }

        Debug.Log("Here in Windows GetDeviceTrackingResult");
        if (windowsTrackingResult.faces.Length == 0)
        {
            Debug.Log("windowsTrackingResult.faces.Length == 0!");
            return null; //No faces detected
        }

        chosenFaceIndex = 0;
        LeiaHeadTracking.Face chosenFace = windowsTrackingResult.faces[chosenFaceIndex];

        AbstractTrackingResult abstractTrackingResult = new AbstractTrackingResult();

        int FaceIndex = 0;
        
        Vector3 PredictedFacePosition = new Vector3(
            chosenFace.point.pos.x,
            chosenFace.point.pos.y,
            chosenFace.point.pos.z
            );
        Vector3 NonPredictedFacePosition = new Vector3(
            chosenFace.point.pos.x,
            chosenFace.point.pos.y,
            chosenFace.point.pos.z
            );

        Vector3 Velocity = new Vector3(
            chosenFace.point.vel.x,
            chosenFace.point.vel.y,
            chosenFace.point.vel.z
            );
        Vector3 Angle = new Vector3(
            chosenFace.angle.x,
            chosenFace.angle.y,
            chosenFace.angle.z
            );
        Face face = new Face(FaceIndex, NonPredictedFacePosition, PredictedFacePosition, Velocity, Angle);
        abstractTrackingResult.AddFace(face);

        return abstractTrackingResult;
    }
#endif

#if UNITY_STANDALONE_WIN
    public override void UpdateFacePosition()
    {
        if (headTrackingEngine != null)
        {
            LeiaHeadTracking.Frame frame = headTrackingEngine.GetLatestFrame();
            frame.GetTrackingResult(out windowsTrackingResult);
        }
        base.UpdateFacePosition();
    }
#endif
}

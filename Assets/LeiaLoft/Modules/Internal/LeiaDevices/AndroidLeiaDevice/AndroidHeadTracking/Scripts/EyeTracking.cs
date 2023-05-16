using LeiaLoft;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EyeTracking : Singleton<EyeTracking>
{
    int numFaces;
    public int NumFaces
    {
        get
        {
            return numFaces;
        }

        private set
        {
            numFaces = value;
        }
    }

    protected int chosenFaceIndex;
    protected int chosenFaceIndexPrev;

    [SerializeField] private Text debugLabel;

    private float faceX = 0, faceY = 0;
    private float _faceZ = 600;
    public float faceZ
    {
        get
        {
            return _faceZ;
        }
        set
        {
            _faceZ = value;
        }
    }

    private float nonPredictedFaceX = 0, nonPredictedFaceY = 0, nonPredictedFaceZ = 600;
    private float predictedFaceX = 0, predictedFaceY = 0, predictedFaceZ = 600;

    RunningFloatAverage runningAverageFaceX;
    RunningFloatAverage runningAverageFaceY;
    RunningFloatAverage runningAverageFaceZ;

    protected enum TrackingState { FaceTracking, NotFaceTracking };
    protected TrackingState priorRequestedState = TrackingState.NotFaceTracking;
    protected TrackingState currentState = TrackingState.NotFaceTracking;
    protected TrackingState requestedState = TrackingState.NotFaceTracking;

    public FaceTrackingStateEngine.FaceTransitionState faceTransitionState
    {
        get
        {
            return faceTrackingStateEngine.faceTransitionState;
        }

        private set
        {
            faceTrackingStateEngine.faceTransitionState = value;
        }
    }

    private float eyeTrackingProcessingTime = 0.0f;
    double t;
    private bool isProfilingEnabled = false;
    protected float delay;
    protected bool IsTracking = false;

    LeiaVirtualDisplay _leiaVirtualDisplay;
    LeiaVirtualDisplay leiaVirtualDisplay
    {
        get
        {
            if (_leiaVirtualDisplay == null)
            {
                _leiaVirtualDisplay = FindObjectOfType<LeiaVirtualDisplay>();
            }
            return _leiaVirtualDisplay;
        }
    }

    protected bool _cameraConnectedPrev;
    protected bool _cameraConnected;
    public bool CameraConnected
    {
        get
        {
            return _cameraConnected;
        }
    }

    protected bool ApplicationQuitting;

    protected virtual void UpdateCameraConnectedStatus()
    {
        if (!_cameraConnected && _cameraConnectedPrev)
        {
            Debug.Log("Camera not connected! Terminating head tracking!");
            TerminateHeadTracking();
        }
        else
        if (_cameraConnected && !_cameraConnectedPrev)
        {
            InitHeadTracking();
        }

        Invoke("UpdateCameraConnectedStatus", 1f);
    }

    protected virtual void OnEnable()
    {
        StartTracking();
    }

    protected virtual void OnDisable()
    {
        if (!ApplicationQuitting)
        {
            faceX = 0;
            faceY = 0;
            faceZ = LeiaDisplay.Instance.GetDisplayConfig().ConvergenceDistance;
        }
    }

    protected virtual void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            StopTracking();
        }
        else
        {
            if (LeiaDisplay.Instance.DesiredLightfieldMode == LeiaDisplay.LightfieldMode.On)
            {
                StartTracking();
            }
        }
    }

    public virtual void StartTracking()
    {
        if (LeiaDisplay.Instance == null)
        {
            Debug.LogError("ERROR: LeiaDisplay.Instance == null");
            return;
        }
        if (LeiaDisplay.Instance.displayConfig == null)
        {
            Debug.LogError("ERROR: LeiaDisplay.Instance.displayConfig == null");
            return;
        }
        faceZ = LeiaDisplay.Instance.displayConfig.ConvergenceDistance;
    }

    public virtual void InitHeadTracking()
    {
        //Implement in child classes
    }

    public virtual void StopTracking()
    {
        //implement in child classes
    }

    public Vector3 GetPredictedFacePosition()
    {
        return new Vector3(predictedFaceX, predictedFaceY, predictedFaceZ);
    }

    public Vector3 GetNonPredictedFacePosition()
    {
        return new Vector3(nonPredictedFaceX, nonPredictedFaceY, nonPredictedFaceZ);
    }

    LeiaDisplay _leiaDisplay;
    LeiaDisplay leiaDisplay
    {
        get
        {
            if (_leiaDisplay == null)
            {
                _leiaDisplay = FindObjectOfType<LeiaDisplay>();
            }

            return _leiaDisplay;
        }
    }

    public float GetEyeTrackingProcessingTime()
    {
        return EyeTrackingProcessingTime;
    }

    FaceTrackingStateEngine faceTrackingStateEngine;
    public float AnimatedBaseline
    {
        get
        {
            return faceTrackingStateEngine.eyeTrackingAnimatedBaselineScalar;
        }
    }

    public float EyeTrackingProcessingTime { get => eyeTrackingProcessingTime; protected set => eyeTrackingProcessingTime = value; }
    public bool IsProfilingEnabled { get => isProfilingEnabled; protected set => isProfilingEnabled = value; }

    private void Awake()
    {
        faceTrackingStateEngine = gameObject.AddComponent<FaceTrackingStateEngine>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        runningAverageFaceX = new RunningFloatAverage(60);
        runningAverageFaceY = new RunningFloatAverage(60);
        runningAverageFaceZ = new RunningFloatAverage(60);

        AssignDebugLabel();

        UpdateCameraConnectedStatus();

        if (Instance != null && Instance == this)
        {
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
            LeiaDisplay.Instance.tracker = this;
        }
    }

    void AssignDebugLabel()
    {
        GameObject debugLabelGameObject = GameObject.Find("BlinkDebugText");
        if (debugLabelGameObject != null)
        {
            debugLabel = debugLabelGameObject.GetComponent<Text>();
        }
    }

    public virtual void SetProfilingEnabled(bool enabed)
    {
        //Implement in child classes
    }

    public virtual void AddTestFace(float x, float y, float z) //A useful method for adding a virtual test face, which can be used for multi-face testing when you don't have other people available to help you test
    {
        //Implement in child classes
    }

    public virtual void UpdateFacePosition()
    {
        //The UpdateFacePosition calls each devices GetDeviceTrackingResult method to get the list of faces

        if (LeiaDisplay.Instance.UsingSimulatedFacePosition)
        {
            faceX = LeiaDisplay.Instance.SimulatedFaceX;
            faceY = LeiaDisplay.Instance.SimulatedFaceY;
            faceZ = LeiaDisplay.Instance.SimulatedFaceZ;
            return;
        }

        if (CameraConnected && IsTracking)
        {
            double old_time_stamp = t;
            DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
            delay = (float)(t - old_time_stamp + config.timeDelay);
            t = Time.time * 1000;

            AbstractTrackingResult trackingResult = GetDeviceTrackingResult();

            if (trackingResult == null)
            {
                //No faces detected
                NumFaces = 0;
                return;
            }

            NumFaces = trackingResult.NumFaces;

            if (NumFaces > 0)
            {
                requestedState = TrackingState.FaceTracking;
                chosenFaceIndexPrev = chosenFaceIndex;
                chosenFaceIndex = -1;

                Face ActiveFace = trackingResult.Faces[0];

                Vector3 currentPos = new Vector3(
                        faceX,
                        faceY,
                        faceZ
                    );

                Vector3 targetPos = new Vector3(
                    ActiveFace.PredictedPosition.x,
                    ActiveFace.PredictedPosition.y,
                    ActiveFace.PredictedPosition.z
                );

                if (targetPos.z > 0)
                {
                    faceX += (targetPos.x - currentPos.x) * Mathf.Min((Time.deltaTime * 5f), 1f);
                    faceY += (targetPos.y - currentPos.y) * Mathf.Min((Time.deltaTime * 5f), 1f);
                    faceZ += (targetPos.z - currentPos.z) * Mathf.Min((Time.deltaTime * 5f), 1f);
                    predictedFaceX = faceX;
                    predictedFaceY = faceY;
                    predictedFaceZ = faceZ;
                    Debug.Log("Gets Here EyeTracking setting predictedFacePosition to " + this.GetPredictedFacePosition());
                }

                if (faceTransitionState == FaceTrackingStateEngine.FaceTransitionState.SlidingCameras
                    && Vector3.Distance(currentPos, targetPos) < 10f)
                {
                    faceTransitionState = FaceTrackingStateEngine.FaceTransitionState.IncreasingBaseline;
                }

                if (faceTransitionState == FaceTrackingStateEngine.FaceTransitionState.FaceLocked)
                {
                    Vector3 facePosPrev = new Vector3(
                        faceX,
                        faceY,
                        faceZ
                    );
                    Vector3 facePosNext = new Vector3(
                        ActiveFace.PredictedPosition.x,
                        ActiveFace.PredictedPosition.y,
                        ActiveFace.PredictedPosition.z
                    );

                    float distance = Mathf.Abs(facePosPrev.z - facePosNext.z);

                    if ((distance < 50 || facePosPrev == Vector3.zero) && ActiveFace.PredictedPosition.z > 0)
                    {
                        faceX = ActiveFace.PredictedPosition.x;
                        faceY = ActiveFace.PredictedPosition.y;
                        faceZ = ActiveFace.PredictedPosition.z;
                        predictedFaceX = ActiveFace.PredictedPosition.x + ActiveFace.Velocity.x * delay;
                        predictedFaceY = ActiveFace.PredictedPosition.y + ActiveFace.Velocity.y * delay;
                        predictedFaceZ = ActiveFace.PredictedPosition.z + ActiveFace.Velocity.z * delay;
                        nonPredictedFaceX = ActiveFace.NonPredictedPosition.x;
                        nonPredictedFaceY = ActiveFace.NonPredictedPosition.y;
                        nonPredictedFaceZ = ActiveFace.NonPredictedPosition.z;
                        runningAverageFaceX.AddSample(faceX);
                        runningAverageFaceY.AddSample(faceY);
                        runningAverageFaceZ.AddSample(faceZ);
                    }
                    else
                    {
                        if (runningAverageFaceZ.Average > 0)
                        {
                            faceX = runningAverageFaceX.Average;
                            faceY = runningAverageFaceY.Average;
                            faceZ = runningAverageFaceZ.Average;
                        }
                        faceTransitionState = FaceTrackingStateEngine.FaceTransitionState.ReducingBaseline;
                    }
                }
            }
            else
            {
                faceTransitionState = FaceTrackingStateEngine.FaceTransitionState.ReducingBaseline;
                requestedState = TrackingState.NotFaceTracking;
            }
        }
    }

    public virtual AbstractTrackingResult GetDeviceTrackingResult()
    {
        //Intentionally left empty.
        //See EyeTrackingAndroid.cs and EyeTrackingWindows.cs for override implementations.
        return null;
    }

    public float GetFrameDelay()
    {
        return delay;
    }


    void OnApplicationQuit()
    {
        ApplicationQuitting = true;
        TerminateHeadTracking();
    }

    protected virtual void TerminateHeadTracking()
    {
        IsTracking = false;

    }

}

public class AbstractTrackingResult
{
    public AbstractTrackingResult()
    {
        Faces = new List<Face>();
    }

    private int chosenFaceIndex = 0;
    public int NumFaces
    {
        get
        {
            return Faces.Count;
        }
    }

    public int ChosenFaceIndex { get => chosenFaceIndex; protected set => chosenFaceIndex = value; }
    public List<Face> Faces { get => faces; set => faces = value; }

    private List<Face> faces;

    public void AddFace(Face face)
    {
        Faces.Add(face);
    }
}

public class Face
{
    private int faceIndex;
    private Vector3 nonPredictedPosition;
    private Vector3 predictedPosition;
    private Vector3 velocity;
    private Vector3 angle;

    public int FaceIndex { get => faceIndex; set => faceIndex = value; }
    public Vector3 NonPredictedPosition { get => nonPredictedPosition; set => nonPredictedPosition = value; }
    public Vector3 PredictedPosition { get => predictedPosition; set => predictedPosition = value; }
    public Vector3 Velocity { get => velocity; set => velocity = value; }
    public Vector3 Angle { get => angle; set => angle = value; }

    public Face(int FaceIndex, Vector3 NonPredictedPosition, Vector3 PredictedPosition, Vector3 Velocity, Vector3 Angle)
    {
        this.FaceIndex = FaceIndex;
        this.NonPredictedPosition = NonPredictedPosition;
        this.PredictedPosition = PredictedPosition;
        this.Velocity = Velocity;
        this.Angle = Angle;
    }
}

using UnityEngine;
using LeiaLoft;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LeiaVirtualDisplay : MonoBehaviour
{
    [SerializeField] private float height = 5f;
    private float heightPrevious;
    [SerializeField] private bool ShowInEditMode = false;
    [SerializeField] private bool ShowAtRuntime = false;

    bool showInEditModePrev;
    bool showAtRuntimePrev;

    float screenWidth = 1000;
    float screenHeight = 1000;

    public float width
    {
        get
        {
            return this.height * screenWidth / screenHeight;
        }
    }

    float convergenceSmoothed;

    public void SetHeight(float height)
    {
        this.height = height;
    }

    public float Width
    {
        get
        {
            return this.width;
        }
    }

    public float Height
    {
        get
        {
            return this.height;
        }
        set
        {
            this.height = Mathf.Clamp(value, .0001f, float.MaxValue);
        }
    }

    [HideInInspector]
    public Transform[] corners;
    [HideInInspector, SerializeField] private Transform[] sides;
    [HideInInspector, SerializeField] private Transform logo;

    Transform _model;
    Transform model
    {
        get
        {
            if (_model == null)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).name.Contains("LeiaVirtualDisplayModel"))
                    {
                        _model = transform.GetChild(i);
                        _model.transform.parent = transform;
                        _model.localPosition = Vector3.zero;
                        break;
                    }
                }
                if (_model == null)
                {
                    GameObject LeiaVirtualDisplayModel = Instantiate(Resources.Load("LeiaVirtualDisplayModel")) as GameObject;
                    _model = LeiaVirtualDisplayModel.transform;
                    _model.transform.parent = transform;
                    _model.localPosition = Vector3.zero;
                }
            }

            return _model;
        }
    }

    public ControlMode DefaultControlMode = ControlMode.DrivenByLeiaCamera;

    public enum ControlMode { DrivesLeiaCamera, DrivenByLeiaCamera };

    private ControlMode _controlMode = ControlMode.DrivenByLeiaCamera;

    public ControlMode controlMode
    {
        get
        {
            return _controlMode;
        }
        set
        {
            _controlMode = value;
        }
    }

    LeiaCamera _leiaCamera;
    public LeiaCamera leiaCamera
    {
        get
        {
            if (_leiaCamera == null)
            {
                _leiaCamera = GetComponentInChildren<LeiaCamera>();
                if (_leiaCamera == null)
                {
                    _leiaCamera = transform.parent.GetComponent<LeiaCamera>();
                    if (_leiaCamera == null)
                    {
                        Debug.LogError("No LeiaCamera found for virtual display on gameobject "+gameObject.name);
                    }
                }
            }

            return _leiaCamera;
        }
    }

    void Start()
    {
        UpdateDisplayGizmos();
    }


    public void UpdateCameraFromVirtualDisplay()
    {
        if (Application.isPlaying)
        {
            Debug.Log("UpdateCameraFromVirtualDisplay");
        }
        //given LeiaCamera FOV angle and height of the virtual display, calculate what distance should be
        //l = h/tan(θ)
        float length = (height / 2f) / Mathf.Tan((leiaCamera.Camera.fieldOfView / 2f) * Mathf.Deg2Rad);

        if (!Application.isPlaying)
        {
            //If app not playing then reset the camera to default position

            leiaCamera.transform.localPosition = new Vector3(0, 0, -1) * length;
            leiaCamera.ConvergenceDistance = length;
        }
    }

    public void UpdateVirtualDisplayFromCamera()
    {
        //if (Application.isPlaying)
        //{
            //Debug.Log("UpdateVirtualDisplayFromCamera"); //we know it gets here
        //}
        height = Mathf.Tan((leiaCamera.Camera.fieldOfView / 2f) / Mathf.Rad2Deg) * (2f * leiaCamera.ConvergenceDistance);
        transform.localPosition = new Vector3(0, 0, leiaCamera.ConvergenceDistance);
        transform.localRotation = Quaternion.identity;
    }

    public void UpdateDisplayGizmos()
    {
        //Debug.Log("UpdateDisplayGizmos");
#if UNITY_EDITOR
        screenHeight = (leiaCamera.ConvergenceDistance * Mathf.Tan(leiaCamera.Camera.fieldOfView * Mathf.PI / 360.0f) * 2f);
        screenWidth = (GameViewUtils.GetGameViewAspectRatio() * (screenHeight));
#else
       screenWidth = Screen.width;
       screenHeight = Screen.height;
#endif

        if (model != null)
        {
            if (logo == null)
            {
                logo = model.Find("LeiaLogo");
            }

            if (sides == null || sides.Length == 0 || sides[0] == null)
            {
                sides = new Transform[4];
                sides[0] = model.Find("Side1");
                sides[1] = model.Find("Side2");
                sides[2] = model.Find("Side3");
                sides[3] = model.Find("Side4");

                if (sides[0] == null)
                {
                    sides[0] = model.GetChild(4);
                    sides[1] = model.GetChild(5);
                    sides[2] = model.GetChild(6);
                    sides[3] = model.GetChild(7);
                }
            }
            if (corners == null || corners.Length == 0 || corners[0] == null)
            {
                corners = new Transform[4];
                corners[0] = model.Find("Corner1");
                corners[1] = model.Find("Corner2");
                corners[2] = model.Find("Corner3");
                corners[3] = model.Find("Corner4");

                if (corners[0] == null)
                {
                    corners[0] = model.GetChild(0);
                    corners[1] = model.GetChild(1);
                    corners[2] = model.GetChild(2);
                    corners[3] = model.GetChild(3);
                }
            }
        }

        if (Application.isPlaying)
        {
            LeiaDisplay.Instance.cameraDriven = (this.controlMode == ControlMode.DrivenByLeiaCamera);
        }

        if (height < .0001f)
        {
            height = .0001f;
        }

        if (this.controlMode == ControlMode.DrivenByLeiaCamera)
        {
            this.UpdateVirtualDisplayFromCamera();
        }

        if (this.controlMode == ControlMode.DrivesLeiaCamera)
        {
            if (transform.localScale.x != 1)
            {
#if UNITY_EDITOR
                Undo.RecordObject(this, "Set Virtual Display Scale");
#endif
                this.height *= transform.localScale.x;
                transform.localScale = Vector3.one;
            }
            if (transform.localScale.y != 1)
            {
#if UNITY_EDITOR
                Undo.RecordObject(this, "Set Virtual Display Scale");
#endif
                this.height *= transform.localScale.y;
                transform.localScale = Vector3.one;
            }
            if (transform.localScale.z != 1)
            {
#if UNITY_EDITOR
                Undo.RecordObject(this, "Set Virtual Display Scale");
#endif
                this.height *= transform.localScale.z;
                transform.localScale = Vector3.one;
            }
        }

        float thickness = this.height * .05f;

        if (Application.isPlaying)
        {
            DisplayConfig config = LeiaDisplay.Instance.GetDisplayConfig();
            float displayHeightMM = config.PanelResolution.y * config.PixelPitchInMM.y;

            if (this.controlMode == ControlMode.DrivesLeiaCamera)
            {
                //Set camera Z position based on eye tracking
                float d = LeiaDisplay.Instance.tracker.faceZ * (height) / displayHeightMM;
                convergenceSmoothed += (d - convergenceSmoothed) * Mathf.Min((Time.deltaTime * 15f), 1f);
                if (leiaCamera.cameraZaxisMovement)
                {
                    //Set camera z-position
                    leiaCamera.ConvergenceDistance = convergenceSmoothed;
                }
                else
                {
                    leiaCamera.ConvergenceDistance = LeiaDisplay.Instance.displayConfig.ConvergenceDistance * (height) / displayHeightMM;
                }

                leiaCamera.transform.localPosition = new Vector3(0, 0, -leiaCamera.ConvergenceDistance);
            }
        }

        if (this.controlMode == ControlMode.DrivesLeiaCamera)
        {
            this.UpdateCameraFromVirtualDisplay();
        }

        //Update the virtual display model
        if (model != null)
        {
            heightPrevious = height;

            sides[0].localPosition = new Vector3(0, height / 2f + .5f * thickness, 0);

            sides[1].localPosition = new Vector3(0, -height / 2f - .5f * thickness, 0);
            sides[2].localPosition = new Vector3(width / 2f + .5f * thickness, 0, 0);
            sides[3].localPosition = new Vector3(-width / 2f - .5f * thickness, 0, 0);
            sides[0].localScale = new Vector3(width, thickness, thickness);
            sides[1].localScale = new Vector3(width, thickness, thickness);
            sides[2].localScale = new Vector3(thickness, height, thickness);
            sides[3].localScale = new Vector3(thickness, height, thickness);
            corners[0].localPosition = new Vector3(width / 2f + .5f * thickness, height / 2f + .5f * thickness, 0);
            corners[1].localPosition = new Vector3(-width / 2f - .5f * thickness, height / 2f + .5f * thickness, 0);
            corners[2].localPosition = new Vector3(width / 2f + .5f * thickness, -height / 2f - .5f * thickness, 0);
            corners[3].localPosition = new Vector3(-width / 2f - .5f * thickness, -height / 2f - .5f * thickness, 0);
            corners[0].localScale = new Vector3(thickness, thickness, thickness);
            corners[1].localScale = new Vector3(thickness, thickness, thickness);
            corners[2].localScale = new Vector3(thickness, thickness, thickness);
            corners[3].localScale = new Vector3(thickness, thickness, thickness);

            corners[0].localRotation = Quaternion.Euler(0, 0, 270);
            corners[1].localRotation = Quaternion.Euler(0, 0, 0);
            corners[2].localRotation = Quaternion.Euler(0, 0, 180);
            corners[3].localRotation = Quaternion.Euler(0, 0, 90);

            logo.localPosition = new Vector3(0, -height / 2f - .5f * thickness, -thickness / 2f);
            logo.localScale = new Vector3(1f, 1f, 1f) * thickness / 2f;

            bool virtualDisplayIsVisible = false;
#if UNITY_EDITOR
            if (!Application.isPlaying && (ShowInEditMode || (gameObject == Selection.activeGameObject)))
            {
                virtualDisplayIsVisible = true;
            }
            if (Application.isPlaying && ShowAtRuntime)
            {
                virtualDisplayIsVisible = true;
            }
#endif

            for (int i = 0; i < sides.Length; i++)
            {
                sides[i].GetComponent<MeshRenderer>().enabled = virtualDisplayIsVisible;

#if UNITY_EDITOR
                if (Selection.Contains(sides[i].gameObject))
                {
                    Selection.objects = new GameObject[] { gameObject };
                }
#endif
            }
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i].GetComponent<MeshRenderer>().enabled = virtualDisplayIsVisible;

#if UNITY_EDITOR
                if (Selection.Contains(corners[i].gameObject))
                {
                    Selection.objects = new GameObject[] { gameObject };
                }
#endif
            }

            MeshRenderer[] logoMeshRenderers = logo.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < logoMeshRenderers.Length; i++)
            {
                logoMeshRenderers[i].enabled = virtualDisplayIsVisible;
            }

#if UNITY_EDITOR
            if (Selection.Contains(logo.gameObject))
            {
                Selection.objects = new GameObject[] { gameObject };
            }
#endif
        }
    }
    void LateUpdate()
    {
        UpdateDisplayGizmos();
    }


    GameObject previousSelectedObject;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!SceneView.currentDrawingSceneView)
        {
            UpdateDisplayGizmos();
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (Selection.activeGameObject != previousSelectedObject)
            {
                if (Selection.activeGameObject == gameObject)
                {
                    //This GameObject is selected in the Editor
                    SetVirtualDisplayAsParent();
                }
                else if (Selection.activeGameObject == this.leiaCamera.gameObject)
                {
                    SetLeiaCameraAsParent();
                }
                else
                {
                    if (previousSelectedObject == gameObject)
                    {
                        //By default, set the LeiaCamera as the parent
                        if (DefaultControlMode == ControlMode.DrivenByLeiaCamera)
                        {
                            SetLeiaCameraAsParent();
                        }
                        else
                        {
                            SetVirtualDisplayAsParent();
                        }
                    }
                }
                previousSelectedObject = Selection.activeGameObject;
            }
        }
    }
#endif

    public void SetLeiaCameraAsParent()
    {
        if (!Application.isPlaying)
        {
            if (transform.parent != leiaCamera.transform)
            {
                if (!IsPartOfPrefab)
                {
                    //This GameObject is selected in the Editor
                    this.controlMode = ControlMode.DrivenByLeiaCamera;
                    Transform newParent = transform.parent;
                    leiaCamera.transform.parent = null;
                    transform.parent = leiaCamera.transform;
                    leiaCamera.transform.parent = newParent;
                }
            }
        }
    }

    public void SetVirtualDisplayAsParent()
    {
        if (!Application.isPlaying)
        {
            if (!IsPartOfPrefab)
            {
                this.controlMode = ControlMode.DrivesLeiaCamera;
                Transform newParent = leiaCamera.transform.parent;
                transform.parent = null;
                leiaCamera.transform.parent = transform;
                transform.parent = newParent;
            }
            else
            {
                Debug.Log("Is part of prefab, so not doing it");
            }
        }
    }

    public bool IsPartOfPrefab
    {
        get
        {
#if UNITY_EDITOR
            bool parentIsAPrefab = false;

            if (transform.parent != null && PrefabUtility.GetPrefabAssetType(transform.parent.gameObject) != PrefabAssetType.NotAPrefab)
            {
                parentIsAPrefab = true;
            }

            bool thisIsAPrefab = PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab;

            return (thisIsAPrefab || parentIsAPrefab);
#else
            return false;
#endif
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;

public class TrackingBackendDropdown : MonoBehaviour
{
    Dropdown dropdown;
    [SerializeField] Text CurrentTrackingBackend;
    void Start()
    {
        dropdown = GetComponent<Dropdown>();
    }

    public void OnValueChanged(int selected)
    {
        switch (selected)
        {
            case 0:
                SetCPU();
                break;
            case 1:
                SetGPU();
                break;
        }
    }

    public void SetCPU()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Leia.FaceDetectorConfig detectorConfig = new Leia.FaceDetectorConfig();
        detectorConfig.backend = Leia.FaceDetectorBackend.CPU;
        LeiaDisplay.Instance.CNSDK.SetFaceTrackingConfig(detectorConfig);
        CurrentTrackingBackend.text = "Current Backend: CPU";
#endif
    }

    public void SetGPU()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Leia.FaceDetectorConfig detectorConfig = new Leia.FaceDetectorConfig();
        detectorConfig.backend = Leia.FaceDetectorBackend.GPU;
        LeiaDisplay.Instance.CNSDK.SetFaceTrackingConfig(detectorConfig);
        CurrentTrackingBackend.text = "Current Backend: GPU";
#endif
    }
}

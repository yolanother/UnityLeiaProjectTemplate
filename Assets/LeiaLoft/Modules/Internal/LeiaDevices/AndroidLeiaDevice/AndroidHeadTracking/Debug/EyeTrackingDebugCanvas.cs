using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;

public class EyeTrackingDebugCanvas : MonoBehaviour
{
    [SerializeField] private GameObject EyeTrackingStatusBar;
    [SerializeField] private Text textLabel;
    EyeTracking eyeTracking;

    [SerializeField] private GameObject logoScene;
    [SerializeField] private GameObject patternMediaViewer;

    LeiaDisplay leiaDisplay;

    [SerializeField] private EyeTrackingModeDropdown eyeTrackingMode;

    bool eyeTrackingNotSupportedInEditorMessageVisible = true;
    readonly float eyeTrackingNotSupportedInEditorMessageDuration = 5f;

    // Start is called before the first frame update

    void Start()
    {
        leiaDisplay = FindObjectOfType<LeiaDisplay>();
        eyeTracking = FindObjectOfType<EyeTracking>();
        Invoke("UpdateStatus", 1f);
        Invoke("HideEditorMessage", eyeTrackingNotSupportedInEditorMessageDuration);
    }

    public void ShowPattern(bool visible)
    {
        if (logoScene != null)
        {
            logoScene.SetActive(!visible);
        }
        if (patternMediaViewer != null)
        {
            patternMediaViewer.SetActive(visible);
        }
    }

    public void HideEditorMessage()
    {
        eyeTrackingNotSupportedInEditorMessageVisible = false;
    }

    void UpdateStatus()
    {
        if (!eyeTrackingNotSupportedInEditorMessageVisible)
        {
            EyeTrackingStatusBar.SetActive(false);
            return;
        }
        Invoke("UpdateStatus", .1f);

        
        if (!eyeTracking.CameraConnected)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                textLabel.text = "Eye tracking camera not connected. Check the USB connection.";
            }
            EyeTrackingStatusBar.SetActive(true && leiaDisplay.eyeTrackingStatusBarEnabled);
        }
        else if (eyeTracking.NumFaces == 0)
        {
            textLabel.text = "No faces detected.";
            EyeTrackingStatusBar.SetActive(true && leiaDisplay.eyeTrackingStatusBarEnabled);
        }
        else
        {
            EyeTrackingStatusBar.SetActive(false);
        }
    }
}

using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

public class EyeTrackingModeDropdown : MonoBehaviour
{
    LeiaDisplay _leiaDisplay;
    LeiaDisplay leiaDisplay
    {
        get
        {
            if (!_leiaDisplay)
            {
                _leiaDisplay = LeiaDisplay.Instance;
            }
            return _leiaDisplay;
        }
    }
    Dropdown dropdown;

    void Start()
    {
        dropdown = GetComponent<Dropdown>();
    }

    public void OnValueChanged(int selected)
    {
        switch (selected)
        {
            case 0:
                leiaDisplay.AutoRenderTechnique = true;
                leiaDisplay.SetPerPixelCorrectionEnabled(true);
                break;
            case 1:
                SetTrackedStereoMode();
                leiaDisplay.AutoRenderTechnique = false;
                break;
            case 2:
                SetViewPeelingMode();
                leiaDisplay.AutoRenderTechnique = false;
                break;
    
        }
    }
    
    
    
    public void SetViewPeelingMode()
    {
        //View Peeling
        leiaDisplay.DesiredRenderTechnique = LeiaDisplay.RenderTechnique.Multiview;
        leiaDisplay.SetPerPixelCorrectionEnabled(true);
        dropdown.SetValueWithoutNotify(2);
    }

    public void SetTrackedStereoMode()
    {
        //Tracked Stereo
        leiaDisplay.DesiredRenderTechnique = LeiaDisplay.RenderTechnique.Stereo;
        leiaDisplay.SetPerPixelCorrectionEnabled(true);
        dropdown.SetValueWithoutNotify(1);
    }
}

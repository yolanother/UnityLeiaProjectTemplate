using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

namespace LeiaLoft.Examples
{
    public class CameraShiftSliderInitializer : MonoBehaviour
    {
        Slider slider;
        // Use this for initialization
        void Start()
        {
            slider = GetComponent<Slider>();
            slider.value = LeiaCamera.Instance.CameraShiftScaling;
            slider.onValueChanged.AddListener (delegate {SetCameraShiftScaling();});
        }

        void SetCameraShiftScaling()
        {
            LeiaCamera.Instance.CameraShiftScaling = slider.value;
        }
    }
}

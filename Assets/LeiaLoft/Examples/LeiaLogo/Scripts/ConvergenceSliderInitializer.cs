using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

namespace LeiaLoft.Examples
{
    public class ConvergenceSliderInitializer : MonoBehaviour
    {
        Slider slider;
        void Start()
        {
            slider = GetComponent<Slider>();
            slider.value = LeiaCamera.Instance.ConvergenceDistance;
            slider.onValueChanged.AddListener (delegate {SetConvergence();});
        }

        void SetConvergence()
        {
            LeiaCamera.Instance.ConvergenceDistance = slider.value;
        }
    }
}

using LeiaLoft;
using UnityEngine;
using UnityEngine.UI;

namespace LeiaLoft.Examples
{
    public class FOVSliderInitializer : MonoBehaviour
    {
#pragma warning disable 649 // Suppress warning that var is never assigned to and will always be null
        [SerializeField] private LeiaCamera leiaCamera;
#pragma warning restore 649
        
        Slider slider;
        // Use this for initialization
        void Start()
        {
            if (leiaCamera == null)
            {
                leiaCamera = FindObjectOfType<LeiaCamera>();
            }
            slider = GetComponent<Slider>();
            slider.value = leiaCamera.Camera.fieldOfView;
            slider.onValueChanged.AddListener (delegate {SetFOV();});
        }
        
        void SetFOV()
        {
            LeiaCamera.Instance.Camera.fieldOfView = slider.value;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;

public class PeelOffsetLabel : MonoBehaviour
{
    Text text;
    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        Vector3 facePosition = LeiaDisplay.Instance.tracker.GetPredictedFacePosition();
        text.text = "Peel Offset: "+(LeiaDisplay.Instance.getPeelOffsetForShader())
            + "\nDisplay Offset: "+LeiaDisplay.Instance.displayOffset
            + "\nNo: "+LeiaDisplay.Instance.No
            + "\nnumViews: "+LeiaDisplay.Instance.numViews
            + "\nFaceX: "+facePosition.x
            + "\nFaceY: "+facePosition.y
            + "\nFaceZ: "+facePosition.z
            ;
    }
}

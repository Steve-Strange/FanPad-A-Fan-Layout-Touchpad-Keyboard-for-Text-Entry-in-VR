using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class TouchPointVisualizeOnKeyboard : MonoBehaviour
{
    public SteamVR_ActionSet keyboardActionSet;
    public SteamVR_Action_Vector2 PadSlide = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("keyboard", "slide");  //��touchpad�ϻ���
    public SteamVR_Action_Boolean PadTouch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "touch");  //touchpad触摸
    public Transform touchpointL, touchpointR;
    public Transform backboardL, backboardR;

    public GameObject fitting;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        SteamVR_Input_Sources[] tmp = new SteamVR_Input_Sources[] { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
        keyboardActionSet.Activate();
        foreach (var hand in tmp)
        {
            PadSlide[hand].onChange += MoveTouchPoint;
            PadTouch[hand].onStateUp += OnTouchUp;
        }
    }

    private void OnDisable()
    {
        SteamVR_Input_Sources[] tmp = new SteamVR_Input_Sources[] { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
        
        keyboardActionSet.Deactivate();
        foreach (var hand in tmp)
        {
            PadSlide[hand].onChange -= MoveTouchPoint;
            PadTouch[hand].onStateUp -= OnTouchUp;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveTouchPoint(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {

        // float radius = fitting.GetComponent<Fitting>().radius;
        // float theta = fitting.GetComponent<Fitting>().theta;
        // float cotValue = 1 / Mathf.Tan(theta);
        // float a = axis.x;
        // float b = axis.y;


        if(fromSource == SteamVR_Input_Sources.LeftHand)
        {
        //     float x1 = (-b * cotValue + a - Mathf.Sqrt(Mathf.Pow(b * cotValue - a, 2) - (1 + Mathf.Pow(cotValue, 2)) * (Mathf.Pow(a, 2) + Mathf.Pow(b, 2) - radius * radius))) / (1 + Mathf.Pow(cotValue, 2));
        //     float y1 = cotValue * x1;
        //     float cosTheta = Vector2.Dot(new Vector2(x1, y1) - axis, new Vector2(x1, y1)) / ((new Vector2(x1, y1) - axis).magnitude * new Vector2(x1, y1).magnitude);
        //     float sinTheta = Mathf.Sqrt(1 - cosTheta*cosTheta);
        //     float distance = (y1 + radius * Mathf.Cos(theta)) / Mathf.Cos(theta);

        //     float x1_final = (distance - 7f) * Mathf.Sin(0.6f);
        //     float y1_final = - (distance - 7f) * Mathf.Cos(0.6f);

        //     // 将向量 a 应用旋转矩阵
        //     Vector2 TouchPointVector = new Vector2(
        //         cosTheta * Mathf.Sin(0.6f) - sinTheta * Mathf.Cos(0.6f),
        //         sinTheta * Mathf.Sin(0.6f) + cosTheta * Mathf.Cos(0.6f)
        //     ) * (-7);
            

        //     Debug.LogWarning(cosTheta);
        //     Debug.LogWarning(sinTheta);
        //     Debug.LogWarning(x1_final);
        //     Debug.LogWarning(y1_final);
        //     Debug.LogWarning(TouchPointVector);

            touchpointL.localPosition = new Vector3(axis.x * backboardL.localScale.x / 2.3f, 2.4f, axis.y * backboardL.localScale.x / 2.3f);
            touchpointR.localPosition = new Vector3(0,0,0);
        }
        else
        {
            // float x1 = (-b * cotValue + a + Mathf.Sqrt(Mathf.Pow(b * cotValue - a, 2) - (1 + Mathf.Pow(cotValue, 2)) * (Mathf.Pow(a, 2) + Mathf.Pow(b, 2) - radius * radius))) / (1 + Mathf.Pow(cotValue, 2));
            // float y1 = -cotValue * x1;

            touchpointL.localPosition = new Vector3(0,0,0);
            touchpointR.localPosition = new Vector3(axis.x * backboardR.localScale.x / 2.3f, 2.4f, axis.y * backboardR.localScale.x / 2.3f);
        }
        

    }

    public void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        touchpointR.localPosition = new Vector3(0,0,0);
        touchpointL.localPosition = new Vector3(0,0,0);
    }
}

// 7
// 0.6

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class TouchPointVisualize : MonoBehaviour
{
    public SteamVR_ActionSet keyboardActionSet;
    public SteamVR_Action_Vector2 PadSlide = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("keyboard", "slide");  //在touchpad上滑动
    public RectTransform touchpoint;
    public RectTransform touchpadL, touchpadR;
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

        }
    }

    private void OnDisable()
    {
        SteamVR_Input_Sources[] tmp = new SteamVR_Input_Sources[] { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
        keyboardActionSet.Deactivate();
        foreach (var hand in tmp)
        {
            PadSlide[hand].onChange -= MoveTouchPoint;

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveTouchPoint(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        if(fromSource == SteamVR_Input_Sources.LeftHand)
        {
            //touchpoint.parent = touchpadL;
            touchpoint.SetParent(touchpadL);
        }
        else
        {
            //touchpoint.parent = touchpadR;
            touchpoint.SetParent(touchpadR);
        }
        // 设置点坐标
        touchpoint.localPosition = axis * touchpadL.sizeDelta / 2;
    }
}

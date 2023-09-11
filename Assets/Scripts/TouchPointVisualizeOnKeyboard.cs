using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class TouchPointVisualizeOnKeyboard : MonoBehaviour
{
    public SteamVR_ActionSet keyboardActionSet;
    public SteamVR_Action_Vector2 PadSlide = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("keyboard", "slide");  //��touchpad�ϻ���
    public Transform touchpoint;
    public Transform backboardL, backboardR;
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
            touchpoint.SetParent(backboardL);
        }
        else
        {
            //touchpoint.parent = touchpadR;
            touchpoint.SetParent(backboardR);
        }
        // ���õ�����
        
        

        touchpoint.localPosition = new Vector3(axis.x * backboardL.localScale.x / 2, touchpoint.localPosition.y, axis.y * backboardL.localScale.x / 2);

    }
}

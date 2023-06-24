using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/* 普通键盘，继承自ClickKeyboard，应当只用实现自己的Axis2Letter方法. */
public class NormalKeyboard : ClickKeyboard
{
    private void Update()
    {
        GameObject key;
        if (touched)
        {
            if (PadSlide[SteamVR_Input_Sources.LeftHand].axis != new Vector2(0, 0))
            {
                Axis2Letter(PadSlide[SteamVR_Input_Sources.LeftHand].axis, SteamVR_Input_Sources.LeftHand, 0, out key);
                //Debug.Log("Key: " + ascii);
            }
            if (PadSlide[SteamVR_Input_Sources.RightHand].axis != new Vector2(0, 0))
            {
                Axis2Letter(PadSlide[SteamVR_Input_Sources.RightHand].axis, SteamVR_Input_Sources.RightHand, 0, out key);
                //Debug.Log("Key: " + ascii);
            }
        }
    }

    private char[,,] keys = new char[5, 4, 2];
    public override int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode, out GameObject key)
    {
        int row, column;
        // TODO!! 普通键盘的映射.
        if (axis.y <= -0.45) row = 0;
        else if (axis.y < 0 && axis.y > -0.45) row = 1;
        else if (axis.y > 0 && axis.y < 0.45) row = 2;
        else row = 3;

        float width = Mathf.Sqrt(1 - axis.y * axis.y);
        print(width);
        print(axis.x);
        float columnRatio = (axis.x + width / 2) / (2 * width / 5);
        column = (int)columnRatio + 1;

        print(columnRatio);
        Debug.Log("( " + column + ' ' + row + " )");
        key = this.gameObject;
        return 0;
    }
}

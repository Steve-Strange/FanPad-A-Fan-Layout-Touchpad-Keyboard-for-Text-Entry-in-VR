using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/* 普通键盘，继承自ClickKeyboard，应当只用实现自己的Axis2Letter方法. */
public class NormalKeyboard : ClickKeyboard
{

    //private void Update()
    //{
    //    GameObject key;
    //    if (touched)
    //    {
    //        if (PadSlide[SteamVR_Input_Sources.LeftHand].axis != new Vector2(0, 0))
    //        {
    //            Axis2Letter(PadSlide[SteamVR_Input_Sources.LeftHand].axis, SteamVR_Input_Sources.LeftHand, 0, out key);
    //            //Debug.Log("Key: " + ascii);
    //        }
    //        if (PadSlide[SteamVR_Input_Sources.RightHand].axis != new Vector2(0, 0))
    //        {
    //            Axis2Letter(PadSlide[SteamVR_Input_Sources.RightHand].axis, SteamVR_Input_Sources.RightHand, 0, out key);
    //            //Debug.Log("Key: " + ascii);
    //        }
    //    }
    //}

    private int[,,] keys = new int[6, 4, 5] { { { 0, ',', 0x20, 0x20, 0x20 }, { 0x10, 'z', 'x', 'c', 'v' }, { 'a', 's', 'd', 'f', 'g' } ,{ 'q', 'w', 'e', 'r', 't' }},
                                              { { 0, ',', 0x20, 0x20, 0x20 }, { 0x10, 'Z', 'X', 'C', 'V' }, { 'A', 'S', 'D', 'F', 'G' } ,{ 'Q', 'W', 'E', 'R', 'T' }},
                                              { { 0, ',', 0x20, 0x20, 0x20 }, { 0x10, '(', ')', '-', '_' }, { '~', '!', '@', '#', '%' } ,{ '1', '2', '3', '4', '5' }},
                                              { { 0x20, 0x20, 0x20, '.', 0x0D }, { 'v', 'b', 'n', 'm', 0x08 }, { 'g', 'h', 'j', 'k', 'l' } ,{ 'y', 'u', 'i', 'o', 'p' }},
                                              { { 0x20, 0x20, 0x20, '.', 0x0D }, { 'V', 'B', 'N', 'M', 0x08 }, { 'G', 'H', 'J', 'K', 'L' } ,{ 'Y', 'U', 'I', 'O', 'P' }},
                                              { { 0x20, 0x20, 0x20, '.', 0x0D }, { '_', ':', ';', '/', 0x08 }, { '%', '\'', '&', '*', '?' } ,{ '6', '7', '8', '9', '0' }} };
    public override int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode, out GameObject key)
    {
        Debug.Log("Source: " + hand);
        int row, column;
        // TODO!! 普通键盘的映射.
        if (axis.y <= -0.4) row = 0;
        else if (axis.y < 0 && axis.y > -0.4) row = 1;
        else if (axis.y > 0 && axis.y < 0.4) row = 2;
        else row = 3;

        float width = Mathf.Sqrt(1 - axis.y * axis.y);
        float columnRatio = (axis.x + width) / (2 * width / 5);
        column = Mathf.FloorToInt(columnRatio);
        if (column > 4) column = 4;
        else if (column < 0) column = 0;

        int handmode = (hand == SteamVR_Input_Sources.LeftHand) ? mode : mode + 3;
        char output = (char)keys[handmode, row, column];
        print(handmode);
        print(output);

        switch (output)
        {
            case (char)VKCode.Space:
                key = keyboardRoot.Find("space").gameObject;
                break;
            case (char)VKCode.Shift:
                key = keyboardRoot.Find("shift").gameObject;
                break;
            case (char)VKCode.Switch:
                key = keyboardRoot.Find("sym").gameObject;
                break;
            case (char)VKCode.Enter:
                key = keyboardRoot.Find("enter").gameObject;
                break;
            case (char)VKCode.Back:
                key = keyboardRoot.Find("back").gameObject;
                break;
            case ',':
                key = keyboardRoot.Find("comma").gameObject;
                break;
            case '.':
                key = keyboardRoot.Find("period").gameObject;
                break;
            default:
                string name = ((char)keys[handmode - mode, row, column]).ToString() + ((char)keys[handmode - mode + 2, row, column]).ToString();
                if (name[1] == '/')
                    name = "m\\";
                key = keyboardRoot.Find(name).gameObject;
                break;
        }

        return keys[handmode, row, column];
    }
}

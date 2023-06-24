using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/* 普通键盘，继承自ClickKeyboard，应当只用实现自己的Axis2Letter方法. */
public class NormalKeyboard : ClickKeyboard
{
    public override int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode, ref GameObject key)
    {
        // TODO!! 普通键盘的映射.
        return 0;
    }
}

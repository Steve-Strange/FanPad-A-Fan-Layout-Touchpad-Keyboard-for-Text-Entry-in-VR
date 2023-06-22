using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public enum Keybd_Flags
{
    KeyDown,
    KeyHold,
    KeyUp,
}

public class KeyboardBase : MonoBehaviour
{
    // Start is called before the first frame update
    [DllImport("User32.dll", EntryPoint = "keybd_event")]
    static extern void keybd_event(byte bVK, byte bScan, int dwFlags, int dwExtraInfo);

    /*
     * TODO:
     * 获取手柄各个按键、触摸板的action.并且能够注册.
     * 按下、滑动等回调函数的虚函数（接口），并且在一开始就在Base中注册这些回调函数.
     * 这样在具体的键盘类中就只需要实现这些回调函数及相关逻辑.
     * 考虑到可能将要用trigger实现，也许这里可以加个map？或者存储当前位于哪里的变量。然后onTriggerEnter是否也应该在这里?
     * 但是如果只是一下子touch马上又松开，trigger是否可能有反应不过来的情况？这时候要主动用一个Axis2Key把Action的坐标映射为符号，是否也需要在这里实现?
     * 
     */


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PushKey(byte bVK)
    {
        // 按下按键
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyDown, 0);
    }

    void ReleaseKey(byte bVK)
    {
        // 松开按键
        // 一定不要忘记按下了要松开.
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyUp, 0);
    }
}

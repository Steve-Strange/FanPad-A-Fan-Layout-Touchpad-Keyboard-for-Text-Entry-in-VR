using UnityEngine;
using Valve.VR;

public class Monitor : MonoBehaviour
{
    public SteamVR_Input_Sources handType; // 手柄类型，例如左手或右手
    public SteamVR_Action_Vector2 thumbstickAction; // 拇指杆输入的操作
    public SteamVR_Action_Boolean triggerAction; // 触发器按钮输入的操作

    void Update()
    {
        // 获取拇指杆的输入值
        Vector2 thumbstickValue = thumbstickAction.GetAxis(handType);
        Debug.Log("Thumbstick Value: " + thumbstickValue);

        // 获取触发器按钮的状态
        bool triggerPressed = triggerAction.GetState(handType);
        if (triggerPressed)
        {
            Debug.Log("Trigger Pressed");
        }
    }
}
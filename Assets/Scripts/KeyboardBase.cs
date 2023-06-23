
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
<<<<<<< Updated upstream
using Valve.VR;
using Valve.VR.InteractionSystem;
using TMPro;
=======
>>>>>>> Stashed changes

public enum Keybd_Flags
{
    KeyDown,
    KeyHold,
    KeyUp,
}

<<<<<<< Updated upstream
/* 键盘的基类，它不应该被实例化，不应该被直接使用，只用于继承 */
/* 关于touchpad的几个回调函数，这里写的基本没太大意义，应在子类中完全重写.不要调用父类的 */
=======
>>>>>>> Stashed changes
public class KeyboardBase : MonoBehaviour
{
    // Start is called before the first frame update
    [DllImport("User32.dll", EntryPoint = "keybd_event")]
    static extern void keybd_event(byte bVK, byte bScan, int dwFlags, int dwExtraInfo);

<<<<<<< Updated upstream
    //Keyboard action set
    public SteamVR_ActionSet keyboardActionSet;

    // fetch actions.
    public SteamVR_Action_Boolean DeleteKey = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "delete");//按菜单键，这里是要删除.
    public SteamVR_Action_Boolean SelectKey = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "select");//按扳机选择，这里用作移动光标
    public SteamVR_Action_Boolean PadTouch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "touch");  //touchpad触摸
    public SteamVR_Action_Boolean PadPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "press");  //touchpad按下去
    public SteamVR_Action_Vector2 PadSlide = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("keyboard", "slide");  //在touchpad上滑动.

    // text input field. 固定的文字输入框。  
    public TextMeshProUGUI inputField;

    // 一些一定会共用的状态变量：selected, deleted, touched. longHolding: 标记是否长时间按着这个案件，弹出特殊字符选择.
    protected bool selected = false, deleted = false, touched = false, longHolding = false;
    protected float last_delete_time, hold_time_start;   //这是用来判断删除字符的时间间隔的，长按delete反复删除，但不应太快.

    /*
     * 确定，在继承KeyboardBase之后，只挂载这个继承的类，由于是直接在这个继承的类上调用函数，所以使用的函数都确定是继承类中重写的函数.
     * 比如，这里OnEnable注册回调函数，在子类中没改动，而Unity中挂载的是子类，所以调用是在子类中调用，没用用父类型调用子类函数的担忧.
     */

=======
    /*
     * TODO:
     * 获取手柄各个按键、触摸板的action.并且能够注册.
     * 按下、滑动等回调函数的虚函数（接口），并且在一开始就在Base中注册这些回调函数.
     * 这样在具体的键盘类中就只需要实现这些回调函数及相关逻辑.
     * 考虑到可能将要用trigger实现，也许这里可以加个map？或者存储当前位于哪里的变量。然后onTriggerEnter是否也应该在这里?
     * 但是如果只是一下子touch马上又松开，trigger是否可能有反应不过来的情况？这时候要主动用一个Axis2Key把Action的坐标映射为符号，是否也需要在这里实现?
     * 
     */


>>>>>>> Stashed changes
    void Start()
    {

    }
<<<<<<< Updated upstream
=======

    // Update is called once per frame
>>>>>>> Stashed changes
    void Update()
    {

    }
<<<<<<< Updated upstream
    private void OnEnable()
    {
        // 注册回调函数
        Debug.Log("Enable keyboard action set and relative callback functions!");
        keyboardActionSet.Activate();
        DeleteKey.onStateDown += OnDeleteKeyDown;
        DeleteKey.onStateUp += OnDeleteKeyUp;
        DeleteKey.onState += OnDeleteKeyHolding;
        SelectKey.onStateDown += OnSelectKeyDown;
        SelectKey.onStateUp += OnSelectKeyUp;
        PadTouch.onStateDown += OnTouchDown;
        PadTouch.onStateUp += OnTouchUp;
        PadPress.onStateUp += OnPressUp;
        PadPress.onStateDown += OnPressDown;
        PadSlide.onChange += OnPadSlide;
    }

    private void OnDisable()
    {
        Debug.Log("Disable keyboard action set!");
        keyboardActionSet.Deactivate();
        DeleteKey.onStateDown -= OnDeleteKeyDown;
        DeleteKey.onStateUp -= OnDeleteKeyUp;
        DeleteKey.onState -= OnDeleteKeyHolding;
        SelectKey.onStateDown -= OnSelectKeyDown;
        SelectKey.onStateUp -= OnSelectKeyUp;
        PadTouch.onStateDown -= OnTouchDown;
        PadTouch.onStateUp -= OnTouchUp;
        PadPress.onStateUp -= OnPressUp;
        PadPress.onStateDown -= OnPressDown;
        PadSlide.onChange -= OnPadSlide;
    }

    // 按下选择——移动光标的功能所有键盘是一样的，不用virtual. 并且这些都是无所谓左右手的.
    // 移动光标使用touchpad，就不需要用pose action了；
    public void OnSelectKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 松开Select键的回调函数（要放开光标） */
        selected = false;
        //TODO: 放下光标（或许什么也不用做即可）.
    }

    public void OnSelectKeyDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 按下Select键开始移动光标的回调函数. */
        selected = true;
    }

    public void OnDeleteKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 松开删除键. */
        deleted = false;
    }

    public void OnDeleteKeyDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 按下删除键.开始删除. */
        deleted = true;
        last_delete_time = Time.time;
        do_delete_char();
    }

    public void OnDeleteKeyHolding(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 一直按住DeleteKey, 要一直删除，不能太快，可能0.2s一个? onState的回调函数 */
        // 因为onState的前提就是按件状态是 true，所以其实这里用不上deleted.
        if(Time.time - last_delete_time > 0.2f)
        {
            last_delete_time = Time.time;
            do_delete_char();
        }
    }

    // 下面是和touchpad有关的了，不同键盘的行为可能不一样，需要virtual.
    virtual public void OnTouchDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 触摸到触摸板! */
        touched = true;
    }

    virtual public void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 松开触摸板，这个逻辑不一样了，click键盘松开触摸板会输出；而slide键盘不会 */
        touched = false;
    }

    virtual public void OnPressDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 按下触摸板的方法, 这个应该只有 SlideKeyboard 需要. */
    }

    virtual public void OnPressUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 按下触摸板之后又松开，这一定是要输出字符了
         * 但是由于大小写等mode的存在，仍旧是需要在子类中重载! */        
    }

    virtual public void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        /* 
         * 比较复杂的逻辑，当手指在touchpad上滑动的时候。注意需要**兼顾移动光标和处理键盘**! 
         * 可能要小心，两只手都只会通过这一个调用函数，所以应该区分.
         */

    }

    // 输出部分.
    virtual public int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode)
    {
        // 从axis与手判断要这是哪一个 返回Ascii码! 这是最终要输出的数字
        // mode: 模式，0-普通小写；1-大写；其他编号根据具体逻辑使用，比如有长按同一个字母弹出符号和数字等选择框的选项!
        // 大写等输入用虚拟键盘组合键实现.
        // 必须要在子类中实现.
        return 0;
    }

    public void OutputLetter(int ascii)
    {
        // 输出ascii码对应的字符，注意要将字符翻译成键盘输出，比如大写需要组合键.
        // TODO: 输出ascii字符；这可能需要定义Windows虚拟键码.
    }
    
    void do_delete_char()
    {
        // 从inputField中（在当前光标位置）删除1个字符.
        // TODO. 删除一个字符.
    }
=======
>>>>>>> Stashed changes

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
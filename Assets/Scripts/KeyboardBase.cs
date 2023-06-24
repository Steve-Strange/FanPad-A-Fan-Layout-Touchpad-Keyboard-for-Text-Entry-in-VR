
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Valve.VR;
using Valve.VR.InteractionSystem;
using TMPro;

public enum Keybd_Flags
{
    KeyDown = 0,
    KeyHold = 1,
    KeyUp = 2,
}

public enum SEEK_MOD
{
    Start = 0,
    Current = 1,
    End = 2
}

public class KeyboardBase : MonoBehaviour
{
    // Start is called before the first frame update
    [DllImport("User32.dll", EntryPoint = "keybd_event")]
    static extern void keybd_event(byte bVK, byte bScan, int dwFlags, int dwExtraInfo);

    //Keyboard action set
    public SteamVR_ActionSet keyboardActionSet;

    // fetch actions.
    public SteamVR_Action_Boolean DeleteKey = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "delete");//删除键-删除字符
    public SteamVR_Action_Boolean SelectKey = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "select");//扳机键-移动光标
    public SteamVR_Action_Boolean PadTouch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "touch");  //touchpad触摸
    public SteamVR_Action_Boolean PadPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "press");  //touchpad按下
    public SteamVR_Action_Vector2 PadSlide = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("keyboard", "slide");  //在touchpad上滑动

    // text input field. 
    public TextMeshProUGUI inputText;
    public TMP_InputField inputField;

    protected bool selected = false, deleted = false, touched = false, longHolding = false;
    protected float last_delete_time, hold_time_start;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnEnable()
    {
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
        Debug.Log("Disable keyboard action set and relative callback functions!");
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

    // 移动光标和删除逻辑，这些在所有键盘中都是一样的.
    public void OnSelectKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        selected = false;
    }

    public void OnSelectKeyDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        selected = true;
    }

    public void OnDeleteKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 松开删除键 */
        deleted = false;
    }

    public void OnDeleteKeyDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 按下删除键 */
        deleted = true;
        last_delete_time = Time.time;
        do_delete_char();
    }

    public void OnDeleteKeyHolding(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 长按删除键。是onState的回调函数，因为onState本身要在true才触发，所以不用判断是否true. */
        // 不能删的太快，比如相隔0.2s再删.
        // TODO
        if (Time.time - last_delete_time > 0.2f)
        {
            last_delete_time = Time.time;
            do_delete_char();
        }
    }

    // 下面是触摸板相关的，每个键盘不太一样.必须在外面重载!
    virtual public void OnTouchDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        touched = true;
    }

    virtual public void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        touched = false;
    }

    virtual public void OnPressDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 在SlideKeyboard中，要用这个开始判断长按. */
    }

    virtual public void OnPressUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 不管是什么键盘，这里都需要输出字符了! */
    }

    virtual public void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        /* 
         手指在触摸板上滑动。一个是以一定的规则移动改变高亮，一个在按下扳机的时候移动光标.
         */

    }

    // Core
    virtual public int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode, out GameObject key)
    {
        // 应该叫Axis2Char更好..
        // 将当前位置触摸板的位置转化为要输出的Ascii字符. 并且把当前所处的键盘位置的按件的GameObject
        // mode: 特殊状态，比如大写的状态，特殊符号的状态. 0- 小写， 1-大写， 2-特殊符号.
        key = this.gameObject;
        return 0;
    }

    public void OutputLetter(int ascii)
    {
        // 把Ascii符号翻译为键盘.
        // 更聪明的办法，应该是用一个ascii->VKcode的数组!
        // 注意，Shift, Enter等控制键不使用ascii，传入就直接用VKCode
        // 1. 字母
        if ('a' <= ascii && ascii <= 'z')
        {
            PutChar((byte)(ascii - 'a' + VKCode.Letters));
        }
        else if('A' <= ascii && ascii <= 'Z')
        {
            PushKey((byte)VKCode.Shift);
            PutChar((byte)(ascii - 'A' + VKCode.Letters));
            ReleaseKey((byte)VKCode.Shift);
        }
        // 2. 数字
        else if('0' <= ascii && ascii <= '9')
        {
            PutChar((byte)(ascii - '0' + VKCode.Numbers));
        }
        // 3. 其它符号, 枚举..
        else
        {
            bool needShift = false;
            switch (ascii)
            {
                case '<': case '>': case '?': case ':': case '\"': case '|': case '{': case '}':
                case '~': case '!': case '@': case '#': case '$': case '%': case '^': case '&':
                case '*': case '(': case ')': case '_': case '+':
                    PushKey((byte)VKCode.Shift);
                    needShift = true;
                    break;
            }
            switch (ascii)
            {
                case ',': case '<':
                    PutChar((byte)VKCode.Comma);
                    break;
                case '.': case '>':
                    PutChar((byte)VKCode.Period);
                    break;
                case '/': case '?':
                    PutChar((byte)VKCode.Slash);
                    break;
                case ';': case ':':
                    PutChar((byte)VKCode.Semicolon);
                    break;
                case '\"': case '\'':
                    PutChar((byte)VKCode.Quote);
                    break;
                case '\\': case '|':
                    PutChar((byte)VKCode.Backslash);
                    break;
                case '[': case '{':
                    PutChar((byte)VKCode.L_Mid_Bracket);
                    break;
                case ']': case '}':
                    PutChar((byte)VKCode.R_Mid_Bracket);
                    break;
                case '`': case '~':
                    PutChar((byte)VKCode.Backquote);
                    break;
                case '!':
                    PutChar((byte)VKCode.Numbers+1);
                    break;
                case '@':
                    PutChar((byte)VKCode.Numbers + 2);
                    break;
                case '#':
                    PutChar((byte)VKCode.Numbers + 3);
                    break;
                case '$':
                    PutChar((byte)VKCode.Numbers + 4);
                    break;
                case '%':
                    PutChar((byte)VKCode.Numbers + 5);
                    break;
                case '^':
                    PutChar((byte)VKCode.Numbers + 6);
                    break;
                case '&':
                    PutChar((byte)VKCode.Numbers + 7);
                    break;
                case '*':
                    PutChar((byte)VKCode.Numbers + 8);
                    break;
                case '(':
                    PutChar((byte)VKCode.Numbers + 9);
                    break;
                case ')':
                    PutChar((byte)VKCode.Numbers);
                    break;
                case '-': case '_':
                    PutChar((byte)VKCode.Minus);
                    break;
                case '=': case '+':
                    PutChar((byte)VKCode.Minus);
                    break;
                case ' ':
                    PutChar((byte)VKCode.Space);
                    break;
                default:
                    PutChar((byte)ascii);   //shift, enter等控制键，直接按照VKCode输出.
                    break;
            }
            if (needShift)
                ReleaseKey((byte)VKCode.Shift);
        }
    }
    
    void do_delete_char()
    {
        // 在inputField的当前位置删除一个字符.
        // 直接用输入一个backspace实现删除.
        PushKey((byte)VKCode.Back);
        ReleaseKey((byte)VKCode.Back);
    }

    void Seek(SEEK_MOD mode, int offset)
    {
        // 移动 inputField 的光标.
        // 永远对offset做加号，如果是移动到末尾，offset则应该是负数!
        if(mode == SEEK_MOD.Start)
        {
            inputField.caretPosition = offset;
        }
        else if(mode == SEEK_MOD.Current)
        {
            inputField.caretPosition += offset;
        }
        else
        {
            inputField.MoveTextEnd(false);  //直接移动到末尾.
            inputField.caretPosition += offset;
        }
    }

    void PushKey(byte bVK)
    {
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyDown, 0);
    }

    void ReleaseKey(byte bVK)
    {
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyUp, 0);
    }

    void PutChar(byte bVK)
    {
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyDown, 0);
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyUp, 0);
    }
}
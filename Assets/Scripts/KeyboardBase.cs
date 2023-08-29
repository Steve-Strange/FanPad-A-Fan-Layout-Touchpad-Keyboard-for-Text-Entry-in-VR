
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

/* 键盘脚本，应该直接挂载到键盘最上层! */
public class KeyboardBase : MonoBehaviour
{
    // Start is called before the first frame update
    [DllImport("User32.dll", EntryPoint = "keybd_event")]
    static extern void keybd_event(byte bVK, byte bScan, int dwFlags, int dwExtraInfo);

    public Statistics statistics;

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
    public WordCubes wordCubes;

    // 键盘上的文字.
    protected TextMeshProUGUI[,] keyStrings;
    protected bool selected = false, deleted = false, touched = false, longHolding = false;
    protected bool left_touched = false, right_touched = false;
    protected float last_delete_time, hold_time_start, last_caret_time;

    WordPrediction predictor = new WordPrediction();

    void Start()
    {
        inputField.ActivateInputField();
        keyStrings = fetchKeyStrings(); 
        WordCubes tmp = GameObject.Find("wordcubes").GetComponent<WordCubes>();
        if(tmp != null)
            wordCubes = tmp;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnEnable()
    {
        Debug.Log("Enable keyboard action set and relative callback functions!");
        SteamVR_Input_Sources[] tmp = new SteamVR_Input_Sources[] { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
        keyboardActionSet.Activate();
        foreach (var hand in tmp)
        {
            
            DeleteKey[hand].onStateDown += OnDeleteKeyDown;
            DeleteKey[hand].onStateUp += OnDeleteKeyUp;
            DeleteKey[hand].onState += OnDeleteKeyHolding;
            SelectKey[hand].onStateDown += OnSelectKeyDown;
            SelectKey[hand].onStateUp += OnSelectKeyUp;
            PadTouch[hand].onStateDown += OnTouchDown;
            PadTouch[hand].onStateUp += OnTouchUp;
            PadPress[hand].onStateUp += OnPressUp;
            PadPress[hand].onStateDown += OnPressDown;
            PadSlide[hand].onChange += OnPadSlide;
        }
    }

    private void OnDisable()
    {
        Debug.Log("Disable keyboard action set and relative callback functions!");
        SteamVR_Input_Sources[] tmp = new SteamVR_Input_Sources[] { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
        keyboardActionSet.Deactivate();
        foreach (var hand in tmp)
        {
            
            DeleteKey[hand].onStateDown -= OnDeleteKeyDown;
            DeleteKey[hand].onStateUp -= OnDeleteKeyUp;
            DeleteKey[hand].onState -= OnDeleteKeyHolding;
            SelectKey[hand].onStateDown -= OnSelectKeyDown;
            SelectKey[hand].onStateUp -= OnSelectKeyUp;
            PadTouch[hand].onStateDown -= OnTouchDown;
            PadTouch[hand].onStateUp -= OnTouchUp;
            PadPress[hand].onStateUp -= OnPressUp;
            PadPress[hand].onStateDown -= OnPressDown;
            PadSlide[hand].onChange -= OnPadSlide;
        }
    }

    // 键盘上的文字显示相关
    protected virtual TextMeshProUGUI[,] fetchKeyStrings()
    {
        // 最开始的时候获取 keyStrings.
        return new TextMeshProUGUI[1, 1];
    }

    protected void switchCapital()
    {
        bool upper;
        if (keyStrings[0, 0].text[0] >= 'a' && keyStrings[0, 0].text[0] <= 'z')  // 原本是小写，变大写.
            upper = false;
        else if (keyStrings[0, 0].text[0] >= 'A' && keyStrings[0, 0].text[0] <= 'Z')  //原本是大写，变小写.
            upper = true;
        else         // 当前middle不是字母，说明在符号键盘状态中，不切换大小写.
            return;
        // 切换字符大小写.
        int length = keyStrings.GetLength(1);
        for(int i=0; i<length; ++i)
        {
            print(keyStrings[0, i].text);
            if (upper)
                keyStrings[0, i].text = keyStrings[0, i].text.ToLower();
            else
                keyStrings[0, i].text = keyStrings[0, i].text.ToUpper();
        }
    }

    protected void switchSymbol()
    {
        // 符号键盘/普通键盘互换.，把keyStrings的第一二行互换.
        int length = keyStrings.GetLength(1);
        print(length);
        for(int i=0; i<length; ++i)
        {
            print(keyStrings[0, i]);
            print(keyStrings[1, i]);
            string tmp = keyStrings[0, i].text;
            keyStrings[0, i].text = keyStrings[1, i].text;
            keyStrings[1, i].text = tmp;
        }
    }

    // 移动光标和删除逻辑，这些在所有键盘中都是一样的.
    virtual public void OnSelectKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        selected = false;
    }

    virtual public void OnSelectKeyDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        selected = true;
        last_caret_time = Time.time;
    }

    virtual public void OnDeleteKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
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
        if (Time.time - last_delete_time > 0.15f)
        {
            last_delete_time = Time.time;
            do_delete_char();
        }
    }

    // 下面是触摸板相关的，每个键盘不太一样.必须在外面重载!
    virtual public void OnTouchDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if(fromSource == SteamVR_Input_Sources.RightHand)
            right_touched = true;
        if(fromSource == SteamVR_Input_Sources.LeftHand)
            left_touched = true;
        touched = true;
    }

    virtual public void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if(fromSource == SteamVR_Input_Sources.RightHand)
            right_touched = false;
        if(fromSource == SteamVR_Input_Sources.LeftHand)
            left_touched = false;
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
        // mode: 特殊状态，比如大写的状态，特殊符号的状态. 0- 小写， 1-大写， 2-符号键盘模式, 3-长按.
        // key: 要把当前所处的按键的GameObject(引用)赋值给key. 注意，如果mode==2，有特殊符号的选择框，则应该把相应的选择框内的三个按键之一赋值给key.
        // Axis2Letter要能处理长按后、弹出了三个选择框时候的特殊情况；这时候只看左右水平移动的分量. 相对左移，返回ascii为最左边那个按键的值；相对右移，返回最右边那个按键的值.
        key = this.gameObject;  //meaningless, 只是占位试图通过编译.
        return 0;
    }

    public void OutputLetter(int ascii)
    {
        int addChars = 1;   // 是否统计算入一个字符.
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
                    if (ascii == (int)VKCode.Back)
                        statistics.deleteTtimes++;
                    if(ascii != (int)VKCode.Enter)
                        addChars = 0;
                    break;
            }
            if (needShift)
                ReleaseKey((byte)VKCode.Shift);
        }
        predictor.next(ascii);        
        string[] strarr = predictor.getSuggestions();
        wordCubes.setWords(strarr);
        string tmp = string.Empty;
        foreach (string str in predictor.getSuggestions())
            tmp = tmp + str + ", ";
        Debug.Log(tmp);
        statistics.outputCchars += addChars;
    }

    public void OutputWord(string word)
    {
        // 选中了预测出来的某个单词，输出单词.
        int length = predictor.getCurLength();
        // 删除末尾的length个字符.
        inputField.text = inputField.text.Substring(0, inputField.text.Length - length);
        // 加上预测出来的单词.
        inputField.text += word;
        // 刷新统计数据.
        statistics.outputCchars = statistics.outputCchars - length + word.Length;
        // 刷新单词预测器.
        predictor.refresh();
    }
    
    protected void do_delete_char()
    {
        // 在inputField的当前位置删除一个字符.
        // 直接用输入一个backspace实现删除. 统一使用OutputLetter
        //statistics.deleteTtimes++;
        //PushKey((byte)VKCode.Back);
        //ReleaseKey((byte)VKCode.Back);
        OutputLetter((int)VKCode.Back);
    }

    protected void Seek(SEEK_MOD mode, int offset)
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

    protected void do_caret_move(Vector2 axis)
    {
        // 移动光标可以用这个.
        // 根据手指所在的位置，向上/下/左/右方向移动**1格**.
        Debug.Log("do_caret_move");
        float slope = axis.y / (axis.x + 1e-6f);  //防止除以0.
        if(-1 < slope && slope < 1)   //是左右移动.
            Seek(SEEK_MOD.Current, axis.x >= 0 ? 1 : -1);
        else
        {
            // 是上下移动. 先获取一行有多少字符，然后再移动一行的字符数.
            // 由于使用等宽字体，并且只有英文，不用考虑一行字数不同.
            var linesinfo = inputText.GetTextInfo(inputText.text);   //用这个方法获取文字文本信息!
            int chars_per_line = linesinfo.characterCount / linesinfo.lineCount;
            int plus1 = linesinfo.characterCount % linesinfo.lineCount == 0 ? 0 : 1;
            chars_per_line += plus1;    //由于每行字符数相等，恰好填满最后一行的时候平均是对的；没有填满最后一行的时候平均比真正的值少1.
            Seek(SEEK_MOD.Current, axis.y >= 0 ? -chars_per_line : chars_per_line);
        }
    }

    protected void PushKey(byte bVK)
    {
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyDown, 0);
    }

    protected void ReleaseKey(byte bVK)
    {
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyUp, 0);
    }

    protected void PutChar(byte bVK)
    {
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyDown, 0);
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyUp, 0);
    }
}
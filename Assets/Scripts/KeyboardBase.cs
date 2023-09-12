
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Valve.VR;
using Valve.VR.InteractionSystem;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

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

/* ���̽ű���Ӧ��ֱ�ӹ��ص��������ϲ�! */
public class KeyboardBase : MonoBehaviour
{
    // Start is called before the first frame update
    [DllImport("User32.dll", EntryPoint = "keybd_event")]
    static extern void keybd_event(byte bVK, byte bScan, int dwFlags, int dwExtraInfo);

    public float selectThreshold = 0.5f;  //���ְ��°����ѡ�񵥴ʣ��̰��������ƶ���꣨����������ֵ.

    public Experiment exp;

    //Keyboard action set
    public SteamVR_ActionSet keyboardActionSet;

    // fetch actions.
    public SteamVR_Action_Boolean DeleteKey = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "delete");//ɾ����-ɾ���ַ�
    public SteamVR_Action_Boolean SelectKey = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "select");//�����-�ƶ����
    public SteamVR_Action_Boolean PadTouch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "touch");  //touchpad����
    public SteamVR_Action_Boolean PadPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "press");  //touchpad����
    public SteamVR_Action_Vector2 PadSlide = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("keyboard", "slide");  //��touchpad�ϻ���

    public SteamVR_Action_Boolean Fitting = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "fitting");  //

    // text input field. 
    public TextMeshProUGUI inputText;
    public TMP_InputField inputField;
    public WordCubes wordCubes;

    // �����ϵ�����.
    protected TextMeshProUGUI[,] keyStrings;
    protected bool selected = false, deleted = false, touched = false, longHolding = false;
    protected bool left_touched = false, right_touched = false;
    protected float last_delete_time, hold_time_start, last_caret_time, select_down_time;

    // ����/��ֹ���.
    public bool enableOutput = true;

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
            Fitting[hand].onStateUp += OnFitting;
            
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
            Fitting[hand].onStateUp -= OnFitting;
        }
    }

    // �����ϵ�������ʾ���
    protected virtual TextMeshProUGUI[,] fetchKeyStrings()
    {
        // �ʼ��ʱ���ȡ keyStrings.
        return new TextMeshProUGUI[1, 1];
    }

    protected void switchCapital()
    {
        if(!enableOutput)
            return;
        bool upper;
        if (keyStrings[0, 0].text[0] >= 'a' && keyStrings[0, 0].text[0] <= 'z')  // ԭ����Сд�����д.
            upper = false;
        else if (keyStrings[0, 0].text[0] >= 'A' && keyStrings[0, 0].text[0] <= 'Z')  //ԭ���Ǵ�д����Сд.
            upper = true;
        else         // ��ǰmiddle������ĸ��˵���ڷ��ż���״̬�У����л���Сд.
            return;
        // �л��ַ���Сд.
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
        if(!enableOutput)
            return;
        // ���ż���/��ͨ���̻���.����keyStrings�ĵ�һ���л���.
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

    // �ƶ�����ɾ���߼�����Щ�����м����ж���һ����.
    virtual public void OnSelectKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if(enableOutput){
            if(Time.time - select_down_time < selectThreshold)
            {
                // �̰��������Ҫѡ�񵥴ʣ�
                string word = wordCubes.getSelectedWord();
                if(word != string.Empty)
                {
                    OutputWord(word);
                }
            }
        }
        selected = false;
    }

    virtual public void OnSelectKeyDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        selected = true;
        last_caret_time = Time.time;
        select_down_time = last_caret_time;
    }

    virtual public void OnDeleteKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* �ɿ�ɾ���� */
        deleted = false;
    }

    public void OnDeleteKeyDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* ����ɾ���� */
        deleted = true;
        last_delete_time = Time.time;
        do_delete_char();
    }

    public void OnDeleteKeyHolding(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* ����ɾ��������onState�Ļص���������ΪonState����Ҫ��true�Ŵ��������Բ����ж��Ƿ�true. */
        // ����ɾ��̫�죬�������0.2s��ɾ.
        if (Time.time - last_delete_time > 0.15f)
        {
            last_delete_time = Time.time;
            do_delete_char();
        }
    }

    // �����Ǵ�������صģ�ÿ�����̲�̫һ��.��������������!
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
        
        /* ��SlideKeyboard�У�Ҫ�������ʼ�жϳ���. */
    }

    virtual public void OnPressUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* ������ʲô���̣����ﶼ��Ҫ����ַ���! */
    }

    virtual public void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        /* 
         ��ָ�ڴ������ϻ�����һ������һ���Ĺ����ƶ��ı������һ���ڰ��°����ʱ���ƶ����.
         */

    }

    virtual public void OnFitting(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        // �����Զ����һЩ��������Ϊ��ͨ���̺���б���̲�ͬ������Ӧ�ò�ͬ.
        return;
    }

    // Core
    virtual public int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode, out GameObject key)
    {
        // Ӧ�ý�Axis2Char����..
        // ����ǰλ�ô������λ��ת��ΪҪ�����Ascii�ַ�. ���Ұѵ�ǰ�����ļ���λ�õİ�����GameObject
        // mode: ����״̬�������д��״̬��������ŵ�״̬. 0- Сд�� 1-��д�� 2-���ż���ģʽ, 3-����.
        // key: Ҫ�ѵ�ǰ�����İ�����GameObject(����)��ֵ��key. ע�⣬���mode==2����������ŵ�ѡ�����Ӧ�ð���Ӧ��ѡ����ڵ���������֮һ��ֵ��key.
        // Axis2LetterҪ�ܴ��������󡢵���������ѡ���ʱ��������������ʱ��ֻ������ˮƽ�ƶ��ķ���. ������ƣ�����asciiΪ������Ǹ�������ֵ��������ƣ��������ұ��Ǹ�������ֵ.
        key = this.gameObject;  //meaningless, ֻ��ռλ��ͼͨ������.
        return 0;
    }

    public void OutputLetter(int ascii)
    {
        if(!enableOutput)
            return;
        // ��Ascii���ŷ���Ϊ����.
        // �������İ취��Ӧ������һ��ascii->VKcode������!
        // ע�⣬Shift, Enter�ȿ��Ƽ���ʹ��ascii�������ֱ����VKCode
        // 1. ��ĸ
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
        // 2. ����
        else if('0' <= ascii && ascii <= '9')
        {
            PutChar((byte)(ascii - '0' + VKCode.Numbers));
        }
        // 3. ��������, ö��..
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
                    PutChar((byte)ascii);   //shift, enter�ȿ��Ƽ���ֱ�Ӱ���VKCode���.
                    break;
            }
            if (needShift)
                ReleaseKey((byte)VKCode.Shift);
        }
        // ͳ��.
        if(exp.onExperiment){
            exp.Next(ascii);
        }
        // ���ʾ���
        predictor.next(ascii);        
        string[] strarr = predictor.getSuggestions();
        // ��������ʾ��ʾ�����ʰ���.
        wordCubes.setWords(strarr);
        // ��Ԥ��ĵ������������̨.
        string tmp = string.Empty;
        foreach (string str in predictor.getSuggestions())
            tmp = tmp + str + ", ";
        Debug.Log(tmp);
    }

    public void OutputWord(string word)
    {
        if(!enableOutput)
            return;
        // ѡ����Ԥ�������ĳ�����ʣ��������.
        int length = predictor.getCurLength();
        // ��������ĸ��д���⣬��wordPrediction��ȡ�� curWord�ж�����ĸ�Ƿ��д.
        string curword = predictor.getCurWord();
        if('A' <= curword[0] && curword[0] <= 'Z'){
            word = word.Substring(0,1).ToUpper() + word.Remove(0, 1);
        }  // ����ĸ��д.
        // ɾ��ĩβ��length���ַ�.
        inputField.text = inputField.text.Substring(0, inputField.text.Length - length);
        // ����Ԥ������ĵ���.
        inputField.text += word;
        // ������ƶ��������.
        inputField.caretPosition += word.Length;
        // ˢ��ͳ������. ����ո�Ӧ���ڴ�֮��.
        if(exp.onExperiment){
            exp.Next(word, curword);
        }
        // ������ʺ����һ���ո�.
        OutputLetter(' ');
        // ˢ�µ���Ԥ����.
        predictor.refresh();
    }
    
    protected void do_delete_char()
    {
        // ��inputField�ĵ�ǰλ��ɾ��һ���ַ�.
        // ֱ��������һ��backspaceʵ��ɾ��. ͳһʹ��OutputLetter
        //statistics.deleteTtimes++;
        //PushKey((byte)VKCode.Back);
        //ReleaseKey((byte)VKCode.Back);
        OutputLetter((int)VKCode.Back);
    }

    protected void Seek(SEEK_MOD mode, int offset)
    {
        // �ƶ� inputField �Ĺ��.
        // ��Զ��offset���Ӻţ�������ƶ���ĩβ��offset��Ӧ���Ǹ���!
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
            inputField.MoveTextEnd(false);  //ֱ���ƶ���ĩβ.
            inputField.caretPosition += offset;
        }
    }

    protected void do_caret_move(Vector2 axis)
    {
        // �ƶ������������.
        // ������ָ���ڵ�λ�ã�����/��/��/�ҷ����ƶ�**1��**.
        Debug.Log("do_caret_move");
        float slope = axis.y / (axis.x + 1e-6f);  //��ֹ����0.
        if(-1 < slope && slope < 1)   //�������ƶ�.
            Seek(SEEK_MOD.Current, axis.x >= 0 ? 1 : -1);
        else
        {
            // �������ƶ�. �Ȼ�ȡһ���ж����ַ���Ȼ�����ƶ�һ�е��ַ���.
            // ����ʹ�õȿ����壬����ֻ��Ӣ�ģ����ÿ���һ��������ͬ.
            var linesinfo = inputText.GetTextInfo(inputText.text);   //�����������ȡ�����ı���Ϣ!
            int chars_per_line = linesinfo.characterCount / linesinfo.lineCount;
            int plus1 = linesinfo.characterCount % linesinfo.lineCount == 0 ? 0 : 1;
            chars_per_line += plus1;    //����ÿ���ַ�����ȣ�ǡ���������һ�е�ʱ��ƽ���ǶԵģ�û���������һ�е�ʱ��ƽ����������ֵ��1.
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.Animations;

public class Experiment : MonoBehaviour
{
    public bool onExperiment = true;
    public string username = string.Empty;
    public int keyboardType = 0;
    public int phrasesNumber = 5;
    public TextMeshProUGUI transcript;  //那块黑板上的字.
    public TMP_InputField inputField;   //用于获取输入的内容.
    public string LogDir = "Assets/ExpLog/";  //存储实验记录的文件夹路径.
    public SteamVR_Action_Boolean PadTouch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "touch");
    public SteamVR_Action_Boolean Over = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "over");
    public SteamVR_Action_Vector2 PadSlide = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("keyboard", "slide");
    public SteamVR_Action_Boolean Fitting = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "fitting");
    string phrases;  //所有句子合成一个，以回车为分割.
    public int index = 0;    //当前正要打哪个字母的下标.
    List<string> inputSequence = new List<string>();
    public int totalErr = 0;
    bool[] alphaerr;  //错过的位置只记一次错误

    Record record;

    float startTime, endTime, firstTypeTime = 0f, lastTypeTime = 0f;
    bool touched = false, start = false, endexp = false, fitting = false;

    FirstTouch firstTouch = new FirstTouch();
    public AudioSource endaudio;  // 结束音效
    // 记录最后一次滑动点所用.
    Vector2 lastSlideAxis = new Vector2(), lastSlideDelta = new Vector2();

    // Start is called before the first frame update
    void Start()
    {
        inputField.ActivateInputField();
        endaudio = GetComponent<AudioSource>();
        if(onExperiment){
            // 需要进行实验.
            record = new Record(username, keyboardType);
            // 获取用于实验的句子
            List<string> phs = PhraseProvider.GetPhrases(phrasesNumber);
            // 将句子数组存储到record中
            record.phrases = phs;
            // 将所有句子整合到一个字符串里，显示到黑板上 最后一个字母后面填上回车.
            phrases = string.Join("\n", phs.ToArray());  //最后不需要回车键作为结束符
            transcript.text = phrases;
            // 记录长度
            record.phraseLength = phrases.Length;
            // 初始化错误数组.
            alphaerr = new bool[phrases.Length];
            for(int i=0; i<phrases.Length; ++i)
                alphaerr[i] = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        inputField.ActivateInputField();
    }

    void OnEnable(){
        if(onExperiment){
            SteamVR_Input_Sources[] tmp = new SteamVR_Input_Sources[] { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
            foreach(var hand in tmp){
                PadTouch[hand].onStateDown += OnTouchDown;
                PadTouch[hand].onStateUp += OnTouchUp;
                PadSlide[hand].onChange += OnPadSlide;
                Over[hand].onStateDown += OnOverDown;
                if(keyboardType == 2 || keyboardType == 3){
                    Fitting[hand].onStateDown += OnFitting;
                }
            }
        }
    }

    void OnDisable(){
        if(onExperiment){
            SteamVR_Input_Sources[] tmp = new SteamVR_Input_Sources[] { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
            foreach(var hand in tmp){
                PadTouch[hand].onStateDown -= OnTouchDown;
                PadTouch[hand].onStateUp -= OnTouchUp;
                PadSlide[hand].onChange -= OnPadSlide;
                Over[hand].onStateDown -= OnOverDown;
                if(keyboardType == 2 || keyboardType == 3){
                    Fitting[hand].onStateDown += OnFitting;
                }
            }
        }
    }

    public void Next(int ascii){
        if(endexp)
            return;
        if(firstTypeTime < 1e-6 && firstTypeTime > -1e-6){
            firstTypeTime = Time.time;
        }
        lastTypeTime = Time.time;
        // 下面这一行和index相关的，判断错误的都无效.
        if(index >= phrases.Length){
            // 可能是不小心输入多了，正在删除.
            if(ascii == (int)VKCode.Back)
                index--;
            else{
                // 输多了还在输入，错!
                totalErr++;
                index++;
            }
        }
        // 用户输入了单个字母，更新状态.
        else if(ascii == (int)VKCode.Back){
            // 用户输入了退格键.
            if(index > 0){
                // 退格键有效.
                --index;  //退一格，但不计入错误数.
            }
            else if(!alphaerr[index]){
                // 在最开始按下了退格，算一次错误.
                ++totalErr;
                alphaerr[index] = true;
            }
        }
        else if((ascii != (int)VKCode.Shift && ascii != (int)VKCode.Switch) && 
                !(ascii == ' ' && phrases[index] == (int)VKCode.Enter)){
            // 用户输入了有用的占位字符, 字母/数字/符号/回车
            // 如果用户输入的是空格而这个位置需要回车，就当作空格不存在，忽略，不增加totalErr也不增加index.
            if(ascii != phrases[index] && !alphaerr[index]){
                alphaerr[index] = true;
                ++totalErr;
            }
            ++index;
        }
        // 添加inputSequence.
        string seqitem;
        if(ascii == (int)VKCode.Back)
            seqitem = "Back";
        else if(ascii == (int)VKCode.Enter)
            seqitem = "Enter";
        else if(ascii == (int)VKCode.Shift)
            seqitem = "Shift";
        else if(ascii == (int)VKCode.Switch)
            seqitem = "Sym";
        else
            seqitem = ((char)ascii).ToString();
        inputSequence.Add(seqitem);

        // 添加firstTouch.
        if(firstTouch.key != string.Empty){
            // 有效.之前有按下过touchpad
            Vector2 upAxis = lastSlideAxis - lastSlideDelta;
            firstTouch.key = seqitem;
            firstTouch.x_up = upAxis.x; firstTouch.y_up = upAxis.y;  // 设置抬手位置.
            firstTouch.release_time = Time.time;
            record.firstTouches.Add(firstTouch);
            firstTouch = new FirstTouch();  //刷新掉
        }
        else if(seqitem == "Back"){
            // 有输入，但是之前没有按下触摸板，说明是用了手柄上按键的delete.
            record.firstTouches.Add(new FirstTouch("Back", -1, 100, 100, 100, 100, 0, 0)); // 占位.
        }
    }   

    public void Next(string word, string replaced){
        // word: 整个输入的单词，注意这里没有空格，但是实际上会输入这个空格. replaced 为被替换掉的字符串.
        if(endexp)
            return;
        // 一定已经有单个输入，不会在这里更新firstTypeTime
        lastTypeTime = Time.time;
        inputSequence.Add("-"+replaced+", +"+word);  //"-XXX, +XXX" 表示单词纠正.
        index -= replaced.Length;
        for(int i=0; i<word.Length; ++i){
            if(index+i >= phrases.Length){
                totalErr += word.Length - i;
                break;
            }
            if(word[i]!=phrases[index+i] && !alphaerr[index+i]){
                alphaerr[index+i] = true;
                ++totalErr;
            }
        }
        index += word.Length;
        // 所以不可能结束在这里！
    }

    public void OnOverDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource){
        // 结束实验流程.
        if(!endexp)
            EndExp();
    }

    public void OnTouchDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource){
        // 记录开始时刻.
        if(!start){
            start = true;
            startTime = Time.time;
        }
    }
    // 松手标志
    public void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource){
        touched = false;
    }
    // 记录firsttouch! 

    public void OnFitting(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource){
        fitting = !fitting;
    }
    public void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        lastSlideAxis = axis;
        lastSlideDelta = delta;
        // delta太大的不要.这个阈值是跟着clickboard来的.
        if(Mathf.Sqrt(delta.x*delta.x + delta.y * delta.y) > 0.2){
            return;
        }
        // 希望记忆抬手点，所以应该仿照clickkeyboard, 记忆lastSlidePoint 和 lastSlideDelta. 并且舍弃delta向量长度大于0.2的滑动输入.
        if(!fitting && !touched){
            touched = true;
            int lr = fromSource == SteamVR_Input_Sources.LeftHand ? 0 : 1;
            //record.firstTouches.Add(new FirstTouch(phrases[index].ToString(), lr, axis));
            firstTouch = new FirstTouch("spaceholder", lr, axis, axis, Time.time, Time.time);
        }
    }

    public void setThetaR(float theta, float r){
        record.theta = theta;
        record.r = r;
    }

    void FinalCalculate(){
        // 做最后的结算工作, 处理Record中的记录.
        record.seconds = endTime - firstTypeTime;  //Time.time直接是秒.
        record.inputSequence = inputSequence;
        record.result = inputField.text;
        record.WPM = record.phraseLength / (record.seconds / 60) / 5;
        record.totalErr = totalErr;
        record.TER = (float)totalErr / record.phraseLength;
        // 比较字符串，计算未修正错误率.
        record.ncErr = 0;
        for(int i=0; i<phrases.Length && i<record.result.Length; ++i){
            if(phrases[i] != record.result[i])
                ++record.ncErr;
        }
        if(phrases.Length != record.result.Length){
            record.ncErr += Mathf.Abs(phrases.Length - record.result.Length);
        }
        record.NCER = (float)record.ncErr / record.phraseLength;
    }
    void SaveRecord(){
        // 保存试验记录
        string json = JsonConvert.SerializeObject(record, Formatting.Indented);
        string path = LogDir + username + "_" + keyboardType.ToString() + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
        System.IO.File.WriteAllText(path, json);
    }

    void EndExp(){
        endexp = true;
        endTime = lastTypeTime;
        FinalCalculate();
        SaveRecord();
        // 提示音
        endaudio.Play();
    }
}

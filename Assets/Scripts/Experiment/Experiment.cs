using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using Valve.VR;
using Valve.VR.InteractionSystem;

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
    public SteamVR_Action_Vector2 PadSlide = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("keyboard", "slide");
    string phrases;  //所有句子合成一个，以回车为分割.
    int index = 0;    //当前正要打哪个字母的下标.
    List<string> inputSequence = new List<string>();
    int totalErr = 0;
    bool[] alphaerr;  //错过的位置只记一次错误

    Record record;

    float startTime, endTime;
    bool touched = false, start = false, endexp = false;

    // Start is called before the first frame update
    void Start()
    {
        if(onExperiment){
            // 需要进行实验.
            record = new Record(name, keyboardType);
            // 获取用于实验的句子
            List<string> phs = PhraseProvider.GetPhrases(phrasesNumber);
            // 将句子数组存储到record中
            record.phrases = phs;
            // 将所有句子整合到一个字符串里，显示到黑板上 最后一个字母后面填上回车.
            phrases = string.Join("\n", phs.ToArray()) + "\n";  //最后多加一个回车键，表示结束！注意多加了回车!
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
        
    }

        void OnEnable(){
        if(onExperiment){
            SteamVR_Input_Sources[] tmp = new SteamVR_Input_Sources[] { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
            foreach(var hand in tmp){
                PadTouch[hand].onStateUp += OnTouchUp;
                PadSlide[hand].onChange += OnPadSlide;
            }
        }
    }

    void OnDisable(){
        if(onExperiment){
            SteamVR_Input_Sources[] tmp = new SteamVR_Input_Sources[] { SteamVR_Input_Sources.LeftHand, SteamVR_Input_Sources.RightHand };
            foreach(var hand in tmp){
                PadTouch[hand].onStateUp -= OnTouchUp;
                PadSlide[hand].onChange -= OnPadSlide;
            }
        }
    }

    public void Next(int ascii){
        if(endexp)
            return;
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
        else if(ascii != (int)VKCode.Shift && ascii != (int)VKCode.Switch){
            // 用户输入了有用的占位字符, 字母/数字/符号/回车
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
        // 判断是否结束  
        if(ascii == (int)VKCode.Enter && index == phrases.Length)
            EndExp();
    }   

    public void Next(string word, string replaced){
        // word: 整个输入的单词，注意这里没有空格，但是实际上会输入这个空格. replaced 为被替换掉的字符串.
        if(endexp)
            return;
        word += " ";
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
        // 因为是以回车为结束，所以不可能结束在这里！
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
    public void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        if(!touched && index < phrases.Length){
            touched = true;
            int lr = fromSource == SteamVR_Input_Sources.LeftHand ? 0 : 1;
            record.firstTouches.Add(new FirstTouch(phrases[index].ToString(), lr, axis));
        }
    }

    void FinalCalculate(){
        // 做最后的结算工作, 处理Record中的记录.
        record.seconds = endTime - startTime;  //Time.time直接是秒.
        record.inputSequence = inputSequence;
        record.result = inputField.text;
        record.totalErr = totalErr;
        record.TER = (float)totalErr / record.phraseLength;
        // 比较字符串，计算未修正错误率.
        record.ncErr = 0;
        for(int i=0; i<phrases.Length; ++i){
            if(phrases[i] != inputField.text[i])
                ++record.ncErr;
        }
        record.NCER = (float)record.ncErr / record.phraseLength;
    }
    void SaveRecord(){
        // 保存试验记录
        string json = JsonConvert.SerializeObject(record);
        string path = LogDir + username + keyboardType.ToString() + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
        System.IO.File.WriteAllText(path, json);
    }

    void EndExp(){
        endexp = true;
        FinalCalculate();
        SaveRecord();
        // 提示音
    }
}

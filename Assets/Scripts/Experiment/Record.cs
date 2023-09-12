using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

// 记录实验结果用. 最后存下来的句子中，每个句子原句末尾没有回车，但打出来的末尾有回车.
public class Record
{
    public string name {get; set;}
    public int keyboardType {get; set;}  //0-normal, 1-normal+crossover, 2-fanpad, 3-fanpad+crossover.
    public float seconds {get; set;}
    public int phraseLength {get; set;}
    public List<string> phrases {get; set;}  //要输入的句子.
    public string result{get; set;}       //输入结果.
    public int totalErr {get; set;}   //总错误数量
    public int ncErr {get; set;}      //未修正错误数量.
    public float WPM {get; set;}
    public float TER {get; set;} //总错误率.
    public float NCER {get; set;} //未修正错误率. 
    public float theta {get; set;}
    public float r {get; set;}
    public List<FirstTouch> firstTouches {get; set;}
    public List<string> inputSequence {get; set;}  //完整的输入序列.
    public Record(string name, int keyboardType){
        this.name = name;
        this.keyboardType = keyboardType;
        this.seconds = 0;
        this.phraseLength = 0;
        this.phrases = new List<string>();
        this.inputSequence = new List<string>();
        this.result = "";
        this.totalErr = 0;
        this.ncErr = 0;
        this.WPM = 0;
        this.TER = 0;
        this.NCER = 0;
        this.theta = 0.3f;
        this.r = 6;
        this.firstTouches = new List<FirstTouch>();
    }

    public Record(
        string name, int keyboardType, float seconds, int phraseLength, List<string> phrases, List<string> inputSequence, 
        string result, int totalErr, int ncErr, float WPM, float TER, float NCER, float theta, float r, List<FirstTouch> firstTouches){
        this.name = name;
        this.keyboardType = keyboardType;
        this.seconds = seconds;
        this.phraseLength = phraseLength;
        this.phrases = phrases;
        this.inputSequence = inputSequence;
        this.result = result;
        this.totalErr = totalErr;
        this.ncErr = ncErr;
        this.WPM = WPM;
        this.TER = TER;
        this.NCER = NCER;
        this.theta = theta;
        this.r = r;
        this.firstTouches = firstTouches;
    }
}

public class FirstTouch
{
    public string key {get; set;}  //要按哪个按键.
    public int lr {get; set;} //0-left, 1-right
    public float x {get; set;}
    public float y {get; set;}
    // 加上结束时候的x,y.
    public float x_up {get; set;}
    public float y_up {get; set;}

    public FirstTouch(){
        this.key = string.Empty;
        this.lr = 0;
        this.x = 0;
        this.y = 0;
        this.x_up = 0;
        this.y_up = 0;
    }

    public FirstTouch(string key, int lr, float x, float y, float xup, float yup){
        this.key = key;
        this.lr = lr;
        this.x = x;
        this.y = y;
        this.x_up = xup;
        this.y_up = yup;
    }

    public FirstTouch(string key, int lr, Vector2 point_down, Vector2 point_up){
        this.key = key;
        this.lr = lr;
        this.x = point_down.x;
        this.y = point_down.y;
        this.x_up = point_up.x;
        this.y_up = point_up.y;
    }
}
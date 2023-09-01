using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 记录实验结果用.
public class Record
{
    public string name {get; set;}
    public int keyboardType {get; set;}  //0-normal, 1-normal+crossover, 2-fanpad, 3-fanpad+crossover.
    public float seconds {get; set;}
    public int phraseLength {get; set;}
    public List<string> phrases {get; set;}  //要输入的句子.
    public List<string> inputSequence {get; set;}  //完整的输入序列.
    public string result{get; set;}       //输入结果.
    public int totalErr {get; set;}   //总错误数量
    public int ncErr {get; set;}      //未修正错误数量.
    public float TER {get; set;} //总错误率.
    public float NCER {get; set;} //未修正错误率. 
    public List<FirstTouch> firstTouches {get; set;}

    public Record(string name, int keyboardType){
        this.name = name;
        this.keyboardType = keyboardType;
    }

    public Record(string name, int keyboardType, float seconds, int phraseLength, List<string> phrases, List<string> inputSequence, string result, int totalErr, int ncErr, float TER, float NCER, List<FirstTouch> firstTouches){
        this.name = name;
        this.keyboardType = keyboardType;
        this.seconds = seconds;
        this.phraseLength = phraseLength;
        this.phrases = phrases;
        this.inputSequence = inputSequence;
        this.result = result;
        this.totalErr = totalErr;
        this.ncErr = ncErr;
        this.TER = TER;
        this.NCER = NCER;
        this.firstTouches = firstTouches;
    }
}

public class FirstTouch
{
    public string key {get; set;}  //要按哪个按键.
    public int lr {get; set;} //0-left, 1-right
    public float x {get; set;}
    public float y {get; set;}

    public FirstTouch(string key, int lr, float x, float y){
        this.key = key;
        this.lr = lr;
        this.x = x;
        this.y = y;
    }

    public FirstTouch(string key, int lr, Vector2 point){
        this.key = key;
        this.lr = lr;
        this.x = point.x;
        this.y = point.y;
    }
}
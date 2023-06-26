using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using TMPro;

/* 
 * 原始键盘、倾斜键盘两个的基类。这两个键盘的逻辑几乎完全一样，只有映射方式不同.
 * 这个脚本应该实现ClickKeyboard的所有逻辑，只留下Axis2Letter让两个子类重写!
 */
public class ClickKeyboard : KeyboardBase
{
    public Transform symbolBox;

    protected Transform keyboardRoot;          // 方案1, 2 ClickKeyboard的根游戏物体.

    GameObject hoveringKey, checkKey = null;   // hoveringKey是当前正处于的按键；checkKey是用来判断长按的
    Color oldColor, hoveringColor = new Color(255, 255, 0, 60);
    int _mode = 0;   //输出模式状态，0-小写，1-大写(按了一次Shift), 2-特殊字符(切换)
    bool isCapitalDisplay = false;   // 是大写展示的键盘.
    Vector2 longHoldingAxis; // Nullable

    // Update is called once per frame
    void Update()
    {
        if (touched)
        {
            if (checkKey == null)
            {
                checkKey = hoveringKey;
                return;
            }
            if (!longHolding)  //还没有长按.
            {
                if (!canBeLongHeld(hoveringKey))
                {
                    checkKey = hoveringKey;
                    return;
                }
                if (checkKey != hoveringKey)
                {
                    // hoveringKey改变了.重新计时.
                    hold_time_start = Time.time;
                    checkKey = hoveringKey;
                }
                else if(Time.time - hold_time_start > 0.5)
                {
                    // 大于1s, 打开特殊符号框.
                    longHolding = true;
                    
                    // TODO: 给symbolBox的位置赋值.
                    symbolBox.position = hoveringKey.transform.position;  //这没写完，要根据最后键盘在场景里怎么放来修改.
                    symbolBox.gameObject.SetActive(true);

                    // 把hoveringKey的三个按键的字符串赋值了.
                    string[] str = new string[3];
                    int i = 0;
                    foreach(var text in hoveringKey.transform.GetChild(0).GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if ((text.text[0] >= 'a' && text.text[1] <= 'z') || (text.text[0] >= 'A' && text.text[0] <= 'Z'))
                        {
                            str[0] = text.text.ToUpper();
                            str[2] = text.text.ToLower();
                        }
                        else
                            str[1] = text.text;
                    }
                    foreach(var text in symbolBox.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        // 左 中 右 大写-符号-小写.
                        text.text = str[i++];
                    }

                    // 变色.
                    hoveringKey.GetComponent<MeshRenderer>().material.color = oldColor;
                    hoveringKey = symbolBox.Find("Rectangle002").gameObject;
                    Material mat = hoveringKey.GetComponent<MeshRenderer>().material;
                    oldColor = mat.color;
                    mat.color = hoveringColor;  //变色.
                }
            }
        }
    }

    protected override TextMeshProUGUI[,] fetchKeyStrings()
    {
        TextMeshProUGUI[,] keychar = new TextMeshProUGUI[2, 26];
        int i = 0;
        foreach (var key in keyboardRoot.GetComponentsInChildren<Transform>())
        {
            Transform canvas = key.GetChild(0);    // 按键下只有一个直接儿子是Canvas.
            if(canvas.childCount == 2)
            {
                // 有两个儿子，确定是有上下的.
                foreach (var text in canvas.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    // 初始的字母一定是小写的.
                    if (text.text[0] >= 'a' && text.text[0] <= 'z')
                        keychar[0, i] = text;   //是字母，中间那个text.
                    else
                        keychar[1, i] = text;
                    ++i;
                }
            }
        }
        return keychar;
    }

    // OnTouchDown
    public override void OnTouchDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        base.OnTouchDown(fromAction, fromSource);  //touched = true.
        // 需要记录长按!.
        hold_time_start = Time.time;
        // 最开始记录当前在哪个按键上.
        Axis2Letter(PadSlide[fromSource].axis, fromSource, _mode, out hoveringKey);
        Material material = hoveringKey.GetComponent<MeshRenderer>().material;
        oldColor = material.color;
        material.color = hoveringColor;  //改变当前所处键的颜色!
    }

    // OnTouchUp, 松开触摸板，这是要输出了!
    public override void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        base.OnTouchUp(fromAction, fromSource);  // touched = false.
        if (selected || deleted)  //如果正在移动光标或者删除字符，就当作无效!
            return;
        GameObject tmp;
        int ascii = longHolding ? longHoldingLogic(new Vector2(1,1)) : Axis2Letter(PadSlide[fromSource].axis, fromSource, _mode, out tmp);
        // 特殊控制键：目前只有VKCode.Shift.
        if(ascii == (int)VKCode.Shift)  //Shift, mode从0变1、从1变0. 按到控制键的时候_mode不应该能变为2.
        {
            _mode = _mode == 1 ? 0 : 1;
            isCapitalDisplay = !isCapitalDisplay;
            switchCapital();
        }
        else if(ascii == (int)VKCode.Switch)   //切换为符号键盘；或者从符号键盘切换回普通键盘.
        {
            _mode = _mode == 2 ? (isCapitalDisplay ? 1 : 0) : 2;
            switchSymbol();
        }
        // 不是特殊控制键，确实要输出字符.
        else
        {
            OutputLetter(ascii);
            if(longHolding)
            {
                symbolBox.gameObject.SetActive(false); // 特殊符号框
                longHolding = false;
            }
        }
        checkKey = null;   //checkKey置空，为下次打字做准备.
    }

    // ClickKeyboard中的按下触摸板没有特别的意义，就像父类那样空着吧. PressUp一定会有TouchUp，不必再调用一遍.

    // Core: OnPadSlide.
    public override void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        // 父类中这个函数是空的，不管.
        if (deleted)  // 正在删除，不管!
            return;
        if (selected)
        {
            //按了扳机，在移动光标，只处理光标，不管其它.
            do_caret_move(axis);
        }
        else
        {
            if (longHolding)
            {
                // TODO 已经长按了.
                longHoldingLogic(delta);
            }
            // 如果要用小球指示的话，也在这里改小球的位置
            // 正常输出行为，正在键盘上移动.
            else
            {
                GameObject oldkey = hoveringKey;
                Axis2Letter(axis, fromSource, _mode, out hoveringKey);
                if (oldkey != hoveringKey)
                {
                    // 变色.
                    oldkey.GetComponent<MeshRenderer>().material.color = oldColor;
                    Material mat = hoveringKey.GetComponent<MeshRenderer>().material;
                    oldColor = mat.color;
                    mat.color = hoveringColor;
                }
            }
        }
    }

    bool canBeLongHeld(GameObject key)
    {
        // 用于判断key是否有特殊符号，可以长按.
        return key.transform.GetChild(0).transform.childCount == 2;
    }

    int longHoldingLogic(Vector2 delta)
    {
        //(0,0), 初始化， （1，1），结算.
        if (delta.x == 0 && delta.y == 0)
        {
            longHoldingAxis.x = longHoldingAxis.y = 0;  //初始化.
            return 0;
        }
        if(delta.x != 1 || delta.y != 1)
            longHoldingAxis += delta;
        if (longHoldingAxis.x >= 0.03)   //0.03 magic number 调参.
            return symbolBox.GetChild(2).GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text[0];
        else if(longHoldingAxis.x <= -0.03)
            return symbolBox.GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text[0];
        return symbolBox.GetChild(1).GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text[0];
    }
}

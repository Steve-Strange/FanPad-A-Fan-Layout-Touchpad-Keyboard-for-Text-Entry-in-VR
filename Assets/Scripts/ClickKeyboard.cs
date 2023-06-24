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

    GameObject hoveringKey, checkKey = null;   // hoveringKey是当前正处于的按键；checkKey是用来判断长按的
    Color oldColor, hoveringColor = new Color(255, 255, 0, 60);
    int _mode = 0;   //输出模式状态，0-小写，1-大写(按了一次Shift), 2-特殊字符(长按)
    bool isCapitalDisplay = false;   // 是大写展示的键盘.

    // Start is called before the first frame update
    void Start()
    {
        
    }

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
            if (_mode != 2)  //还没有长按.
            {
                if (checkKey != hoveringKey)
                {
                    // hoveringKey改变了.重新计时.
                    hold_time_start = Time.time;
                    checkKey = hoveringKey;
                }
                else if(Time.time - hold_time_start > 1)
                {
                    // 大于1s, 打开特殊符号框.
                    _mode = 2;
                    // TODO: 给symbolBox的位置赋值.
                    symbolBox.position = hoveringKey.transform.position;  //这没写完，要根据最后键盘在场景里怎么放来修改.
                    symbolBox.gameObject.SetActive(true);
                    hoveringKey.GetComponent<MeshRenderer>().material.color = oldColor;
                    hoveringKey = symbolBox.Find("Rectangle002").gameObject;
                    Material mat = hoveringKey.GetComponent<MeshRenderer>().material;
                    oldColor = mat.color;
                    mat.color = hoveringColor;  //变色.
                }
            }
        }
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
        int ascii = Axis2Letter(PadSlide[fromSource].axis, fromSource, _mode, out tmp);
        // 特殊控制键：目前只有VKCode.Shift.
        if(ascii == (int)VKCode.Shift)  //Shift, mode从0变1、从1变0. 按到控制键的时候_mode不应该能变为2.
        {
            _mode = _mode == 1 ? 0 : 1;
            isCapitalDisplay = !isCapitalDisplay;
            foreach(var key in gameObject.GetComponentsInChildren<TextMeshProUGUI>())
            {
                // TODO: 将所有字母键的大小写转换. 这里的TextMeshProUGUI只是一个可能的实现，如果最后用的是纹理的话，那还应该用其他的.
                // 根据新的isCapitalDisplay改大小写.
            }
        }
        // 其他特殊控制键...(如有)
        // 不是特殊控制键，确实要输出字符.
        else
        {
            OutputLetter(ascii);
            _mode = _mode == 2 ? (isCapitalDisplay ? 1 : 0) : _mode;  //如果是2，则回到原先的状态.
            if(_mode == 2)
            {
                _mode = isCapitalDisplay ? 1 : 0;
                symbolBox.gameObject.SetActive(false); // 特殊符号框
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
            // 如果要用小球指示的话，也在这里改小球的位置
            // 正常输出行为，正在键盘上移动.
            GameObject oldkey = hoveringKey;
            Axis2Letter(axis, fromSource, _mode, out hoveringKey);
            if(oldkey != hoveringKey)
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

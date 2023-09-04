using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using TMPro;

/* 
 * ԭʼ���̡���б���������Ļ��ࡣ���������̵��߼�������ȫһ����ֻ��ӳ�䷽ʽ��ͬ.
 * ����ű�Ӧ��ʵ��ClickKeyboard�������߼���ֻ����Axis2Letter������������д!
 */
public class ClickKeyboard : KeyboardBase
{
    public Transform symbolBox;

    public Transform keyboardRoot;          // ����1, 2 ClickKeyboard�ĸ���Ϸ����.
    public float longHoldingTime = 1.5f;

    GameObject hoveringKey, checkKey = null;   // hoveringKey�ǵ�ǰ�����ڵİ�����checkKey�������жϳ�����
    Color oldColor, hoveringColor = new Color(255, 255, 0, 30);  //TODO: 调整颜色.
    int _mode = 0;   //���ģʽ״̬��0-Сд��1-��д(����һ��Shift), 2-�����ַ�(�л�)
    bool isCapitalDisplay = false;   // �Ǵ�дչʾ�ļ���.
    Vector2 longHoldingAxis;

    Vector2 lastSlideAxis, lastSlideDelta;

    SteamVR_Input_Sources mutex;   // exclude left hand and right hand.

    // Update is called once per frame
    void Update()
    {
        if (!selected && !deleted && touched)
        {
            if (checkKey == null)
            {
                checkKey = hoveringKey;
                return;
            }
            if (!longHolding)  //��û�г���.
            {
                if (!canBeLongHeld(hoveringKey))
                {
                    checkKey = hoveringKey;
                    return;
                }
                if (checkKey != hoveringKey)
                {
                    // hoveringKey�ı���.���¼�ʱ.
                    hold_time_start = Time.time;
                    checkKey = hoveringKey;
                }
                else if(Time.time - hold_time_start > longHoldingTime)  // 多久算长按, 调参
                {
                    // ����1s, ��������ſ�.
                    longHolding = true;

                    // TODO: ��symbolBox��λ�ø�ֵ.
                    symbolBox.transform.position = hoveringKey.transform.position;
                    symbolBox.transform.Translate(new Vector3(0, 0, 0.1f), Space.Self);
                    symbolBox.gameObject.SetActive(true);
                    longHoldingLogic(new Vector2(0, 0), ref hoveringKey);  // hoveringKey would not be changed when giving (0,0)

                    // ��hoveringKey�������������ַ�����ֵ��.
                    char[] c = new char[2];
                    c[0] = hoveringKey.name[0];
                    c[1] = hoveringKey.name[1] == '\\' ? '/' : hoveringKey.name[1];
                    symbolBox.GetComponent<PrepareLongholding>().init_chars(c[0], c[1]);

                    // ��ɫ.
                    hoveringKey.GetComponent<MeshRenderer>().material.color = oldColor;
                    hoveringKey = symbolBox.Find("Rectangle002").gameObject;
                    Material mat = hoveringKey.GetComponent<MeshRenderer>().material;
                    oldColor = mat.color;
                    mat.color = hoveringColor;  //��ɫ.
                }
            }
        }
    }

    protected override TextMeshProUGUI[,] fetchKeyStrings()
    {
        TextMeshProUGUI[,] keychar = new TextMeshProUGUI[2, 26];
        int i = 0;
        foreach (var key in keyboardRoot.GetComponentsInChildren<MeshRenderer>())
        {
            Transform canvas = key.transform.GetChild(0);    // ������ֻ��һ��ֱ�Ӷ�����Canvas.
            if(canvas.childCount == 2)
            {
                // ���������ӣ�ȷ���������µ�.
                foreach (var text in canvas.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    // ��ʼ����ĸһ����Сд��.
                    if (text.text[0] >= 'a' && text.text[0] <= 'z')
                    {
                        keychar[0, i] = text;   //����ĸ���м��Ǹ�text.
                    }
                    else
                        keychar[1, i] = text;
                }
                ++i;
            }
        }
        return keychar;
    }

    // OnTouchDown
    public override void OnTouchDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("Touch down!");
        print(fromSource);
        if (mutex != SteamVR_Input_Sources.Any)
            return;    // conduct TouchDown only when mutex is free.
        if (selected)
        {
            if(Time.time - select_down_time > selectThreshold)  //只有在按下扳机事件超过阈值，在移动光标状态中才需要更新last_caret_time.
                last_caret_time = Time.time;
            return;
        }
        if (deleted)
            return;
        mutex = fromSource;   // seize the mutex "lock".
        base.OnTouchDown(fromAction, fromSource);  //touched = true.
        // ��Ҫ��¼����!.
        hold_time_start = Time.time;
        // �ʼ��¼��ǰ���ĸ�������.
        Axis2Letter(PadSlide[fromSource].axis, fromSource, _mode, out hoveringKey);
        //Debug.LogWarning("TouchDown - hoveringKey: " + hoveringKey.name);
        Material material = hoveringKey.GetComponent<MeshRenderer>().material;
        oldColor = material.color;
        material.color = hoveringColor;  //�ı䵱ǰ����������ɫ!
    }

    // OnTouchUp, �ɿ������壬����Ҫ�����!
    public override void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (selected && Time.time - select_down_time > selectThreshold)
        {
            // 超过阈值了，在移动光标状态！
            do_caret_move(lastSlideAxis - lastSlideDelta);
            return;
        }
        if (deleted || !touched)  //��������ƶ�������ɾ���ַ����͵�����Ч!
            return;
        if (mutex != fromSource)  // only when the mutex was seized by the fromSource's "touchDown" should OnTouchUp be conducted.
            return;
        base.OnTouchUp(fromAction, fromSource);  // touched = false.
        GameObject tmp = hoveringKey;
        int ascii = longHolding ? longHoldingLogic(new Vector2(1,1), ref tmp) : Axis2Letter(lastSlideAxis - lastSlideDelta, fromSource, _mode, out tmp);
        hoveringKey.GetComponent<MeshRenderer>().material.color = oldColor;
        // ������Ƽ���Ŀǰֻ��VKCode.Shift.
        if(ascii == (int)VKCode.Shift)  //Shift, mode��0��1����1��0. �������Ƽ���ʱ��_mode��Ӧ���ܱ�Ϊ2.
        {
            if(_mode != 2)
            {
                _mode = _mode == 1 ? 0 : 1;
                isCapitalDisplay = !isCapitalDisplay;
                switchCapital();
            }

        }
        else if(ascii == (int)VKCode.Switch)   //�л�Ϊ���ż��̣����ߴӷ��ż����л�����ͨ����.
        {
            _mode = _mode == 2 ? (isCapitalDisplay ? 1 : 0) : 2;
            switchSymbol();
        }
        // ����������Ƽ���ȷʵҪ����ַ�.
        else
        {
            OutputLetter(ascii);
            if(longHolding)
            {
                symbolBox.gameObject.SetActive(false); // ������ſ�
                longHolding = false;
            }
        }
        checkKey = null;   //checkKey�ÿգ�Ϊ�´δ�����׼��.
        mutex = SteamVR_Input_Sources.Any;
    }

    // ClickKeyboard�еİ��´�����û���ر�����壬�������������Ű�. PressUpһ������TouchUp�������ٵ���һ��.

    // Core: OnPadSlide.
    public override void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        lastSlideAxis = axis;
        lastSlideDelta = delta;
        if (Mathf.Sqrt(delta.x * delta.x + delta.y * delta.y) > 0.2)
            return;
        // ��������������ǿյģ�����.
        if (deleted)  // ����ɾ��������!
            return;
        if (selected && Time.time - last_caret_time > 0.2f)
        {
            // require an interval time!
            do_caret_move(axis);
            last_caret_time = Time.time;
            return;
        }
        
        if (mutex != fromSource)  //only when the mutex was seized by the fromSource's "touchDown" should OnTouchUp be conducted.
            return;    
        
        
        if(touched)
        {
            // ���Ҫ��С��ָʾ�Ļ���Ҳ�������С���λ��
            // ���������Ϊ�����ڼ������ƶ�.
            GameObject oldkey = hoveringKey;
            int useless = longHolding ? longHoldingLogic(delta,ref hoveringKey) : Axis2Letter(axis, fromSource, _mode, out hoveringKey);  //useless无用.
            //Debug.LogWarning("OnPadSlide - delta = " + delta + " hoveringKey: " + hoveringKey.name);
            if (hoveringKey != null && oldkey != hoveringKey)  //这个hoveringKey很可能是null.
            {
                // ��ɫ.
                oldkey.GetComponent<MeshRenderer>().material.color = oldColor;
                Material mat = hoveringKey.GetComponent<MeshRenderer>().material;
                oldColor = mat.color;
                mat.color = hoveringColor;
            }
            
        }
    }

    public bool canBeLongHeld(GameObject key)
    {
        // �����ж�key�Ƿ���������ţ����Գ���.
        return key.transform.GetChild(0).transform.childCount == 2;
    }

    public int longHoldingLogic(Vector2 delta, ref GameObject key)
    {
        //(0,0), ��ʼ���� ��1��1��������.
        if (delta.x == 0 && delta.y == 0)
        {
            longHoldingAxis.x = longHoldingAxis.y = 0;  //��ʼ��.
            return 0;
        }
        if(delta.x != 1 || delta.y != 1)
            longHoldingAxis += delta;
        PrepareLongholding plh = symbolBox.GetComponent<PrepareLongholding>();
        if (longHoldingAxis.x >= 0.15)
        {
            key = plh.rects[2];
            return plh.texts[2].text[0];
        }
        else if (longHoldingAxis.x <= -0.15)
        {
            key = plh.rects[0];
            return plh.texts[0].text[0];
        }
        key = plh.rects[1];
        return plh.texts[1].text[0];
    }
}

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

    GameObject hoveringKey, checkKey = null;   // hoveringKey�ǵ�ǰ�����ڵİ�����checkKey�������жϳ�����
    Color oldColor, hoveringColor = new Color(255, 255, 0, 60);
    int _mode = 0;   //���ģʽ״̬��0-Сд��1-��д(����һ��Shift), 2-�����ַ�(�л�)
    bool isCapitalDisplay = false;   // �Ǵ�дչʾ�ļ���.
    Vector2 longHoldingAxis;

    Vector2 lastSlideAxis, lastSlideDelta;

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
                else if(Time.time - hold_time_start > 0.5)
                {
                    // ����1s, ��������ſ�.
                    longHolding = true;
                    
                    // TODO: ��symbolBox��λ�ø�ֵ.
                    symbolBox.position = hoveringKey.transform.position;  //��ûд�꣬Ҫ�����������ڳ�������ô�����޸�.
                    symbolBox.gameObject.SetActive(true);

                    // ��hoveringKey�������������ַ�����ֵ��.
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
                        // �� �� �� ��д-����-Сд.
                        text.text = str[i++];
                    }

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
        base.OnTouchDown(fromAction, fromSource);  //touched = true.
        // ��Ҫ��¼����!.
        hold_time_start = Time.time;
        // �ʼ��¼��ǰ���ĸ�������.
        Axis2Letter(PadSlide[fromSource].axis, fromSource, _mode, out hoveringKey);
        Material material = hoveringKey.GetComponent<MeshRenderer>().material;
        oldColor = material.color;
        material.color = hoveringColor;  //�ı䵱ǰ����������ɫ!
    }

    // OnTouchUp, �ɿ������壬����Ҫ�����!
    public override void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        base.OnTouchUp(fromAction, fromSource);  // touched = false.
        if (selected || deleted)  //��������ƶ�������ɾ���ַ����͵�����Ч!
            return;
        GameObject tmp;
        int ascii = longHolding ? longHoldingLogic(new Vector2(1,1)) : Axis2Letter(lastSlideAxis - lastSlideDelta, fromSource, _mode, out tmp);
        hoveringKey.GetComponent<MeshRenderer>().material.color = oldColor;
        Debug.Log("TouchUp: " + (char)ascii);
        // ������Ƽ���Ŀǰֻ��VKCode.Shift.
        if(ascii == (int)VKCode.Shift)  //Shift, mode��0��1����1��0. �������Ƽ���ʱ��_mode��Ӧ���ܱ�Ϊ2.
        {
            _mode = _mode == 1 ? 0 : 1;
            isCapitalDisplay = !isCapitalDisplay;
            switchCapital();
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
    }

    // ClickKeyboard�еİ��´�����û���ر�����壬�������������Ű�. PressUpһ������TouchUp�������ٵ���һ��.

    // Core: OnPadSlide.
    public override void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        lastSlideAxis = axis;
        lastSlideDelta = delta;
        // ��������������ǿյģ�����.
        if (deleted)  // ����ɾ��������!
            return;
        if (selected)
        {
            //���˰�������ƶ���ֻ꣬������꣬��������.
            do_caret_move(axis);
        }
        else
        {
            if (longHolding)
            {
                // TODO �Ѿ�������.
                longHoldingLogic(delta);
            }
            // ���Ҫ��С��ָʾ�Ļ���Ҳ�������С���λ��
            // ���������Ϊ�����ڼ������ƶ�.
            else
            {
                GameObject oldkey = hoveringKey;
                Axis2Letter(axis, fromSource, _mode, out hoveringKey);
                if (oldkey != hoveringKey)
                {
                    // ��ɫ.
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
        // �����ж�key�Ƿ���������ţ����Գ���.
        return key.transform.GetChild(0).transform.childCount == 2;
    }

    int longHoldingLogic(Vector2 delta)
    {
        //(0,0), ��ʼ���� ��1��1��������.
        if (delta.x == 0 && delta.y == 0)
        {
            longHoldingAxis.x = longHoldingAxis.y = 0;  //��ʼ��.
            return 0;
        }
        if(delta.x != 1 || delta.y != 1)
            longHoldingAxis += delta;
        if (longHoldingAxis.x >= 0.03)   //0.03 magic number ����.
            return symbolBox.GetChild(2).GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text[0];
        else if(longHoldingAxis.x <= -0.03)
            return symbolBox.GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text[0];
        return symbolBox.GetChild(1).GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text[0];
    }
}

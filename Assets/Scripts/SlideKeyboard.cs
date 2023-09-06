using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

/* ??�???�??�?????�?�??????ClickKeyboard????????????�?�?�?�?�??????�??Axis2Letter??�???. */
public class SlideKeyboard : ClickKeyboard
{
    private float radius = 1;
    public float thumbTheta;
    public float thumbLength;
    private Vector2[] thumbCenter = new Vector2[12];
    private float[] d= new float[12];
    Color hoveringColor = new Color(255, 255, 0, 30);  //TODO: ????.

    private int[,,] keys = new int[6, 5, 6] { { { 0x20, 0, 0, 0, 0, 0 }, { 'v', 'c', 'x', 'z', 0X10, 0 }, {'g', 'f', 'd', 's', 'a' , 0X10}, { 'y', 't', 'r', 'e', 'w', 'q' }, {  '.', '?', '!', 0, 0, 0} },
                                              { { 0x20, 0, 0, 0, 0, 0 }, { 'V', 'C', 'X', 'Z', 0X10, 0 }, {'G', 'F', 'D', 'S', 'A' , 0X10}, { 'Y', 'T', 'R', 'E', 'W', 'Q' }, {  '.', '?', '!', 0, 0, 0} },
                                              { { 0x20, 0, 0, 0, 0, 0 }, { '_', '-', ')', '(', 0X10, 0 }, {'%', '#', '@', '!', '~' , 0X10}, { '6', '5', '4', '3', '2', '1' }, {  '.', '?', '!', 0, 0, 0} },
                                              { { 0x20, 0x0D, 0, 0, 0, 0 }, {'v', 'b', 'n', 'm', 0X08, 0}, {'g', 'h', 'j', 'k', 'l' ,0X08} ,{'t', 'y', 'u', 'i', 'o', 'p'}, { ',', ':', '\"', 0, 0, 0} },
                                              { { 0x20, 0x0D, 0, 0, 0, 0 }, {'V', 'B', 'N', 'M', 0X08, 0}, {'G', 'H', 'J', 'K', 'L' ,0X08} ,{'T', 'Y', 'U', 'I', 'O', 'P'}, { ',', ':', '\"', 0, 0, 0} },
                                              { { 0x20, 0x0D, 0, 0, 0, 0 }, {'_', ':', ';', '/', 0X08, 0}, {'%', '\'', '&', '*', '?',0X08} ,{'5', '6', '7', '8', '9', '0'}, { ',', ':', '\"', 0, 0, 0} }};

    private int left_column = 0;
    private int right_column = 0;
    int _mode = 0;
    bool isCapitalDisplay = false;
    public GameObject keyboard_model;
    public GameObject keyboard_root;
    

    Vector2 lastSlideAxis, lastSlideDelta;

    // private void Start()        
    // {

    // }

    private void Update()
    {
        if (left_touched)
        {
            left_column = getPosition(PadSlide[SteamVR_Input_Sources.LeftHand].axis, SteamVR_Input_Sources.LeftHand);
        }
        else {
            left_column = -1;
        }
        if (right_touched)
        {
            right_column = getPosition(PadSlide[SteamVR_Input_Sources.RightHand].axis, SteamVR_Input_Sources.RightHand);
        }
        else {
            right_column = -1;
        }
        highlight(left_column, right_column);
        
    }

    public int getPosition(Vector2 axis, SteamVR_Input_Sources hand)
    {
        if(hand == SteamVR_Input_Sources.LeftHand)
        {
            for (int i = 0; i < 11; i++)
            {
                d[i] = thumbLength + (float)(i - 5) * 1 / 5f;
                thumbCenter[i] = new Vector2((d[i] * Mathf.Sin(thumbTheta)), (d[i] * Mathf.Cos(thumbTheta)));
            }
        }
        else
        {
            for (int i = 0; i < 11; i++)
            {
                d[i] = thumbLength + (float)(i - 5) * 1 / 5f;
                thumbCenter[i] = new Vector2(-(d[i] * Mathf.Sin(thumbTheta)), (d[i] * Mathf.Cos(thumbTheta)));
            }
        }

        int column;
        int row = 0;
        for (int i = 0; i < 11; i++)
        {
            float distanceA = Mathf.Sqrt(Mathf.Pow((axis.x - thumbCenter[i].x), 2) + Mathf.Pow((axis.y - thumbCenter[i].y), 2));
            float distanceB = Mathf.Sqrt(Mathf.Pow((axis.x - thumbCenter[i+1].x), 2) + Mathf.Pow((axis.y - thumbCenter[i+1].y), 2));

            if (distanceA < thumbLength && distanceB > thumbLength)
            {
                row = i;
                break;
            }
        }

        //print("d: " + axis.y);
        //print("d[row] " + d[row]);
        //print("thetaMax cos num " + Mathf.Min((Mathf.Pow(d[row], 2) + Mathf.Pow(thumbLength, 2) - Mathf.Pow(radius, 2)) / (2 * d[row] * thumbLength),
        //                                      (Mathf.Pow(d[row + 1], 2) + Mathf.Pow(thumbLength, 2) - Mathf.Pow(radius, 2)) / (2 * d[row + 1] * thumbLength)));

        int maxThetaRow = (Mathf.Pow(d[row], 2) + Mathf.Pow(thumbLength, 2) - Mathf.Pow(radius, 2)) / (2 * d[row] * thumbLength)
                            < (Mathf.Pow(d[row + 1], 2) + Mathf.Pow(thumbLength, 2) - Mathf.Pow(radius, 2)) / (2 * d[row + 1] * thumbLength) ? row : row + 1;

        float thetaMax = Mathf.Acos((Mathf.Pow(d[maxThetaRow], 2) + Mathf.Pow(thumbLength, 2) - Mathf.Pow(radius, 2)) / (2 * d[maxThetaRow] * thumbLength));

        float currentTheta = (thumbTheta == 0) ? Mathf.Atan((axis.x - thumbCenter[maxThetaRow].x) / (axis.y - thumbCenter[maxThetaRow].y)) : 
                                                 Mathf.Abs(Mathf.Atan((axis.x - thumbCenter[maxThetaRow].x) / (axis.y - thumbCenter[maxThetaRow].y))) - thumbTheta;

        float fcolumn = (currentTheta + thetaMax) / (2 * thetaMax / 6);

        //print("thetaMax" + thetaMax);
        //print("currentTheta" + currentTheta);
        //print("fcolumn" + fcolumn);

        column = (int)fcolumn;
        if (column < 0) column = 0;
        if (column > 5) column = 5;
        
        return column;
    }

    public void highlight(int left_column, int right_column){
        for(int i = 1; i <= 36; i++){
            string str = i.ToString();
            GameObject highlight_key = GameObject.Find("Player/SteamVRObjects/VRCamera/SlideKeyboard_orimodel/Line0" + str);
            highlight_key.GetComponent<MeshRenderer>().material.color = Color.white;
                
        }
        
        if(left_column != -1){
            for(int i = 0; i < 6; i++){
                int num = left_column + 1 + i * 6;
                string str = num.ToString();
                GameObject highlight_key = GameObject.Find("Player/SteamVRObjects/VRCamera/SlideKeyboard_orimodel/Line0" + str);
                Material mat = highlight_key.GetComponent<MeshRenderer>().material;
                mat.color = hoveringColor;
            }
        }
        if(right_column != -1){
            for(int i = 0; i < 6; i++){
                int num = (5-right_column) * 6 + i + 1;
                string str = num.ToString();
                GameObject highlight_key = GameObject.Find("Player/SteamVRObjects/VRCamera/SlideKeyboard_orimodel/Line0" + str);
                Material mat = highlight_key.GetComponent<MeshRenderer>().material;
                mat.color = hoveringColor;
            }
        }
        if(left_column != -1 && right_column != -1){
            int num = (5-right_column) * 6 + left_column + 1;
            string str = num.ToString();
            GameObject highlight_key = GameObject.Find("Player/SteamVRObjects/VRCamera/SlideKeyboard_orimodel/Line0" + str);
            Material mat = highlight_key.GetComponent<MeshRenderer>().material;
            mat.color = Color.red;
        }
    }
    
    public override void OnPressDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if(right_touched && left_touched){
            hold_time_start = Time.time;
        }

    }

    public override void OnPressUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if(right_touched && left_touched){
            GameObject tmp;
            int ascii = Axis2Letter(new Vector2(left_column, right_column), fromSource, _mode, out tmp);

            if(ascii == (int)VKCode.Shift)  
            {
                if(_mode != 2)
                {
                    _mode = _mode == 1 ? 0 : 1;
                    isCapitalDisplay = !isCapitalDisplay;
                    switchCapital();
                }

            }
            else if(ascii == (int)VKCode.Switch)   //�?�?���?��?����??��?����?�����?����.
            {
                _mode = _mode == 2 ? (isCapitalDisplay ? 1 : 0) : 2;
                switchSymbol();
            }
            // ����������?���???����?�.
            else
            {
                OutputLetter(ascii);
                if(longHolding)
                {
                    symbolBox.gameObject.SetActive(false); // ������?�
                    longHolding = false;
                }
            }
        }

    }
    

    public override void OnTouchDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if(fromSource == SteamVR_Input_Sources.RightHand)
            right_touched = true;
        if(fromSource == SteamVR_Input_Sources.LeftHand)
            left_touched = true;
    }

    public override void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if(fromSource == SteamVR_Input_Sources.RightHand)
            right_touched = false;
        if(fromSource == SteamVR_Input_Sources.LeftHand)
            left_touched = false;
    }
    // public override void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    // {
    //     if(fromSource == SteamVR_Input_Sources.LeftHand)
    //         LeftHand_axis = PadSlide[SteamVR_Input_Sources.LeftHand].axis;
    //     if(fromSource == SteamVR_Input_Sources.RightHand)
    //         RightHand_axis = PadSlide[SteamVR_Input_Sources.RightHand].axis;    
    // }

    public override int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode, out GameObject useless){
        int ascii;
        int cnt = (int)((5-axis.x)*6 + axis.y);
        useless = null;
        Transform key = transform.GetChild(cnt);

        switch (key.name)
        {
            case "Sym":
                ascii = 0x00;
                break;
            case "Space":
                ascii = 0x20;
                break;
            case "Enter":
                ascii = 0x0D;
                break;
            case "Shift":
                ascii = 0x10;
                break;
            case "Back":
                ascii = 0x08;
                break;            
            default:
                ascii = key.name[mode];
                break;
        }

        return ascii;
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
}
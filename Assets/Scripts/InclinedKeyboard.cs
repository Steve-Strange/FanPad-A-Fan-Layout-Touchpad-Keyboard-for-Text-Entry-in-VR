using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

/* 倾斜�?盘，继承自ClickKeyboard，应当只用实现自己的Axis2Letter方法. */
public class InclinedKeyboard : ClickKeyboard
{
    private float radius = 1;
    public float thumbTheta;
    public float thumbLength;
    private Vector2[] thumbCenter = new Vector2[7];
    private float[] d = new float[7];

    private int[] keyColumn = new int[5] {4, 5, 6, 6, 4};

    private int[,,] keys = new int[6, 5, 6] { { { 0x20, 0x20, 0, 0, 0, 0 }, { 'v', 'c', 'x', 'z', 0X10, 0 }, {'g', 'f', 'd', 's', 'a' , 0X10}, { 'y', 't', 'r', 'e', 'w', 'q' }, { '.', '.', '!', '?', 0, 0 } },
                                              { { 0x20, 0x20, 0, 0, 0, 0 }, { 'V', 'C', 'X', 'Z', 0X10, 0 }, {'G', 'F', 'D', 'S', 'A' , 0X10}, { 'Y', 'T', 'R', 'E', 'W', 'Q' }, { '.', '.', '!', '?', 0, 0 } },
                                              { { 0x20, 0x20, 0, 0, 0, 0 }, { '_', '-', ')', '(', 0X10, 0 }, {'%', '#', '@', '!', '~' , 0X10}, { '6', '5', '4', '3', '2', '1' }, { '.', '.', '!', '?', 0, 0 } },
                                              { { 0x20, 0x20, 0x0D, 0x0D, 0, 0 }, {'v', 'b', 'n', 'm', 0X08, 0}, {'g', 'h', 'j', 'k', 'l' ,0X08} ,{'t', 'y', 'u', 'i', 'o', 'p'}, {',', ',', ':', '\"', 0, 0}},
                                              { { 0x20, 0x20, 0x0D, 0x0D, 0, 0 }, {'V', 'B', 'N', 'M', 0X08, 0}, {'G', 'H', 'J', 'K', 'L' ,0X08} ,{'T', 'Y', 'U', 'I', 'O', 'P'}, {',', ',', ':', '\"', 0, 0}},
                                              { { 0x20, 0x20, 0x0D, 0x0D, 0, 0 }, {'_', ':', ';', '/', 0X08, 0}, {'%', '\'', '&', '*', '?',0X08} ,{'5', '6', '7', '8', '9', '0'}, {',', ',', ':', '\"', 0, 0}}};

    // private void Start()
    // {

    // }
    // private void Update()
    // {
    //    GameObject key;
    //    if (touched)
    //    {
    //        if (PadSlide[SteamVR_Input_Sources.LeftHand].axis != new Vector2(0, 0))
    //        {
    //            Axis2Letter(PadSlide[SteamVR_Input_Sources.LeftHand].axis, SteamVR_Input_Sources.LeftHand, 0, out key);
    //            //Debug.Log("Key: " + ascii);
    //        }
    //        if (PadSlide[SteamVR_Input_Sources.RightHand].axis != new Vector2(0, 0))
    //        {
    //            Axis2Letter(PadSlide[SteamVR_Input_Sources.RightHand].axis, SteamVR_Input_Sources.RightHand, 0, out key);
    //            //Debug.Log("Key: " + ascii);
    //        }
    //    }
    // }

    public override int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode, out GameObject key)
    {
        if(hand == SteamVR_Input_Sources.LeftHand)
        {
            for (int i = 0; i < 6; i++)
            {
                d[i] = thumbLength + (float)(i - 2.5) * 1 / 2.5f;
                thumbCenter[i] = new Vector2((d[i] * Mathf.Sin(thumbTheta)), (d[i] * Mathf.Cos(thumbTheta)));
            }
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                d[i] = thumbLength + (float)(i - 2.5) * 1 / 2.5f;
                thumbCenter[i] = new Vector2(-(d[i] * Mathf.Sin(thumbTheta)), (d[i] * Mathf.Cos(thumbTheta)));
            }
        }

        int column;
        int row = 0;
        for (int i = 0; i < 5; i++)
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

        float fcolumn = (currentTheta + thetaMax) / (2 * thetaMax / keyColumn[maxThetaRow]);

        //print("thetaMax" + thetaMax);
        //print("currentTheta" + currentTheta);
        //print("fcolumn" + fcolumn);

        column = (int)fcolumn;
        if (column < 0) column = 0;
        if (column > keyColumn[row] - 1) column = keyColumn[row] - 1;

        Debug.Log("( " + column + ' ' + row + " )");

        int handmode = (hand == SteamVR_Input_Sources.LeftHand) ? mode : mode + 3;

        char output = (char)keys[handmode, row, column];
        Debug.Log(output);
        Transform LR = hand == SteamVR_Input_Sources.LeftHand ? keyboardRoot.GetChild(0) : keyboardRoot.GetChild(1);

        switch (output)
        {
            case (char)VKCode.Space:
                key = LR.Find("space").gameObject;
                break;
            case (char)VKCode.Shift:
                key = LR.Find("shift").gameObject;
                break;
            case (char)VKCode.Switch:
                key = LR.Find("sym").gameObject;
                break;
            case (char)VKCode.Enter:
                key = LR.Find("enter").gameObject;
                break;
            case (char)VKCode.Back:
                key = LR.Find("back").gameObject;
                break;
            case ',':
                key = LR.Find("comma").gameObject;
                break;
            case '.':
                key = LR.Find("period").gameObject;
                break;
            case '\"':
                key = LR.Find("quotation").gameObject;
                break;
            case '!':
                if (row != 4) goto default;
                key = LR.Find("exclamation").gameObject;
                break;
            case '?':
                if (row != 4) goto default;
                key = LR.Find("question").gameObject;
                break;
            case ':':
                if (row != 4) goto default;
                key = LR.Find("colon").gameObject;
                break;
            default:
                string name = ((char)keys[handmode - mode, row, column]).ToString() + ((char)keys[handmode - mode + 2, row, column]).ToString();
                if (name[1] == '/')
                    name = "m\\";
                key = LR.Find(name).gameObject;
                break;
        }

        return keys[handmode, row, column];
    }

    protected override TextMeshProUGUI[,] fetchKeyStrings()
    {
        TextMeshProUGUI[,] ret = new TextMeshProUGUI[2, 30];
        int i = 0;
        Transform[] children = new Transform[2] { keyboardRoot.GetChild(0), keyboardRoot.GetChild(1) };
        foreach(Transform LR in children)
        {
            foreach(var keys in LR.GetComponentsInChildren<MeshRenderer>())
            {
                Transform canvas = keys.transform.GetChild(0);
                if(canvas.childCount == 2)
                {
                    foreach(var text in canvas.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        if (text.text[0] >= 'a' && text.text[0] <= 'z')
                            ret[0, i] = text;
                        else
                            ret[1, i] = text;
                    }
                    ++i;
                }
            }
        }
        return ret;
    }
}

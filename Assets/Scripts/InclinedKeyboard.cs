using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/* 倾斜键盘，继承自ClickKeyboard，应当只用实现自己的Axis2Letter方法. */
public class InclinedKeyboard : ClickKeyboard
{
    private float radius = 1;
    public float thumbTheta;
    public float thumbLength;
    private Vector2[] thumbCenter = new Vector2[7];
    private float[] d = new float[7];

    private int[] keyColumn = new int[6] { 1, 2, 3, 5, 4, 3};

    private char[,,] keys = new char[6, 6, 2];      //Fill in the keys

    private void Start()
    {
        for (int i = 0; i < 7; i++)
        {
            d[i] = thumbLength + (float) (i - 3) * 1 / 3f;
            thumbCenter[i] = new Vector2((d[i] * Mathf.Sin(thumbTheta)), (d[i] * Mathf.Cos(thumbTheta)));
            print(d[i]);
            print(thumbCenter[i]);
        }
    }

    //作为测试，在Update里面轮询.
    private void Update()
    {
        GameObject key;
        if (touched)
        {
            if (PadSlide[SteamVR_Input_Sources.LeftHand].axis != new Vector2(0, 0))
            {
                Axis2Letter(PadSlide[SteamVR_Input_Sources.LeftHand].axis, SteamVR_Input_Sources.LeftHand, 0, out key);
                //Debug.Log("Key: " + ascii);
            }
            if (PadSlide[SteamVR_Input_Sources.RightHand].axis != new Vector2(0, 0))
            {
                Axis2Letter(PadSlide[SteamVR_Input_Sources.RightHand].axis, SteamVR_Input_Sources.RightHand, 0, out key);
                //Debug.Log("Key: " + ascii);
            }
        }
    }

    public override int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode, out GameObject key)
    {
        // TODO: 获取相应位置的按件对象并赋值给key
        int column;
        int row = 0;
        for (int i = 0; i < 6; i++)
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

        float frow = (currentTheta + thetaMax) / (2 * thetaMax / keyColumn[maxThetaRow]);

        //print("thetaMax" + thetaMax);
        //print("currentTheta" + currentTheta);
        //print("frow" + frow);

        column = (int)frow;
        if (column < 0) column = 0;
        if (column > keyColumn[row] - 1) column = keyColumn[row] - 1;

        Debug.Log("( " + column + ' ' + row + " )");

        key = this.gameObject;
        return keys[column, row, mode];
    }
}

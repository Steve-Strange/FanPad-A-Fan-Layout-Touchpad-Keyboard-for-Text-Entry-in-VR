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

    private int[] keyColumn = new int[6] { 3, 4, 5, 3, 2, 1 };

    private char[,,] keys = new char[6, 6, 2];      //Fill in the keys

    private void Start()
    {
        for (int i = 0; i < 7; i++)
        {
            d[i] = thumbLength + (i - 3) * 1 / 3;
            thumbCenter[i] = new Vector2((d[i] * Mathf.Sin(thumbTheta)), (d[i] * Mathf.Cos(thumbTheta)));
        }
    }

    public override int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode, ref GameObject key)
    {
        int row;
        int column = 0;
        for (int i = 0; i < 6; i++)
        {
            float distanceA = Mathf.Sqrt(Mathf.Pow((axis.x - thumbCenter[i].x), 2) + Mathf.Pow((axis.y - thumbCenter[i].y), 2));
            float distanceB = Mathf.Sqrt(Mathf.Pow((axis.x - thumbCenter[i+1].x), 2) + Mathf.Pow((axis.y - thumbCenter[i+1].y), 2));
            if (distanceA > thumbLength && distanceB < thumbLength)
            {
                column = i;
                break;
            }
        }

        float thetaMax = Mathf.Acos((Mathf.Pow(d[column + 1], 2) + Mathf.Pow(thumbLength, 2) - Mathf.Pow(radius, 2)) / 2 * d[column + 1] * thumbLength);

        float currentTheta = Mathf.Abs(Mathf.Atan((axis.x - thumbCenter[column + 1].x) / (axis.y - thumbCenter[column + 1].y))) - thumbTheta;

        float frow = (currentTheta + thetaMax) / (2 * thetaMax / keyColumn[column]);

        row = (int)frow;


        return keys[column, row, mode];
    }
}

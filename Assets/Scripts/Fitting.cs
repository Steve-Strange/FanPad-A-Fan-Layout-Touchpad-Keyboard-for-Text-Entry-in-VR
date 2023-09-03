using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Linq;
using System;
using UnityEngine.UIElements;

public class Fitting : KeyboardBase
{
    List<Vector2> points = new List<Vector2>{};

    public GameObject InclinedKeyboard;
    public bool onFittingMode = false;
    public float theta;
    public float radius;

    // Start is called before the first frame update
    void Start()
    {
        InclinedKeyboard = GameObject.Find("InclinedKeyboard");
    }

    // Update is called once per frame
    void Update()
    {
        if(!onFittingMode) points.Clear();
        if(onFittingMode && points.Count < 200) {
            print(points.Count);
            GetCostomized();
        }
    }
    void GetCostomized(){
        if(left_touched)
        {
            points.Add(PadSlide[SteamVR_Input_Sources.LeftHand].axis);
        }
        if(right_touched)
        {
            points.Add(PadSlide[SteamVR_Input_Sources.RightHand].axis);
        }
        if(points.Count == 200) {
            LeastSquaresFit(points);
            Debug.LogWarning("done fitting!!");
            // InclinedKeyboard.GetComponent<InclinedKeyboard>().thumbTheta = theta;
            // InclinedKeyboard.GetComponent<InclinedKeyboard>().thumbLength = radius < 5.5 ? 5.5f : radius;
            InclinedKeyboard.GetComponent<InclinedKeyboard>().setThetaR(theta, radius);
        }
    }
    
    // void FitCircleCenter(List<Vector2> points)
    // {
    //     radius = 0;
    //     theta = 0;
    //     float sumX = points.Sum(p => p.x);
    //     float sumY = points.Sum(p => p.y);
    //     float sumX2 = points.Sum(p => p.x * p.x);
    //     float sumY2 = points.Sum(p => p.y * p.y);
    //     float sumXY = points.Sum(p => p.x * p.y);
    //     int n = points.Count;

    //     float centerX = (sumX2 * sumY - sumX * sumXY) / (n * sumX2 - sumX * sumX);
    //     float centerY = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);

    //     for (int i = 0; i < n; i++) {
    //         print(radius);
    //         radius += Mathf.Sqrt((points[i].x - centerX)*(points[i].x - centerX) + (points[i].y - centerY)*(points[i].y - centerY)) / n;
    //     }
    //     theta = Mathf.Abs(Mathf.Atan(centerY / centerX));
    //     Debug.LogWarning(centerX);
    //     Debug.LogWarning(centerY);
    //     Debug.LogWarning("theta: " + theta);
    //     Debug.LogWarning("radius: " + radius);
    // }

    void LeastSquaresFit(List<Vector2> points)
    {
        float cent_x = 0.0f,
              cent_y = 0.0f;
        float sum_x = 0.0f, sum_y = 0.0f;
        float sum_x2 = 0.0f, sum_y2 = 0.0f;
        float sum_x3 = 0.0f, sum_y3 = 0.0f;
        float sum_xy = 0.0f, sum_x1y2 = 0.0f, sum_x2y1 = 0.0f;
        int N = points.Count;
        float x, y, x2, y2;
        for (int i = 0; i < N; i++)
        {
            x = points[i].x;
            y = points[i].y;
            x2 = x * x;
            y2 = y * y;
            sum_x += x;
            sum_y += y;
            sum_x2 += x2;
            sum_y2 += y2;
            sum_x3 += x2 * x;
            sum_y3 += y2 * y;
            sum_xy += x * y;
            sum_x1y2 += x * y2;
            sum_x2y1 += x2 * y;
        }
        float C, D, E, G, H;
        float a, b, c;
        C = N * sum_x2 - sum_x * sum_x;
        D = N * sum_xy - sum_x * sum_y;
        E = N * sum_x3 + N * sum_x1y2 - (sum_x2 + sum_y2) * sum_x;
        G = N * sum_y2 - sum_y * sum_y;
        H = N * sum_x2y1 + N * sum_y3 - (sum_x2 + sum_y2) * sum_y;
        a = (H * D - E * G) / (C * G - D * D);
        b = (H * C - E * D) / (D * D - G * C);
        c = -(a * sum_x + b * sum_y + sum_x2 + sum_y2) / N;
        cent_x = a / (-2);
        cent_y = b / (-2);

        radius = Mathf.Sqrt(a * a + b * b - 4 * c) * 2;
        radius = radius < 5.5 ? 5.5f : radius;

        theta = Mathf.Abs(Mathf.Atan(cent_x / cent_y));
        Debug.LogWarning(cent_x);
        Debug.LogWarning(cent_y);
        Debug.LogWarning("theta: " + theta);
        Debug.LogWarning("radius: " + radius);
    }

}
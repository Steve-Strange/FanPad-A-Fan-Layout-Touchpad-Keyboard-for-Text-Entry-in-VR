using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Linq;
using System;

public class Customized : KeyboardBase
{
    Vector2 leftAxis, rightAxis;
    float leftAxisX = 0, rightAxisX = 0;
    List<Vector2> points = new List<Vector2>{};

    float theta;
    // Start is called before the first frame update
    void Start()
    {
        while(!left_touched && !right_touched) print("waiting for fitting");
        GetCostomized();
        print(theta);
        print(FitCircleCenter(points));
    }

    // Update is called once per frame
    void Update()
    {

    }
    void GetCostomized(){
        while(left_touched)
        {
            if(PadSlide[SteamVR_Input_Sources.LeftHand].axis.x < leftAxisX)
            {
                leftAxis = PadSlide[SteamVR_Input_Sources.LeftHand].axis;
                leftAxisX = PadSlide[SteamVR_Input_Sources.LeftHand].axis.x;
                points.Add(leftAxis);
            }
            if(PadSlide[SteamVR_Input_Sources.LeftHand].axis.x > rightAxisX)
            {
                rightAxis = PadSlide[SteamVR_Input_Sources.LeftHand].axis;
                rightAxisX = PadSlide[SteamVR_Input_Sources.LeftHand].axis.x;
                points.Add(rightAxis);
            }
        }
        while(right_touched)
        {
            if(PadSlide[SteamVR_Input_Sources.RightHand].axis.x < leftAxisX)
            {
                leftAxis = PadSlide[SteamVR_Input_Sources.RightHand].axis;
                leftAxisX = PadSlide[SteamVR_Input_Sources.RightHand].axis.x;
            }
            if(PadSlide[SteamVR_Input_Sources.RightHand].axis.x > rightAxisX)
            {
                rightAxis = PadSlide[SteamVR_Input_Sources.RightHand].axis;
                rightAxisX = PadSlide[SteamVR_Input_Sources.RightHand].axis.x;
            }
        }
        theta = Mathf.Atan(Mathf.Abs(leftAxis.y - rightAxis.y)/Mathf.Abs(leftAxis.x - rightAxis.x));
    }
    
    Vector2 FitCircleCenter(List<Vector2> points)
    {
        float sumX = points.Sum(p => p.x);
        float sumY = points.Sum(p => p.y);
        float sumX2 = points.Sum(p => p.x * p.x);
        float sumY2 = points.Sum(p => p.y * p.y);
        float sumXY = points.Sum(p => p.x * p.y);
        int n = points.Count;

        float centerX = (sumX2 * sumY - sumX * sumXY) / (n * sumX2 - sumX * sumX);
        float centerY = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);

        return new Vector2(centerX, centerY);
    }

}
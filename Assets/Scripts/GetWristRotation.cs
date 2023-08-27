using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

public class GetWristRotation : MonoBehaviour
{
    
    // 获取左手手柄
    public Transform leftHandTransform;


    // 获取右手手柄  
    public Transform rightHandTransform;

    void Start()
    {   
        leftHandTransform = GameObject.Find("LeftHand").transform;
        rightHandTransform = GameObject.Find("RightHand").transform;
    }

    // Update is called once per frame
    void Update()
    {
        // 读取手柄旋转  
        Quaternion leftRot = leftHandTransform.rotation;
        Quaternion rightRot = rightHandTransform.rotation;

        print(leftRot);
        print(rightRot);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 不断向HMD正前方（Main camera的方向）投射射线.
public class HMDForwordRay : MonoBehaviour
{
    Transform trackedHMD;
    // Start is called before the first frame update
    void Start()
    {
        trackedHMD = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 在FixedUpdate中投射射线.
    private void FixedUpdate()
    {
        Ray hmdray = new Ray(trackedHMD.position, trackedHMD.forward);
        Physics.Raycast(hmdray, 100);  //默认会触发trigger.
    }
}

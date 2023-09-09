using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 不断向HMD正前方（Main camera的方向）投射射线. 识别碰到了哪个单词块并且调用WordCubes的控制函数.
public class HMDForwordRay : MonoBehaviour
{
    Transform trackedHMD;
    public WordCubes wordCubes;
    // Start is called before the first frame update
    void Start()
    {
        trackedHMD = Camera.main.transform;
        wordCubes = GameObject.Find("wordcubes").GetComponent<WordCubes>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 在FixedUpdate中投射射线.
    private void FixedUpdate()
    {
        Ray hmdray = new Ray(trackedHMD.position, trackedHMD.forward);
        RaycastHit raycasthit;
        //Debug.DrawRay(trackedHMD.position, trackedHMD.forward * 100, Color.red);
        if(Physics.Raycast(hmdray, out raycasthit, 100)){   //默认会触发trigger.
            if(raycasthit.collider.name.Substring(0, raycasthit.collider.name.Length-1)=="wordcube"){
                //Debug.LogWarning("hit " + raycasthit.collider.name);
                int index = raycasthit.collider.name[raycasthit.collider.name.Length-1] - '1';
                //Debug.LogWarning("index = " + index.ToString());
                wordCubes.selectbyRay(index);
            }
        }
        else{
            wordCubes.selectbyRay(-1);
        }
    }
}

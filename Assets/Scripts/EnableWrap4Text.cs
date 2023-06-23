using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnableWrap4Text : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.ScrollRect;
    }
    /* TODO: 使用 Page模式，并且在写完的时候自动翻页!
     * 这应该需要根据InputField的一些状态或者实现回调函数(如果有相应的事件的话).
     */

    // Update is called once per frame
    void Update()
    {
        
    }
}

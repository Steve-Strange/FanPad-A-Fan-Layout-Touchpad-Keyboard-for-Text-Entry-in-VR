using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/* 第三个方案基于滑动的方案和其他有不太一样的逻辑。这个类在KeyboardBase的基础上实现第三个方案. */
public class SlideKeyboard : KeyboardBase
{
    int row = 2;
    int column = 2;
    public override void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {

        /* 
         手指在触摸板上滑动。一个是以一定的规则移动改变高亮，一个在按下扳机的时候移动光标.
         */

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

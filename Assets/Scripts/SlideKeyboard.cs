using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using Valve.VR;

/* 第三个方案基于滑动的方案和其他有不太一样的逻辑。这个类在KeyboardBase的基础上实现第三个方案. */
public class SlideKeyboard : KeyboardBase
{
    Dictionary<string,char> ALT1 = new Dictionary<string,char>();
    Dictionary<string,char> ALT2 = new Dictionary<string, char>();

    bool have_slide = false;
    Vector3 last_move;    
    public GameObject controller;
    private Collider target;
    
    private int mode = 0;

    private float deltatime = 1.5f;
    private float begintime;
  
    private bool press = false;
    public GameObject Alt;

    private Vector3 EndPos;
    private Vector3 PressPos;
   

    public TextMeshProUGUI[] Up;
    public TextMeshProUGUI[] Mid;
    protected TextMeshProUGUI[,] Key = new TextMeshProUGUI[2,36];
    public TextMeshProUGUI left;
    public TextMeshProUGUI mid;
    public TextMeshProUGUI right;
    private bool first = true;
    // Start is called before the first frame update
    //void Start()
    //{
        
    //    fetchKeyStrings();
    //}

    // Update is called once per frame
    void Update()
    {
        if (press == true && Time.time - begintime > deltatime && target != null && target.name.Length == 3)
        {
            Alt.SetActive(true);
            right.text = target.name[0].ToString();
            left.text = target.name[1].ToString();
            mid.text = target.name[2].ToString();
            longHolding = true;

        }
        else longHolding = false;
        Debug.Log(longHolding);
    }
    
    protected override TextMeshProUGUI[,] fetchKeyStrings()
    {
        for (int i = 0; i <= Up.Length-1; i++)
        {
            Key[1, i] = Up[i];
            Key[0, i] = Mid[i];
         }
        return Key;
    }
    override public void OnTouchDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        touched = true;
    }

    override public void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        touched = false;
        have_slide = false;
        //Debug.Log("Touch Up");
        // 最后一次松开的瞬间因为触摸读取有误，应该把最后一次移动逆向返回去.
        controller.transform.localPosition = controller.transform.localPosition - last_move;
    }

    override public void OnPressDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 在SlideKeyboard中，要用这个开始判断长按. */
        press = true;
        PressPos = PadSlide[fromSource].axis;

        if (target != null)
           target.GetComponent<MeshRenderer>().material.color = Color.yellow;
        begintime = Time.time;
   
    }
    private void OnTriggerEnter(Collider other)
    { 
        
        if(longHolding == false)
        { 
        Debug.LogWarning("enter the"+other.name);
        target = other;
        other.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!longHolding)
        {
            target = other;
            other.GetComponent<MeshRenderer>().material.color = Color.yellow;
        }
    }
    void OnTriggerExit(Collider other)      //  触发结束被调用  
    {
       
        if(!longHolding)
        {
        other.GetComponent<MeshRenderer>().material.color = Color.white;
        target = null;
        Debug.LogWarning("Exit");//同等于print("")输出

        }

        
    }

    override public void OnPressUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* 不管是什么键盘，这里都需要输出字符了! */
        //区分实词键和功能键
       // Debug.LogWarning("PRESS UP ONE");
        if (target == null) return;
       // 
        if(longHolding && target.name.Length == 3)
        {
             Debug.Log(PadSlide[fromSource].axis[0]+" "+ PressPos[0]);
            if (PadSlide[fromSource].axis[0]-PressPos[0]>0.05)
                OutputLetter(target.name[0]);
            else if (PadSlide[fromSource].axis[0] - PressPos[0] < - 0.05)
                OutputLetter(target.name[1]);
            else 
                OutputLetter(target.name[2]);
            
        }
        else
        {

        if(target.name == "shift")
        {

            switchCapital();
            if (mode == 0) mode = 1;
            else if(mode == 1) mode = 0;

        }
        else if(target.name =="Symbol")
        {
            switchSymbol();
            if (mode == 0)
                mode = 2;
            else if (mode == 2)
                mode = 0;
        }
        else if(target.name == "back")
        {
            OutputLetter((int )VKCode.Back);
        }
        else if(target.name =="sp")
        {
            OutputLetter(' ');
        }


            if(target.name.Length == 3)
            {
                //Debug.Log("Should print "+mode.ToString()+" "+ target.name[0]);
            if (mode == 0)
                OutputLetter(target.name[0]);
            if (mode == 1)
                OutputLetter(target.name[1]);
            if (mode == 2)
                OutputLetter(target.name[2]); 
            }
            else if(target.name.Length == 1)
            {
                OutputLetter(target.name[0]);
            }
        }
        
        longHolding = false;
        if(target!=null)
        target.GetComponent<MeshRenderer>().material.color = Color.white;
        Alt.SetActive(false);
        press = false;
    }

    override public void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        /* 
         手指在触摸板上滑动。一个是以一定的规则移动改变高亮，一个在按下扳机的时候移动光标.
         */
        
        
        if (controller == null) return;

        if (!have_slide)
        {
            have_slide = true;
            return;
        }
        
        else if(selected == false)
        {
            Vector3  move = new Vector3(-delta.x*2.5f, 0,-delta.y*2.5f);
            //映射逻辑
            Debug.Log(move.magnitude);
            if (move.magnitude < 1)
            {
                controller.transform.localPosition = controller.transform.localPosition + move;
                last_move = move;
            }
          // EndPos
    
        }
        else
        {   //int move = 0;
            //if (delta[0] > 0)
            //{
            //    move =(int) (delta[0] / 2 * 5);
            //    inputField.caretPosition = inputField.caretPosition + move;
            //}
            //else if (delta[0] < 0)
            //{
            //    move = (int)((-delta[0]) / 2 * 5);
            //    inputField.caretPosition = inputField.caretPosition - move;
            //}
            do_caret_move(axis);
        }

    }



  



}


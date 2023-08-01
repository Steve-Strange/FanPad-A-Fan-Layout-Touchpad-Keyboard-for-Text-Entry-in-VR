//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.ExceptionServices;
//using TMPro;
//using UnityEngine;
//using Valve.VR;

///* 第三个方案基于滑动的方案和其他有不太一样的逻辑。这个类在KeyboardBase的基础上实现第三个方案. */
//public class SlideKeyboard : KeyboardBase
//{
//    int right;
//    int left;
//    public GameObject[] targets = new GameObject[36];
//  //  Dictionary<string,char> ALT1 = new Dictionary<string,char>();
//   // Dictionary<string,char> ALT2 = new Dictionary<string, char>();

//    bool have_slide = false;
//    Vector3 last_move;    
//    public GameObject controller;
//    private Collider target;
    
//    private int mode = 0;

//    private float deltatime = 0.8f;
//    private float begintime;
//    private float loose;

//    private bool press = false;
//    public GameObject Alt;
//    public GameObject l;
//    public GameObject m;
//    public GameObject r;
//    public Material material;

//    private Vector3 EndPos;
//    private Vector3 PressPos;
   

//    public TextMeshProUGUI[] Up;
//    public TextMeshProUGUI[] Mid;
//    protected TextMeshProUGUI[,] Key = new TextMeshProUGUI[2,36];
//    public TextMeshProUGUI left;
//    public TextMeshProUGUI mid;
//    public TextMeshProUGUI right;
//    private bool first = true;
//    private char choose;
//    // Start is called before the first frame update
//    //void Start()
//    //{
        
//    //    fetchKeyStrings();
//    //}

//    // Update is called once per frame
//    void Update()
//    {
//        if (selected == true && Time.time - begintime > deltatime && target != null && target.name.Length == 3)
//        {
//            Alt.SetActive(true);
//            right.text = target.name[0].ToString();
//            left.text = target.name[1].ToString();
//            mid.text = target.name[2].ToString();
//            longHolding = true;

//        }
//        else longHolding = false;
//        Debug.Log(longHolding);
//    }
    
//    protected override TextMeshProUGUI[,] fetchKeyStrings()
//    {
//        for (int i = 0; i <= Up.Length-1; i++)
//        {
//            Key[1, i] = Up[i];
//            Key[0, i] = Mid[i];
//         }
//        return Key;
//    }
//    override public void OnTouchDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
//    {
//        touched = true;
//        if(selected == true)
//            PressPos = PadSlide[fromSource].axis;
//    }

//    override public void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
//    {
//        touched = false;
        
//        //Debug.Log("Touch Up");
//        // 最后一次松开的瞬间因为触摸读取有误，应该把最后一次移动逆向返回去.
//        controller.transform.localPosition = controller.transform.localPosition - last_move;
//        first = true;

//    }

//   /* override public void OnPressDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
//    {
       
//        press = true;
//        PressPos = PadSlide[fromSource].axis;

//        if (target != null)
//           target.GetComponent<MeshRenderer>().material.color = Color.yellow;
//        begintime = Time.time;
   
//    }*/
//    private void OnTriggerEnter(Collider other)
//    { 
        
//        if(longHolding == false)
//        { 
//        Debug.LogWarning("enter the"+other.name);
//        target = other;
//        other.GetComponent<MeshRenderer>().material.color = Color.yellow;
//        }
//    }
//    private void OnTriggerStay(Collider other)
//    {
//        if (!longHolding)
//        {
//            target = other;
//            other.GetComponent<MeshRenderer>().material.color = Color.yellow;
//        }
//    }
//    void OnTriggerExit(Collider other)      //  触发结束被调用  
//    {
       
//        if(!longHolding)
//        {
//        other.GetComponent<MeshRenderer>().material.color = Color.white;
//        target = null;
//        Debug.LogWarning("Exit");//同等于print("")输出

//        }

        
//    }

//    override public void OnSelectKeyDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
//    {
//        selected = true;
//        last_caret_time = Time.time;

        

//        if (target != null)
//            target.GetComponent<MeshRenderer>().material.color = Color.yellow;
//        begintime = Time.time;
//    }

//    override public void OnSelectKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
//    {
//        /* 松开删除键 */
//        loose = Time.time;
//        selected = false;
//        r.GetComponent<MeshRenderer>().material = material;
//        m.GetComponent<MeshRenderer>().material = material;
//        l.GetComponent<MeshRenderer>().material = material;
//        if (target == null) return;
//        // 
//        if(longHolding && target.name.Length == 3)
//         {
//              Debug.Log(PadSlide[fromSource].axis[0]+" "+ PressPos[0]);
//             if (choose == 'r')
//                 OutputLetter(target.name[0]);
//             else if (choose == 'l')
//                 OutputLetter(target.name[1]);
//             else 
//                 OutputLetter(target.name[2]);

//         }
//         else
//         {

//             if(target.name == "shift")
//             {

//                 switchCapital();
//                 if (mode == 0) mode = 1;
//                 else if(mode == 1) mode = 0;

//             }
//             else if(target.name =="Symbol")
//             {
//                 switchSymbol();
//                 if (mode == 0)
//                     mode = 2;
//                 else if (mode == 2)
//                     mode = 0;
//             }
//             else if(target.name == "back")
//             {
//                 OutputLetter((int )VKCode.Back);
//             }
//             else if(target.name =="sp")
//             {
//                 OutputLetter(' ');
//             }
//             else if(target.name == "Enter")
//             {
//                 OutputLetter((int)VKCode.Enter);
//             }

//             if(target.name.Length == 3)
//             {
//                 //Debug.Log("Should print "+mode.ToString()+" "+ target.name[0]);
//             if (mode == 0)
//                 OutputLetter(target.name[0]);
//             if (mode == 1)
//                 OutputLetter(target.name[1]);
//             if (mode == 2)
//                 OutputLetter(target.name[2]); 
//             }
//             else if(target.name.Length == 1)
//             {
//                 OutputLetter(target.name[0]);
//             }
//         }

//         longHolding = false;
//         if(target!=null)
//         target.GetComponent<MeshRenderer>().material.color = Color.white;
//         Alt.SetActive(false);
//         press = false;
//     }


//    /*   override public void OnPressUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
//     {
//        不管是什么键盘，这里都需要输出字符了! 
//        // Debug.LogWarning("PRESS UP ONE");
//        if (target == null) return;
//       // 
//       /* if(longHolding && target.name.Length == 3)
//        {
//             Debug.Log(PadSlide[fromSource].axis[0]+" "+ PressPos[0]);
//            if (PadSlide[fromSource].axis[0]-PressPos[0]>0.05)
//                OutputLetter(target.name[0]);
//            else if (PadSlide[fromSource].axis[0] - PressPos[0] < - 0.05)
//                OutputLetter(target.name[1]);
//            else 
//                OutputLetter(target.name[2]);
            
//        }
//        else
//        {

//            if(target.name == "shift")
//            {

//                switchCapital();
//                if (mode == 0) mode = 1;
//                else if(mode == 1) mode = 0;

//            }
//            else if(target.name =="Symbol")
//            {
//                switchSymbol();
//                if (mode == 0)
//                    mode = 2;
//                else if (mode == 2)
//                    mode = 0;
//            }
//            else if(target.name == "back")
//            {
//                OutputLetter((int )VKCode.Back);
//            }
//            else if(target.name =="sp")
//            {
//                OutputLetter(' ');
//            }
//            else if(target.name == "Enter")
//            {
//                OutputLetter((int)VKCode.Enter);
//            }

//            if(target.name.Length == 3)
//            {
//                //Debug.Log("Should print "+mode.ToString()+" "+ target.name[0]);
//            if (mode == 0)
//                OutputLetter(target.name[0]);
//            if (mode == 1)
//                OutputLetter(target.name[1]);
//            if (mode == 2)
//                OutputLetter(target.name[2]); 
//            }
//            else if(target.name.Length == 1)
//            {
//                OutputLetter(target.name[0]);
//            }
//        }
        
//        longHolding = false;
//        if(target!=null)
//        target.GetComponent<MeshRenderer>().material.color = Color.white;
//        Alt.SetActive(false);
//        press = false;
//    }*/
//    public float dis(float x, float y)//返回点到直线的距离
//    {
//        return Mathf.abs(2 * x + y) / Mathf.sqrt(5);

//    }
//    override public void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
//    {
//        /* 
//         手指在触摸板上滑动。一个是以一定的规则移动改变高亮，一个在按下扳机的时候移动光标.
//         */
//        //得到当前触摸点坐标
//        float x = axis[0];
//        float y = axis[1];
//        float offset = Mathf.sqrt(5) / 75; 
//        Vector2 core1 = new Vector2(1 - 2 * offset, -2 + 4 * offset);
//        Vector2 core2 = new Vector2(1 - 1 * offset, -2 + 2 * offset);
//        Vector2 core3 = new Vector2(1 , -2 );
//        Vector2 core4 = new Vector2(1 + 1 * offset, -2 - 2 * offset);
//        Vector2 core5 = new Vector2(1 + 2 * offset, -2 - 4 * offset);
//        Vector2 cores[5] = { core1, core2, core3, core4, core5 };
//        float len = 100.0;
//        int i = 0;
//        while(len > 5 && i <= 4)
//        {
//            len = Mathf.pow(x - cores[4 - i][1], 2) + Mathf.pow(y - cores[4 - i][y], 2);
//            i = i + 1;
//        }//找到第一个使他小于半径的弧度
//        float dis_ = dis(x, y);
//        //再更细致的判定
//        if(i == 0)
//        {
           
//        }
//        else if(i == 1)
//        {

//        }
//        else if (i == 2)
//        {

//        }
//        else if (i == 3)
//        {

//        }
//        else if (i == 4)
//        {

//        }
//        else if (i == 5)
//        {

//        }



//        if (controller == null) return;

//        if(first == true)
//        {
//            first = false;
//            return;
//        }
        
//        else if(selected == false || Time.time-loose<0.01)
//        {
//            if(target == false) { 
//            Vector3  move = new Vector3(-delta.x*2.8f, 0,-delta.y*2.8f);
//            //映射逻辑
//            Debug.Log(move.magnitude);
//            if (move.magnitude < 1)
//            {
//                controller.transform.localPosition = controller.transform.localPosition + move;
//                last_move = move;
//            }}
//            else
//            {
//                Vector3 move = new Vector3(-delta.x * 2.5f, 0, -delta.y * 2.5f);
//                //映射逻辑
//                Debug.Log(move.magnitude);
//                if (move.magnitude < 1)
//                {
//                    controller.transform.localPosition = controller.transform.localPosition + move;
//                    last_move = move;
//                }
//            }
//          // EndPos
    
//        }
//        else if(selected == true)
//        {   
            
//            if(axis[0]< PressPos[0]-0.05)
//            {   choose = 'l';
//                l.GetComponent<MeshRenderer>().material.color = Color.yellow;
//                r.GetComponent<MeshRenderer>().material = material;
//                m.GetComponent<MeshRenderer>().material = material;
                
//            }
//            else if(axis[0]>PressPos[0]+0.05)
//            {    choose = 'r';
//                r.GetComponent<MeshRenderer>().material.color = Color.yellow;
//                l.GetComponent<MeshRenderer>().material = material;
//                m.GetComponent<MeshRenderer>().material = material;
                
//            }
//            else
//            {   choose = 'm';
//                m.GetComponent<MeshRenderer>().material.color = Color.yellow;
//                r.GetComponent<MeshRenderer>().material = material;
//                l.GetComponent<MeshRenderer>().material = material;
               
//            }
//            //int move = 0;
//            //if (delta[0] > 0)
//            //{
//            //    move =(int) (delta[0] / 2 * 5);
//            //    inputField.caretPosition = inputField.caretPosition + move;
//            //}
//            //else if (delta[0] < 0)
//            //{
//            //    move = (int)((-delta[0]) / 2 * 5);
//            //    inputField.caretPosition = inputField.caretPosition - move;
//            //}
//            //do_caret_move(axis);
//        }

//    }
//}


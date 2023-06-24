
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Valve.VR;
using Valve.VR.InteractionSystem;
using TMPro;

public enum Keybd_Flags
{
    KeyDown,
    KeyHold,
    KeyUp,
}

public class KeyboardBase : MonoBehaviour
{
    // Start is called before the first frame update
    [DllImport("User32.dll", EntryPoint = "keybd_event")]
    static extern void keybd_event(byte bVK, byte bScan, int dwFlags, int dwExtraInfo);

    //Keyboard action set
    public SteamVR_ActionSet keyboardActionSet;

    // fetch actions.
    public SteamVR_Action_Boolean DeleteKey = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "delete");//���˵�����������Ҫɾ��.
    public SteamVR_Action_Boolean SelectKey = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "select");//�����ѡ�����������ƶ����
    public SteamVR_Action_Boolean PadTouch = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "touch");  //touchpad����
    public SteamVR_Action_Boolean PadPress = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("keyboard", "press");  //touchpad����ȥ
    public SteamVR_Action_Vector2 PadSlide = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("keyboard", "slide");  //��touchpad�ϻ���.

    // text input field. �̶������������  
    public TextMeshProUGUI inputField;

    // һЩһ���Ṳ�õ�״̬������selected, deleted, touched. longHolding: ����Ƿ�ʱ�䰴��������������������ַ�ѡ��.
    protected bool selected = false, deleted = false, touched = false, longHolding = false;
    protected float last_delete_time, hold_time_start;   //���������ж�ɾ���ַ���ʱ�����ģ�����delete����ɾ��������Ӧ̫��.

    /*
     * ȷ�����ڼ̳�KeyboardBase֮��ֻ��������̳е��࣬������ֱ��������̳е����ϵ��ú���������ʹ�õĺ�����ȷ���Ǽ̳�������д�ĺ���.
     * ���磬����OnEnableע��ص���������������û�Ķ�����Unity�й��ص������࣬���Ե������������е��ã�û���ø����͵������ຯ���ĵ���.
     */

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnEnable()
    {
        // ע��ص�����
        Debug.Log("Enable keyboard action set and relative callback functions!");
        keyboardActionSet.Activate();
        DeleteKey.onStateDown += OnDeleteKeyDown;
        DeleteKey.onStateUp += OnDeleteKeyUp;
        DeleteKey.onState += OnDeleteKeyHolding;
        SelectKey.onStateDown += OnSelectKeyDown;
        SelectKey.onStateUp += OnSelectKeyUp;
        PadTouch.onStateDown += OnTouchDown;
        PadTouch.onStateUp += OnTouchUp;
        PadPress.onStateUp += OnPressUp;
        PadPress.onStateDown += OnPressDown;
        PadSlide.onChange += OnPadSlide;
    }

    private void OnDisable()
    {
        Debug.Log("Disable keyboard action set!");
        keyboardActionSet.Deactivate();
        DeleteKey.onStateDown -= OnDeleteKeyDown;
        DeleteKey.onStateUp -= OnDeleteKeyUp;
        DeleteKey.onState -= OnDeleteKeyHolding;
        SelectKey.onStateDown -= OnSelectKeyDown;
        SelectKey.onStateUp -= OnSelectKeyUp;
        PadTouch.onStateDown -= OnTouchDown;
        PadTouch.onStateUp -= OnTouchUp;
        PadPress.onStateUp -= OnPressUp;
        PadPress.onStateDown -= OnPressDown;
        PadSlide.onChange -= OnPadSlide;
    }

    // ����ѡ�񡪡��ƶ����Ĺ������м�����һ���ģ�����virtual. ������Щ��������ν�����ֵ�.
    // �ƶ����ʹ��touchpad���Ͳ���Ҫ��pose action�ˣ�
    public void OnSelectKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* �ɿ�Select���Ļص�������Ҫ�ſ���꣩ */
        selected = false;
        //TODO: ���¹�꣨����ʲôҲ���������ɣ�.
    }

    public void OnSelectKeyDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* ����Select����ʼ�ƶ����Ļص�����. */
        selected = true;
    }

    public void OnDeleteKeyUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* �ɿ�ɾ����. */
        deleted = false;
    }

    public void OnDeleteKeyDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* ����ɾ����.��ʼɾ��. */
        deleted = true;
        last_delete_time = Time.time;
        do_delete_char();
    }

    public void OnDeleteKeyHolding(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* һֱ��סDeleteKey, Ҫһֱɾ��������̫�죬����0.2sһ��? onState�Ļص����� */
        // ��ΪonState��ǰ����ǰ���״̬�� true��������ʵ�����ò���deleted.
        if(Time.time - last_delete_time > 0.2f)
        {
            last_delete_time = Time.time;
            do_delete_char();
        }
    }

    // �����Ǻ�touchpad�йص��ˣ���ͬ���̵���Ϊ���ܲ�һ������Ҫvirtual.
    virtual public void OnTouchDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* ������������! */
        touched = true;
    }

    virtual public void OnTouchUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* �ɿ������壬����߼���һ���ˣ�click�����ɿ���������������slide���̲��� */
        touched = false;
    }

    virtual public void OnPressDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* ���´�����ķ���, ���Ӧ��ֻ�� SlideKeyboard ��Ҫ. */
    }

    virtual public void OnPressUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        /* ���´�����֮�����ɿ�����һ����Ҫ����ַ���
         * �������ڴ�Сд��mode�Ĵ��ڣ��Ծ�����Ҫ������������! */        
    }

    virtual public void OnPadSlide(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        /* 
         * �Ƚϸ��ӵ��߼�������ָ��touchpad�ϻ�����ʱ��ע����Ҫ**����ƶ����ʹ�������**! 
         * ����ҪС�ģ���ֻ�ֶ�ֻ��ͨ����һ�����ú���������Ӧ������.
         */

    }

    // �������.
    virtual public int Axis2Letter(Vector2 axis, SteamVR_Input_Sources hand, int mode)
    {
        // ��axis�����ж�Ҫ������һ�� ����Ascii��! ��������Ҫ���������
        // mode: ģʽ��0-��ͨСд��1-��д��������Ÿ��ݾ����߼�ʹ�ã������г���ͬһ����ĸ�������ź����ֵ�ѡ����ѡ��!
        // ��д�����������������ϼ�ʵ��.
        // ����Ҫ��������ʵ��.
        return 0;
    }

    public void OutputLetter(int ascii)
    {
        // ���ascii���Ӧ���ַ���ע��Ҫ���ַ�����ɼ�������������д��Ҫ��ϼ�.
        // TODO: ���ascii�ַ����������Ҫ����Windows�������.
    }
    
    void do_delete_char()
    {
        // ��inputField�У��ڵ�ǰ���λ�ã�ɾ��1���ַ�.
        // TODO. ɾ��һ���ַ�.
    }

    void PushKey(byte bVK)
    {
        // ���°���
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyDown, 0);
    }

    void ReleaseKey(byte bVK)
    {
        // �ɿ�����
        // һ����Ҫ���ǰ�����Ҫ�ɿ�.
        keybd_event(bVK, 0, (int)Keybd_Flags.KeyUp, 0);
    }
}
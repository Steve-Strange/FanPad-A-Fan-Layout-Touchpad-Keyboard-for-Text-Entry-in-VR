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
        text.text = "";
    }
    /* TODO: ʹ�� Pageģʽ��������д���ʱ���Զ���ҳ!
     * ��Ӧ����Ҫ����InputField��һЩ״̬����ʵ�ֻص�����(�������Ӧ���¼��Ļ�).
     */

    // Update is called once per frame
    void Update()
    {

    }
}
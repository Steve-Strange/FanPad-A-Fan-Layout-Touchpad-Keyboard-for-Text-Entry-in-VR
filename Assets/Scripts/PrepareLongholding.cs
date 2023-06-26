using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PrepareLongholding : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI[] texts = new TextMeshProUGUI[3];
    public GameObject[] rects;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void init_chars(char letter, char sym)
    {
        // letter±Ø¶¨Ð¡Ð´.
        texts[0].text = letter.ToString().ToUpper();
        texts[1].text = sym.ToString();
        texts[2].text = letter.ToString();
    }
}

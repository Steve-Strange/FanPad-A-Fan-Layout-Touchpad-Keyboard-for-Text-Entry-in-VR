using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordCubes : MonoBehaviour
{
    TextMeshProUGUI[] words;
    int selectedIndex = -1;
    // Start is called before the first frame update
    void Start()
    {
        int childcount = transform.childCount;
        words = new TextMeshProUGUI[childcount];
        for(int i=0; i<childcount; ++i){
            words[i] = transform.GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setWords(string[] w){
        for(int i=0; i<words.Length; ++i){
            words[i].text = w[i];
        }
    }

    public void setSelectedIndex(int index) { selectedIndex = index; }
    public int getSelectedIndex() { return selectedIndex;}
    public string getSelectedWord() {
        if (selectedIndex != -1)
            return words[selectedIndex].text;
        else
            return string.Empty;
    }
}

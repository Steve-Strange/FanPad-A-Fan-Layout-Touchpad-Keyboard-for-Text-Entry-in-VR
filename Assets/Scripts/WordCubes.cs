using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordCubes : MonoBehaviour
{
    TextMeshProUGUI[] words;
    WordCubeTrigger[] wordCubeTriggers;
    int selectedIndex = -1;
    // Start is called before the first frame update
    void Start()
    {
        int childcount = transform.childCount;
        words = new TextMeshProUGUI[childcount];
        wordCubeTriggers = new WordCubeTrigger[childcount];
        for(int i=0; i<childcount; ++i){
            Transform child = transform.GetChild(i);
            int ind = child.name[child.name.Length - 1] - '1';
            words[ind] = child.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            wordCubeTriggers[ind] = child.GetComponent<WordCubeTrigger>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setWords(string[] w){
        for(int i=0; i<words.Length; ++i){
            words[i].text = w[i];
            if(w[i].Length == 0 && selectedIndex == i)  // 这个单词被刷新为空了，但是这里原本被选中，可能处于高亮状态，需要关闭高亮.
                wordCubeTriggers[i].highlight(false);
        }
    }

    public void selectbyRay(int index){
        if(index == selectedIndex && index >=0 && index < wordCubeTriggers.Length){
            wordCubeTriggers[selectedIndex].highlight(true);
            return;
        }
        if(index != -1){
            if(selectedIndex != -1){
                wordCubeTriggers[selectedIndex].highlight(false);
            }
            selectedIndex = index;
            wordCubeTriggers[selectedIndex].highlight(true);
        }
        else if(index >=0 && index < wordCubeTriggers.Length){
            wordCubeTriggers[selectedIndex].highlight(false);
            selectedIndex = -1;
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

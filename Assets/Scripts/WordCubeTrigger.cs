using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 不再是触发器，而是一个单纯的在单词cube层级的控制器，有本单词块的信息...
public class WordCubeTrigger : MonoBehaviour
{
    public int index;
    public Color selectedColor = new Color(255, 255, 0, 30);  //被选中的高亮颜色.
    WordCubes wordCubes;
    Color oldColor;
    MeshRenderer meshRenderer;
    TextMeshProUGUI selfWord;
    // Start is called before the first frame update
    void Start()
    {
        wordCubes = transform.parent.GetComponent<WordCubes>();
        index = gameObject.name[gameObject.name.Length - 1] - '1';
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        oldColor = meshRenderer.material.color;
        selfWord = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void highlight(bool on){
        // 如果本块没有单词，无论如何不高亮!
        if(on && selfWord.text.Length > 0)
            meshRenderer.material.color = selectedColor;
        else
            meshRenderer.material.color = oldColor;
    }



    // private void OnTriggerEnter(Collider other)
    // {
    //     Debug.LogWarning("WordCube Triggered!");
    //     if(wordCubes.getSelectedIndex() != index)  
    //         wordCubes.setSelectedIndex(index);
    //     // 高亮
    //     //if(selfWord.text.Length>0)
    //         meshRenderer.material.color = selectedColor;
    // }
    // private void OnTriggerExit(Collider other)
    // {
    //     Debug.LogWarning("WordCube not Triggered!");
    //     if(wordCubes.getSelectedIndex() == index)
    //         wordCubes.setSelectedIndex(-1);
    //     // 取消高亮.
    //     //if(meshRenderer.material.color != oldColor)
    //         meshRenderer.material.color = oldColor;
    // }
}

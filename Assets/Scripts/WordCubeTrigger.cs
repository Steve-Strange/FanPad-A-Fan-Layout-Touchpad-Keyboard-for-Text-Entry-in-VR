using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordCubeTrigger : MonoBehaviour
{
    public int index;
    public Color selectedColor = new Color(255, 255, 0, 30);  //被选中的高亮颜色.
    WordCubes wordCubes;
    Color oldColor;
    MeshRenderer meshRenderer;
    // Start is called before the first frame update
    void Start()
    {
        wordCubes = transform.parent.GetComponent<WordCubes>();
        index = gameObject.name[gameObject.name.Length - 1] - '1';
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        oldColor = meshRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(wordCubes.getSelectedIndex() != index)  
            wordCubes.setSelectedIndex(index);
        // 高亮
        meshRenderer.material.color = selectedColor;
    }
    private void OnTriggerExit(Collider other)
    {
        if(wordCubes.getSelectedIndex() == index)
            wordCubes.setSelectedIndex(-1);
        // 取消高亮.
        meshRenderer.material.color = oldColor;
    }
}

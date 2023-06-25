using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class follow : MonoBehaviour
{
    public GameObject followPrefab;
    private Vector3 dis = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        dis = transform.position - followPrefab.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        followPrefab.transform.position = transform.position - dis;
    }
}

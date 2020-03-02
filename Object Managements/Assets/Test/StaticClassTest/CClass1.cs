using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CClass1 : FClass
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        Debug.Log("CClass1被启用了");
    }
}

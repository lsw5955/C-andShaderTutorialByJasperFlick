using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        Debug.Log("我被MouseDown输出了野");
    }

    public void MouseDown()//在PointerDown类的EventTrigger中添加了该函数
    {
        Debug.Log("我被事件输出了");
    }

}

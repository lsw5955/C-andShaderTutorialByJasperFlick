using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour{
    public TestScriptableObject ts;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) {
            ts.S = "我被搞了";
            ts.SS = "我也被搞了";
            ts.KKB = "变了变了";
        }
        else if (Input.GetKeyDown(KeyCode.F2)) {
            Debug.Log(ts.S);
            Debug.Log(ts.SS);
            Debug.Log(ts.KKB);
        }
    }


    private void OnBecameVisible()
    {
        Debug.Log("我出去了 嘻嘻 我又进来了 你打我啊笨蛋!");
    }

    private void OnBecameInvisible()
    {
        Debug.Log("我又出去了嘻嘻");
    }
}

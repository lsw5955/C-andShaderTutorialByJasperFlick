using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualDotProduct : MonoBehaviour {
    //夹角
    public float angel;

    //向量长度
    public float vLength1, vLength2;
    //LineRenderer
    public LineRenderer lr1, lr2,lr3;
    //两条线所代表的向量
    Vector3 line1Vector, line2Vector;

    //点积
    float dot;

    void Initialize() {
        //设置lineRenderer顶点数量
        lr1.positionCount = 2;
        lr2.positionCount = 2;
        //设置lineRenderer顶点坐标
        line1Vector = new Vector3(vLength1, 0, 0);
        line2Vector = new Vector3(vLength2 * Mathf.Cos(angel * Mathf.Deg2Rad), vLength2 * Mathf.Sin(angel * Mathf.Deg2Rad), 0);
        lr1.SetPositions(new Vector3[] { Vector3.zero, line1Vector });
        lr2.SetPositions(new Vector3[] { Vector3.zero, line2Vector });
    }

    /// <summary>
    /// 打印和返回点积结果
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    float Dot(Vector3 v1, Vector3 v2) {
        dot = Vector3.Dot(v1, v2);
        Debug.Log(v1 + "和" + v2 + "的点积 = " + dot);
        return dot;
    }

    /// <summary>
    /// 点积乘以line2是个啥呢...
    /// </summary>
    void WhatRUFckDoing() {
        Vector3 temp = line2Vector * dot;
        lr3.positionCount = 2;
        lr3.SetPositions(new Vector3[] { Vector3.zero, temp });
        
        Debug.Log("line2Vector * Dot = " + temp + "新向量与内个向量的夹角是" + Vector3.Angle(temp, line1Vector - temp));
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Initialize();
            Dot(line1Vector, line2Vector);
            WhatRUFckDoing();
        }
    }    
}

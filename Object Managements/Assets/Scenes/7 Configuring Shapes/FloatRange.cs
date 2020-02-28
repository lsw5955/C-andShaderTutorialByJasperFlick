using UnityEngine;

[System.Serializable]
//代表特定范围内小数的结构类型
public struct FloatRange {
    //最大值和最小值
    public float min, max;

    //该属性可以获得最大值和最小值之间的随机数
    public float RandomValueInRange {
        get {
            return Random.Range(min, max);
        }
    }
}
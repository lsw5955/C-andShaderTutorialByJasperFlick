using UnityEngine;

//创建一个为FloatRange类型值使用的滑动条特性
public class FloatRangeSliderAttribute : PropertyAttribute {
    //代表滑动条最小值的属性
    public float Min { get; private set; }
    //代表滑动条最大值的属性
    public float Max { get; private set; }

    //初始化Min和Max的构造函数
    public FloatRangeSliderAttribute(float min, float max)
    {
        //如果参数max小于参数min, 则让max至少等于min
        if (max < min) {
            max = min;
        }
        //初始化Min属性
        Min = min;
        //初始化Max属性
        Max = max;
    }
}
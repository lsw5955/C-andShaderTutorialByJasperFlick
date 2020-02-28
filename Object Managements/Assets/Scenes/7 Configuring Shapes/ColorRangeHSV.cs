using UnityEngine;

[System.Serializable]
//用于配置随机颜色范围的结构
public struct ColorRangeHSV {
    //[Range(0f,1f)]
    //使用自定义特性约束FloatRange类型字段在Inspector中的取值范围
    //Unity的特殊规则, 使用Attribute结尾的特性名称, 使用的时候可以省略这个字符串
    //此处其实就是在调用我们为自定义特性类写的的构造函数
    [FloatRangeSlider(0f, 1f)]
    //使用三个FloatRange字段分别代表颜色的色度范围,饱和度范围和明度范围
    public FloatRange hue, saturation, value;

    //获取对应范围内的随机颜色
    public Color RandomInRange {
        get {
            return Random.ColorHSV(
                hue.min, hue.max,
                saturation.min, saturation.max,
                value.min, value.max,
                1f, 1f
            );
        }
    }
}
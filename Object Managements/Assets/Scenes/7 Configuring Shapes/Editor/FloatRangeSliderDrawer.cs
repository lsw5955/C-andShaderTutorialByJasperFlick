using UnityEditor;
using UnityEngine;

//该特性将typeof()中书写的类型作为参数, 用来指定要在Inspector中自定义哪一种类型属性的显示方式
[CustomPropertyDrawer(typeof(FloatRangeSliderAttribute))]
public class FloatRangeSliderDrawer : PropertyDrawer {
    //重写OnGUI方法, 进行自定义UI的绘制
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //记录下原始的缩进值
        int originalIndentLevel = EditorGUI.indentLevel;
        //告诉编辑器要开始绘制属性自定义UI了
        EditorGUI.BeginProperty(position, label, property);
        //绘制标签, 并使用position存储绘制后的剩余区域
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        //设置接下来绘制内容的缩进
        EditorGUI.indentLevel = 0;
        //得到代表min属性的SerializedProperty对象
        SerializedProperty minProperty = property.FindPropertyRelative("min");
        //得到代表max属性的SerializedProperty对象
        SerializedProperty maxProperty = property.FindPropertyRelative("max");
        //取出min值, 用于设置滑动条滑块
        float minValue = minProperty.floatValue;
        //取出max值, 用于设置滑动条滑块
        float maxValue = maxProperty.floatValue;
        //计算出输入框要占据的宽度, 存储到变量中, 这里的减4就是减去4像素宽度
        float fieldWidth = position.width / 4f - 4f;
        //计算出滑动条要占据的宽度, 存储到变量中
        float sliderWidth = position.width / 2f;
        //将绘制完左侧标签的剩余宽度划分为三份, 
        //这三份区域准备依次绘制第一个输入框, 滑动条, 和第二个输入框
        //position.width /= 3;
        //使用fieldWidth变量设置输入框绘制的宽度
        position.width = fieldWidth;
        //绘制显示minValue的输入框, 输入框的值改变后会再赋值给minValue
        minValue = EditorGUI.FloatField(position, minValue);
        //为绘制滑动条而移动绘制位置的x坐标
        //position.x += position.width;
        //绘制滑动条的x坐标偏移应该在输入框宽度基础上再加4像素
        position.x += fieldWidth + 4f;
        //使用sliderWidth变量设置滑动条绘制的宽度
        position.width = sliderWidth;
        //通过attribute得到要绘制的属性的特性, 需要将其转换为要使用的自定义特性的类型
        FloatRangeSliderAttribute limit = attribute as FloatRangeSliderAttribute;
        EditorGUI.MinMaxSlider(
            //绘制滑动条的position 
            position,
            //滑动条的属性标签 
            //删除标签参数, 不通过该方法绘制标签
            //label, 
            //滑动条左滑块指示的值, ref表示引用传递, 这样方法内修改该参数即改变了minValue变量的值
            ref minValue,
            //滑动条左滑块指示的值, ref表示引用传递, 这样方法内修改该参数即改变了maxValue变量的值
            ref maxValue,
            //滑动条的最小值 
            limit.Min,
            //滑动条的最大值
            limit.Max
         );
        //为绘制第二个输入框而移动绘制位置的x坐标
        //position.x += position.width;
        //绘制第二个输入框的x坐标偏移应该是滑动条宽度再加4像素
        position.x += sliderWidth + 4f;
        //使用fieldWidth变量设置输入框绘制的宽度
        position.width = fieldWidth; 
        //绘制显示maxValue的输入框, 输入户口的值改变后会再复制给maxValue
        maxValue = EditorGUI.FloatField(position, maxValue);
        //如果输入的最小值小于滑块支持的最小值, 则将其设置为滑块最小值
        if (minValue < limit.Min) {
            minValue = limit.Min;
        }
        //如果输入的最大值小于输入的最小值, 则最大值设置月最小值相等
        if (maxValue < minValue) {
            maxValue = minValue;
        }
        //如果输入的最大值大于滑块支持的最大值, 则将其设置为滑块最大值
        else if (maxValue > limit.Max) {
            maxValue = limit.Max;
        }
        //用于在滑动条变化后更新min属性值
        minProperty.floatValue = minValue;
        //用于在滑动条变化后更新max属性值
        maxProperty.floatValue = maxValue;
        //告诉编辑器属性自定义UI绘制完毕了
        EditorGUI.EndProperty();
        //还原缩进距离为原始值
        EditorGUI.indentLevel = originalIndentLevel;
    }
}
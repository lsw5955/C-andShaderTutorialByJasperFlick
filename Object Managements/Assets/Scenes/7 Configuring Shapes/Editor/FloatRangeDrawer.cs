//与编辑器操作有关的内容都需要引用该命名空间
using UnityEditor;
using UnityEngine;

//该特性将typeof()中书写的类型作为参数, 用来指定要在Inspector中自定义哪一种类型属性的显示方式
[CustomPropertyDrawer(typeof(FloatRange))]
//用来绘制FloatRange类型成员在Inspector中的显示方式
public class FloatRangeDrawer : PropertyDrawer {
    //重写OnGUI方法, 进行自定义UI的绘制
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //记录UI绘制前的原始缩进
        int originalIndentLevel = EditorGUI.indentLevel;
        //记录UI绘制前的原始标签宽度
        float originalLabelWidth = EditorGUIUtility.labelWidth;
        //告诉编辑器要开始绘制属性自定义UI了
        EditorGUI.BeginProperty(position, label, property);
        //在position位置处显示label代表的标签内容, 并将标签显示后剩余的可绘制区域信息存储到position中
        //position = EditorGUI.PrefixLabel(position, label);
        //代码太长, 参数分行写清晰点
        //增加了第二个参数, 范围名称标签指定了一个与min不同的控制标签,这样二者不会一同被高亮选中
        //FocusType.Passive保障了该标签文字也不能单独被选中, 选中它不是错误, 但没有意义
        position = EditorGUI.PrefixLabel(
            position,
            GUIUtility.GetControlID(FocusType.Passive),
            label
            );
        //将position中代表UI显示宽度的width属性减半, 使用它作为位置参数进行绘制的UI就只占一半宽度
        position.width = position.width / 2f;
        //在position的宽度减半之后, 使用减半之后的宽度再减半的值设置标签宽度
        //注意, 这不止设置的是最左边的范围名称的标签宽度, 也是设置了Min和Max这俩文字的宽度, 它俩也是标签
        EditorGUIUtility.labelWidth = position.width / 2f;
        //在这之后绘制的标签缩进都会被设置为1
        EditorGUI.indentLevel = 1;
        //Debug.Log("最开始宽度是" + originalLabelWidth + "结束前宽度是 :" + EditorGUIUtility.labelWidth);
        //PropertyField方法, 在参数1代表的位置处, 绘制参数2代表的属性内容
        EditorGUI.PropertyField(position, property.FindPropertyRelative("min"));
        //将position中代表UI横坐标的x属性加上偏移值, 使用它作为位置参数进行绘制的UI就会向右偏移指定距离
        position.x += position.width;
        //与min一样绘制max
        EditorGUI.PropertyField(position, property.FindPropertyRelative("max"));
        //告诉编辑器属性自定义UI绘制完毕了
        EditorGUI.EndProperty();
        //不恢复原始缩进和原始标签宽度对于显示效果没有任何影响, 但很可能会影响以后其他UI的绘制, 按照教程一样加上吧
        //UI绘制完成后, 恢复原始缩进
        EditorGUI.indentLevel = originalIndentLevel;
        //UI绘制完成后, 恢复原始标签宽度
        EditorGUIUtility.labelWidth = originalLabelWidth;
        //Debug.Log("结束后宽度是 :" + EditorGUIUtility.labelWidth);
    }
}
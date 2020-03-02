using UnityEngine;

//所有形状行为脚本组件的抽象基类 
//public abstract class ShapeBehavior : MonoBehaviour {
//不再继承MonoBehaviour类, 不再是一种Unity组件类
//为了在运行时重编译后可以保持行为的实例, 让它们继承ScriptableObject
public abstract class ShapeBehavior
    //带有#的#if语句, 叫做条件编译指令, 顾名思义, 在满足指定条件后才会编译被它包围的代码
    // UNITY_EDITOR 就代表了在编辑器环境下这个前提条件
    //仅编辑器环境下才需要以下代码解决热重载后的回收行为的引用丢失问题
#if UNITY_EDITOR
    : ScriptableObject
#endif
    {
    //该抽象方法主要用于更新形状行为的状态, abstract, 前面教程提到过, 声明"抽象"的关键字
    //shape参数用来指定要做出该行为的形状
    public abstract void GameUpdate(Shape shape);
    //定义保存行为配置与状态数据的抽象方法
    public abstract void Save(GameDataWriter writer);
    //定义加载行为配置与状态数据的抽象方法
    public abstract void Load(GameDataReader reader);
    //定义用于得到代表行为组件类型的枚举值的抽象属性
    public abstract ShapeBehaviorType BehaviorType { get; }
    //定义用来回收行为的抽象方法
    public abstract void Recycle();
    //仅编辑器环境下才需要以下代码解决热重载后的回收行为的引用丢失问题
#if UNITY_EDITOR
    //标记一个行为是否应该在回收池内的属性, true表示该行为应该在回收池内
    public bool IsReclaimed { get; set; }
    private void OnEnable()
    {
        Debug.Log($"我输出我自己{this}");
        //如果行为发现自身是被回收状态, 则将自己放入回收池
        if (IsReclaimed) {
            Recycle();
        }        
    }
#endif
    static ShapeBehavior()
    {
        Debug.Log("ShapeBehavior.count被创建了");
    }
}
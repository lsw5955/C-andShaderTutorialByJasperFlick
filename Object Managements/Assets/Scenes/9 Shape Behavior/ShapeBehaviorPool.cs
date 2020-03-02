using System.Collections.Generic;
using UnityEngine;

//代表行为回收池的静态类
public static class ShapeBehaviorPool<T> where T : ShapeBehavior, new() {
    static ShapeBehaviorPool()
    {
        Debug.Log($"{typeof(T)}的回收池创建了啊 ");
    }

    //存储被回收的行为的引用的"堆"
    public static Stack<T> stack = new Stack<T>();
    //获取一个行为
    public static T Get()
    {
        if (stack.Count > 0) {
            //return stack.Pop();
            //使用变量存储出栈的行为, 以便设置了IsReclaimed后再返回它
            T behavior = stack.Pop();

            //仅编辑器环境下才需要以下代码解决热重载后的回收行为的引用丢失问题
#if UNITY_EDITOR
            //行为本身通过IsReclaimed决定热重载后是否重新加入回收池, false表示不加入
#endif
            behavior.IsReclaimed = false;
            return behavior;
        }
        //return new T();
        //Unity不赞成我们调用任何继承自Object类的构造方法, 手动调用可能带来问题
        //所以使用Unity推荐的实例化方法来对类实例化
        return ScriptableObject.CreateInstance<T>();
    }
    //回收一个行为
    public static void Reclaim(T behavior)
    {
        //仅编辑器环境下才需要以下代码解决热重载后的回收行为的引用丢失问题
#if UNITY_EDITOR
        //行为本身通过IsReclaimed决定热重载后是否重新加入回收池, true表示加入
        behavior.IsReclaimed = true;
#endif
        stack.Push(behavior);
    }
}
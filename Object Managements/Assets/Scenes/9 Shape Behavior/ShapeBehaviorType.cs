//代表每一种行为组件类型的枚举
public enum ShapeBehaviorType {
    //代表移动行为组件
    Movement,
    //代表旋转行为组件
    Rotation,
    //代表震荡行为代号的枚举
    Oscillation
}

public static class ShapeBehaviorTypeMethods {
    //this关键字表示这是一个ShapeBehaviorType类型的扩展方法
    public static ShapeBehavior GetInstance(this ShapeBehaviorType type)
    {
        {
            //Shape.AddBehavior中的代码全部复制过来, 略加修改
            switch (type) {
                //添加移动行为
                case ShapeBehaviorType.Movement:
                    //return语句执行后会直接结束方法, 不会继续执行后面的代码
                    //添加与参数类型对应的行为组件, 并返回添加的行为组件引用
                    //return AddBehavior<MovementShapeBehavior>();
                    //使用回收池的Get方法获得指定类型的行为实例
                    return ShapeBehaviorPool<MovementShapeBehavior>.Get();
                //添加旋转行为
                case ShapeBehaviorType.Rotation:
                    //添加与参数类型对应的行为组件, 并返回添加的行为组件引用
                    //return AddBehavior<RotationShapeBehavior>();
                    //使用回收池的Get方法获得指定类型的行为实例
                    return ShapeBehaviorPool<RotationShapeBehavior>.Get();
                //添加震荡行为
                case ShapeBehaviorType.Oscillation:
                    return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
            }
            //如过代码能执行到这里, 说明没有添加与参数对应类型的行为组件, 输出提示文字
            //在不引入命名空间UnityEngine的情况下, 调用Debug类时要把命名空间也写上
            UnityEngine.Debug.LogError("该行为脚本组件未进行处理, 请检查Shape.AddBehavior代码 : " + type);
            //返回null, 表示组件添加失败
            return null;
        }
    }
}
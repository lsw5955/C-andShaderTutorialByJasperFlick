using UnityEngine;

//实现移动行为的脚本组件, 继承抽象类ShapeBehavior
//sealed关键字, 顾名思义, 这个类被"密封"住了, 其他类不可以继承它
public sealed class MovementShapeBehavior : ShapeBehavior {
    //代表移动速度
    public Vector3 Velocity { get; set; }

    //将自身放入行为回收池
    public override void Recycle()
    {
        //回收该行为到与自身类型对应的回收池
        ShapeBehaviorPool<MovementShapeBehavior>.Reclaim(this);
    }

    //更新行为状态
    public override void GameUpdate(Shape shape)
    {
        //每次执行根据速度值更新形状的位置
        shape.transform.localPosition += Velocity * Time.deltaTime;
    }
    //保存方法
    public override void Save(GameDataWriter writer)
    {
        //写入速度值
        writer.Write(Velocity);
    }
    //加载方法
    public override void Load(GameDataReader reader)
    {
        //读取速度值
        Velocity = reader.ReadVector3();
    }
    //获得代表当前行为组件类型的枚举值
    public override ShapeBehaviorType BehaviorType {
        get {
            return ShapeBehaviorType.Movement;
        }
    }

    //测试用
    //private void OnEnable()
    //{
    //    Debug.Log("我是movementBehavior");
    //}
}
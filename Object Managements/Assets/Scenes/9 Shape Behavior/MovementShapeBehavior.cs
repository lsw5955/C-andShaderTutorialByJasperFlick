using UnityEngine;

//实现移动行为的脚本组件, 继承抽象类ShapeBehavior
public class MovementShapeBehavior : ShapeBehavior {
    //代表移动速度
    public Vector3 Velocity { get; set; }

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
}
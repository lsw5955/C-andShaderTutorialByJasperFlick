using UnityEngine;

//实现旋转行为的脚本组件, 继承抽象类ShapeBehavior
public class RotationShapeBehavior : ShapeBehavior {
    //代表旋转角速度
    public Vector3 AngularVelocity { get; set; }
    //更新行为状态
    public override void GameUpdate(Shape shape)
    {
        //每次执行根据角速度值设置形状的旋转角度
        shape.transform.Rotate(AngularVelocity * Time.deltaTime);
    }
    //保存方法
    public override void Save(GameDataWriter writer)
    {
        //写入角速度值
        writer.Write(AngularVelocity);
    }
    //加载方法
    public override void Load(GameDataReader reader)
    {
        //读取角速度值
        AngularVelocity = reader.ReadVector3();
    }
}
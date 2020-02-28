    using UnityEngine;

//该特性将保障一个游戏对象只能添加一个该脚本
[DisallowMultipleComponent]
/// <summary>
/// cube用来实现对自身数据保存和加载的脚本
/// </summary>
public class PersistableObject : MonoBehaviour {
    //数据保存方法, 传入一个GameDataWriter类型的参数
    //public void Save (GameDataWriter writer)
    //增加virtual关键字后, 表示可以在子类中重写该方法
    public virtual void Save(GameDataWriter writer)
    {
        //依次写入自身transform的位置, 旋转和缩放数据
        writer.Write(transform.localPosition);
        writer.Write(transform.localRotation);
        writer.Write(transform.localScale);
    }

    //数据加载方法, 传入一个GameDataReader类型的参数
    //public void Load (GameDataReader reader) {
    //增加virtual关键字后, 表示可以在子类中重写该方法
    public virtual void Load(GameDataReader reader)
    {
        //依次加载自身transform的位置, 旋转和缩放数据
        transform.localPosition = reader.ReadVector3();
        transform.localRotation = reader.ReadQuaternion();
        transform.localScale = reader.ReadVector3();
    }
}
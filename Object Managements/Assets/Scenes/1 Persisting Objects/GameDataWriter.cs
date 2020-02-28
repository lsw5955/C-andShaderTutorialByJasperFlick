using System.IO;
using UnityEngine;

/// <summary>
/// 自定义的游戏数据写入类
/// </summary>
public class GameDataWriter {
    BinaryWriter writer;

    //向保存文件中写入随机状态值的Write方法, 随机状态数据在Unity中的类型是Random.State
    public void Write(Random.State value)
    {
        //将当前运行时的随机状态信息转换为JSON格式的字符串, 打印到控制台观察下
        //Debug.Log(JsonUtility.ToJson(value));
        //将运行时的随机状态数据转换为JSON字符串, 并写入到保存文件中
        writer.Write(JsonUtility.ToJson(value));
    }

    //可以接收Color类型参数的Write方法
    public void Write(Color value)
    {
        //向文件写入颜色数据的红色值
        writer.Write(value.r);
        //向文件写入颜色数据的绿色值
        writer.Write(value.g);
        //向文件写入颜色数据的蓝色值
        writer.Write(value.b);
        //向文件写入颜色数据的透明通道值
        writer.Write(value.a);
    }

    /// <summary>
    //自定义的带有BinaryWriter参数的构造方法
    /// </summary>
    /// <param name="writer"></param>
    public GameDataWriter(BinaryWriter writer)
    {
        //this关键字将明确定义它后面书写的名称代表字段
        this.writer = writer;
    }

    
    /// <summary>
    //写入浮点数的方法
    /// </summary>
    /// <param name="value">要存储的数据</param>
    public void Write(float value)
    {
        //调用writer的Write方法
        writer.Write(value);
    }

    /// <summary>
    //写入整型数字的方法
    /// </summary>
    /// <param name="value">要存储的数据</param>   
    public void Write(int value)
    {
        //调用writer的Write方法
        writer.Write(value);
    }

    /// <summary>
    //写入四元数数据的方法, 四元数顾名思义, 需要四个数字进行定义, 分别是x,y,z,w分量
    /// </summary>
    /// <param name="value"></param>
    public void Write(Quaternion value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
        writer.Write(value.w);
    }
    
    /// <summary>
    //写入三维向量数据的方法, 三维向量, 三个分量, x,y,z
    /// </summary>
    /// <param name="value"></param>
    public void Write(Vector3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }
}
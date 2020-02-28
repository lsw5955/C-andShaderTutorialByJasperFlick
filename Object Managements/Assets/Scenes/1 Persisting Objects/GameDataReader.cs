using System.IO;
using UnityEngine;
public class GameDataReader {
    //该属性只有get方法, 没有set方法, 也就是它只能读取, 不能设置, 即它是只读(read-only)属性
    public int Version { get; }

    //用于获取读取数据流的字段
    BinaryReader reader;
    
    //从保存文件中读取随机状态值的方法
    public Random.State ReadRandomState()
    {
        //return Random.state;
        //使用JsonUtility.FromJson方法将读取到的JSON数据字符串转换为程序中的数据值
        //return JsonUtility.FromJson(reader.ReadString());
        //使用泛型机制来为FromJson方法指定要返回的数据类型, 此处需要返回Random.State类型的值
        return JsonUtility.FromJson<Random.State>(reader.ReadString());
    }

    //可以读取颜色数据的方法
    public Color ReadColor()
    {
        //该变量用于存储读取的到颜色数据
        Color value;
        //读取颜色数据的红色值
        value.r = reader.ReadSingle();
        //读取颜色数据的绿色值
        value.g = reader.ReadSingle();
        //读取颜色数据的蓝色值
        value.b = reader.ReadSingle();
        //读取颜色数据的透明通道值
        value.a = reader.ReadSingle();
        //返回读取到的颜色数据
        return value;
    }

    //构造方法, 传入一个BinaryReader参数
    //public GameDataReader (BinaryReader reader) {
    //原有的构造方法增加一个代表版本号的参数version
    public GameDataReader(BinaryReader reader, int version)
    {
        //this关键字将明确定义它后面书写的名称代表当前类的字段
        this.reader = reader;
        //在构造方法中可以为只读属性赋值
        this.Version = version;
    }

    //读取浮点数据的方法
    public float ReadFloat()
    {
        return reader.ReadSingle();
    }

    //读取整型数据的方法
    public int ReadInt()
    {
        return reader.ReadInt32();
    }

    //读取四元数数据的方法
    public Quaternion ReadQuaternion()
    {
        Quaternion value;
        //读取每个分量数据的顺序与写入方法的顺序一样
        value.x = reader.ReadSingle();
        value.y = reader.ReadSingle();
        value.z = reader.ReadSingle();
        value.w = reader.ReadSingle();
        return value;
    }
    //读取Vector3向量的方法
    public Vector3 ReadVector3()
    {
        Vector3 value;
        //读取每个分量数据的顺序与写入方法的顺序一样
        value.x = reader.ReadSingle();
        value.y = reader.ReadSingle();
        value.z = reader.ReadSingle();
        return value;
    }
}
using System.IO;
using UnityEngine;

//管理保存和加载功能的类
public class PersistentStorage : MonoBehaviour {
    //用来存放保存文件路径字符串
    string savePath;

    void Awake()
    {
        //初始化保存文件路径字符串
        savePath = Path.Combine(Application.persistentDataPath, "saveFile");
    }

    //参数代表要被保存的cube的PersistableObject脚本实例
    //public void Save (PersistableObject o) {
    //Save方法新增代表版本号的第二个参数
    public void Save(PersistableObject o, int version)
    {
        using (
            //获取保存文件写入数据流
            var writer = new BinaryWriter(File.Open(savePath, FileMode.Create))
        ) {
            //控制台输出保存路径, 便于查看存档文件路径
            Debug.Log("保存路径在 :" + savePath);
            //向文件写入版本号数据
            writer.Write(-version);
            //调用方法参数o的保存方法, 将当前文件写入数据流作为参数传递
            o.Save(new GameDataWriter(writer));
        }
    }

    //参数代表要被加载的cube的PersistableObjet脚本实例
    public void Load(PersistableObject o)
    {
        //using (
        //    //获取文件加载数据流
        //    var reader = new BinaryReader(File.Open(savePath, FileMode.Open))
        //) {
        //    //调用方法参数o的加载方法, 将当前文件加载数据流作为参数传递
        //    //o.Load(new GameDataReader(reader));
        //    //调用GameDataReader的构造函数时, 读取文件中的第一个整数数据, 作为版本号参数通过第二个参数传递进去
        //    o.Load(new GameDataReader(reader, -reader.ReadInt32()));
        //}
        //不再打开文件流逐条读取数据, 而是一次性的将文件的全部数据粗存到一个byte数组, 所以也不再需要处理文件流是否正确关闭的问题
        byte[] data = File.ReadAllBytes(savePath);
        //通过MemoryStream, 将data数组转换为一个Stream类型的值
        var reader = new BinaryReader(new MemoryStream(data));
        //调用参数o的Load方法, 加载游戏数据
        o.Load(new GameDataReader(reader, -reader.ReadInt32()));
    }
}
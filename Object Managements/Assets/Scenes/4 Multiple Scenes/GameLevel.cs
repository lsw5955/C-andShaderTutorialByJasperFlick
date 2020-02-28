using UnityEngine;

//专门用于将关卡场景的生成区管理到主场景的Game脚本的SpawnZoneOfLevel属性的脚本组件
//public class GameLevel : MonoBehavior {
//继承PersistableObject类来实现保存和加载方法
public class GameLevel : PersistableObject {
    [SerializeField]
    //该数组用于指定要参与游戏保存的关卡内容
    PersistableObject[] persistentObjects = null;

    //用来存储当前游戏关卡中的GameLevel脚本实例
    public static GameLevel Current { get; private set; }

    [SerializeField]
    //该字段用来存储该组件所在场景的生成区引用
    SpawnZone spawnZone = null;

    ////该属性用于获取当前关卡生成区的一个位置
    //该属性功能已被ConfigureSpawn方法替代
    //public Vector3 SpawnPoint {
    //    get {
    //        return spawnZone.SpawnPoint;
    //    }
    //}

    //配置关卡形状的方法
    //public void ConfigureSpawn(Shape shape) {
    //ConfigureSpawn方法改名
    public Shape SpawnShape()
    {
        //将需要进行配置的形状传递给关卡生成区的SpawnZone.ConfigureSpawn方法
        //spawnZone.ConfigureSpawn(shape);
        //返回生成区产生的形状
        return spawnZone.SpawnShape();
    }

    //重写父类的Save方法
    public override void Save(GameDataWriter writer)
    {
        //数组长度代表了该关卡总共保存了几个对象, 将其写入保存文件
        writer.Write(persistentObjects.Length);
        //遍历persistentObjects数组
        for (int i = 0; i < persistentObjects.Length; i++) {
            //调用persistentObjects数组中每个元素的Save方法
            persistentObjects[i].Save(writer);
        }
    }
    //重写父类的Load方法
    public override void Load(GameDataReader reader)
    {
        //先得到文件中该关卡保存对象的数量
        int savedCount = reader.ReadInt();
        //根据读取到的保存对象数量, 遍历persistentObjects数组,.为每个数组元素加载保存数据
        for (int i = 0; i < savedCount; i++) {
            persistentObjects[i].Load(reader);
        }
    }

    //GameLevel不再需要负责为Game单例的SpawnZoneOfLevel属性赋值
    //void Start()
    //{
    //    //Start方法会在同时加载的所有其他脚本的Awake方法和OnEnable方法执行之后才执行
    //    //在这里进行生成区的关联可以保障代码所需的游戏对象已经全部初始化完成
    //    Game.Instance.SpawnZoneOfLevel = spawnZone;
    //}

    //脚本启用时, 设置Current属性为当前脚本实例的引用
    void OnEnable()
    {
        Current = this;
    }
}
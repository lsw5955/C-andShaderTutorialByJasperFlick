using UnityEngine;

//用于定义复合类型的生成区的脚本组件
public class CompositeSpawnZone : SpawnZone {
    [SerializeField]
    //控制复合生成区如何选择数组中的基本生成区, 为ture表示依照次序循环选择基本生成区, false表示随机选择
    bool sequential = false;

    //在使用有序选择基本生成区的方式时, 该字段用来追踪需要选择的下一个生成区数组索引
    int nextSequentialIndex;

    [SerializeField]
    //存储构成复合生成区所需的基本生成区的数组
    SpawnZone[] spawnZones = null;

    //复合生成区的SpawnPoint属性, 依然需要重写父类的抽象属性
    public override Vector3 SpawnPoint {
        //复合生成区的该属性代码将全部被复制到下面的ConfigureSpawn方法中, 
        //该属性已经没有其他代码调用了, 所以可以全部注释或删除掉
        //但是因为父类的该属性是抽象的, 必须要实现get方法, 所以可以写成下面的默认形式
        //不注释或者不删除也不会出错, 这个选择留给你自己, 我去看了原作者的源代码, 他倒是也没注释
        get {
            //以下代码, 我还以为没用了, 具体看上面的几行注释, 做到后面才发现, 注释掉, 调用base.ConfigureSpawn时候会煞笔
            int index;
            if (sequential) {
                index = nextSequentialIndex++;
                if (nextSequentialIndex >= spawnZones.Length) {
                    nextSequentialIndex = 0;
                }
            }
            else {
                index = Random.Range(0, spawnZones.Length);
            }

            return spawnZones[index].SpawnPoint;
        }
    }

    [SerializeField]
    //控制是否要使用复合生成区的形状配置重写基本生成区的形状配置
    bool overrideConfig = false;

    //复合生成区的ConfigureSpawn方法, 在选定了一个本次要产生形状的生成区后, 将形状的实例传递给该生成区处理
    //public override void ConfigureSpawn (Shape shape) {
    //ConfigureSpawn方法改名
    public override Shape SpawnShape()
    {
        //判断是否使用复合生成区自身的spawnConfig数据配置形状
        if (overrideConfig) {
            //如果要使用复合生成区的配置, 调用其父类的ConfigureSpawn方法
            //base的用法我们之前介绍过, 会执行父类的同名方法的逻辑
            //但是注意, 只是调用父类方法逻辑, 过程中要用的字段和属性值还是自己的
            //base.ConfigureSpawn(shape);
            //返回父类SpawnShape方法返回的形状
            return base.SpawnShape();
        }
        //用else语句块把之前的代码都包围起来
        else {
            //现在只有overrideConfig为false时才会把形状传递给基本生成区进行配置

            //以下代码, 除了最后一句, 全部是从该类的SpawnPrint属性的Get方法复制来的
            int index;
            if (sequential) {
                index = nextSequentialIndex++;
                if (nextSequentialIndex >= spawnZones.Length) {
                    nextSequentialIndex = 0;
                }
            }
            else {
                index = Random.Range(0, spawnZones.Length);
            }
            //return spawnZones[index].SpawnPoint;
            //将SpawnPrint属性的Get方法最后一句代码改为下面这句,把本次生成的形状传递给选定的基本生成区进行处理
            //spawnZones[index].ConfigureSpawn(shape);
            //返回指定索引的基本生成区生成的形状
            return spawnZones[index].SpawnShape();
        }
    }

    //重写父类的Save方法
    public override void Save(GameDataWriter writer)
    {
        //写入有序选择复合生成区需要的下一个数组索引
        writer.Write(nextSequentialIndex);
    }

    //重写父类的Load方法
    public override void Load(GameDataReader reader)
    {
        //读取有序选择复合生成区需要的下一个数组索引
        nextSequentialIndex = reader.ReadInt();
    }
}
using UnityEngine;
//要使用List类, 需要引入该命名空间
using System.Collections.Generic;
//在ShapeFactory脚本中引入该命名空间以便使用与场景有关的代码
using UnityEngine.SceneManagement;

//生产形状的"工厂"
[CreateAssetMenu]
public class ShapeFactory : ScriptableObject {
    //容器场景, 用来盛放所有产生的形状
    public Scene poolScene;

    //新增一个Shape类型的List列表的数组字段
    public List<Shape>[] pools;

    [SerializeField]
    //代表工厂是否可以回收利用已产生的形状
    bool recycle = false;

    //将非公开字段显示在Inspector中, 方便编辑
    [SerializeField]
    //用来存储要产生的形状的预制体的引用
    Shape[] prefabs = null;

    [SerializeField]
    //存储为形状设置用的随机材质引用
    Material[] materials = null;

    //该属性用于获取和设置工厂ID
    public int FactoryId {
        get {
            return factoryId;
        }
        set {
            //只有工厂id是默认值且不是要为其赋值整型的最小值时才能进行赋值操作
            if (factoryId == int.MinValue && value != int.MinValue) {
                //满足以上条件, 则表示是对属性初次赋值, 允许执行
                factoryId = value;
            }
            else {
                //如果不是默认值以外的值, 表示已经进行过明确的赋值, 不可以再次更改
                Debug.Log("工厂Id不能重复设置");
            }
        }
    }

    //该特性会使得被指定的字段不会被序列化, 即每次运行游戏都会恢复其代码中的默认值
    [System.NonSerialized]
    //工厂id字段, 使用指定的默认值
    int factoryId = int.MinValue;

    //向池子中回收形状的方法
    public void Reclaim(Shape shapeToRecycle)
    {
        //每次回收形状前都要检查自身是否有资格回收该形状
        if (shapeToRecycle.OriginFactory != this) {
            //如果自身与形状中记录的生成工厂不符, 表示没有资格回收形状, 报错并终止方法
            Debug.LogError("当前工厂与形状记录的生产工厂不一致, 无权回收该形状");
            return;
        }
        //检查工厂是否可以回收形状
        if (recycle) {
            //检查是否存在可用的回收池
            if (pools == null) {
                //如果没有可用的回收池, 需要创建一下
                CreatePools();
            }
            //通过形状的的ShapeId属性作为索引来找到要用来储备它的池子列表, 把它增加到池子中
            pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
            //将被收回的形状设置为停用状态
            shapeToRecycle.gameObject.SetActive(false);
        }
        else {
            //如果recycle为false, 就直接销毁形状
            Destroy(shapeToRecycle.gameObject);
        }
    }

    //用来创建形状储备池的方法
    void CreatePools()
    {
        //pools形状列表数组的长度就是prefabs数组的长度
        pools = new List<Shape>[prefabs.Length];
        //初始化pools中的每一个形状列表
        for (int i = 0; i < pools.Length; i++) {
            pools[i] = new List<Shape>();
        }
        //如果处于编辑器环境, Application.isEditor会返回True
        //只有在编辑器环境中才需要进行以下处理
        if (Application.isEditor) {
            //2017.4.4f1版本没有下面这句代码, 会导致运行中重编译poolScene丢失引用; 
            //译者使用的2019.2.15f1版本无需下面这句代码, 重编译后依然保持正确的引用
            poolScene = SceneManager.GetSceneByName(name);
            if (poolScene.isLoaded) {
                //在运行时重编以后, 获取容器场景中的所有根节点物体
                GameObject[] rootObjects = poolScene.GetRootGameObjects();
                //遍历rootObject数组
                for (int i = 0; i < rootObjects.Length; i++) {
                    //依次提取所有的数组中对象包含的Shape脚本引用
                    Shape pooledShape = rootObjects[i].GetComponent<Shape>();
                    //一个游戏对象是启用状态, activeSelf属性返回true, 否则返回false
                    if (!pooledShape.gameObject.activeSelf) {
                        //如果形状物体是禁用状态, 则需要将其放置到与其形状代号对应的池中
                        pools[pooledShape.ShapeId].Add(pooledShape);
                    }
                }
                //如果容器场景poolScene已经存在, 则在此终止方法的执行
                return;
            }
        }
        //调用CreatePools方法说明形状可以回收, 则同时创建盛放可回收形状的容器场景
        //CreateScene方法的参数代表了创建场景的名字, 此处的name是ShapeFactory实例游戏对象的名称属性
        //注意, name的值不是类的名字, 而是你的Shpae Factory资源文件的名字
        poolScene = SceneManager.CreateScene(name);
    }

    //返回工厂生产的形状实例的方法, 参数shapeId用来代表要生产的形状种类
    //public Shape Get (int shapeId, int materialId) {
    //为Get方法的第一和第二个参数均设置一个默认值, 从而可以在只传入一个参数甚至完全不传参数的情况下调用Get方法
    public Shape Get(int shapeId = 0, int materialId = 0)
    {
        //在Get方法的最开始先声明instance变量, 方便根据不同情况为其赋值
        Shape instance;
        //判断该工厂是否可以回收形状
        if (recycle) {
            //如果工厂可以回收形状, 判断当前是否存在形状储备池
            if (pools == null) {
                //如果不存在储备池, 则调用创建储备池的方法
                CreatePools();
            }
            //通过shapeId作为索引, 得到该类型形状的储备池列表
            List<Shape> pool = pools[shapeId];
            //获取得到的列表的最后一个索引
            int lastIndex = pool.Count - 1;
            //检查得到的最后一个索引是否合法, 如果大于等于0, 说明池子至少存在一个储备的形状, 可以进行抓取
            if (lastIndex >= 0) {
                //将储备池的最后一个形状的引用赋值给instance
                instance = pool[lastIndex];
                //激活从池子中抓取到的形状
                instance.gameObject.SetActive(true);
                //在池子中移除已经被抓取的形状
                pool.RemoveAt(lastIndex);
            }
            //如果最后一个索引小于0, 说明池子中一个形状也没有, 那就需要新实例化一个形状
            else {
                instance = Instantiate(prefabs[shapeId]);
                instance.ShapeId = shapeId;
                //生产形状后将将自身记录为形状的生产工厂
                instance.OriginFactory = this;
                //创建新的形状实例后, 将其放入到容器场景中
                SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
            }
        }
        //将原来的直接实例化形状的代码, 包到新增的else块里, 只有recycle为flase时才执行
        else {
            //创建并返回通过shapeId索引到的预制体的实例
            //return Instantiate(prefabs[shapeId]);
            //Shape instance = Instantiate(prefabs[shapeId]);
            //instance的声明语句被提前到了方法的开始部分, 此处直接赋值即可
            instance = Instantiate(prefabs[shapeId]);
            //为新创建的形状设置设置代号
            instance.ShapeId = shapeId;
            //调用Shape的SetMaterial方法完成对形状材质代号和材质资源的设置
        }
        instance.SetMaterial(materials[materialId], materialId);
        return instance;
    }

    public Shape GetRandom()
    {
        //return Get(Random.Range(0, prefabs.Length));
        //为GetRandom中的Get方法调用传入随机 
        return Get( Random.Range(0, prefabs.Length), Random.Range(0, materials.Length));
    }
}
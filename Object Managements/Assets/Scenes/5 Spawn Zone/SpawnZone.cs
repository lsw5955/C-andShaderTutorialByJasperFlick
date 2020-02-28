using UnityEngine;

//通过该脚本获取形状生成的位置
//public class SpawnZone : MonoBehaviour {
//abstract关键字, 把SpawnZone类变成了一个抽象类
//"抽象"是面相对象编程的一种概念, 抽象类仅可用于被其他类继承,自身不能实例化
//public abstract class SpawnZone : MonoBehavior {
//让SpawnZone继承PersistableObject类, SpawnZone是所有基本生成区脚本与符合生成区脚本的父类
public abstract class SpawnZone : PersistableObject {
    //代表形状可选的运动方向的枚举类型
    public enum SpawnMovementDirection {
        //代表形状应该向生成区的前方向运动
        Forward,
        //代表形状应该向生成区的上方向运动
        Upward,
        //代表形状应该从当前位置向远离生成区中心方向运动
        Outward,
        //代表随机选择形状的运动方向
        Random
    }

    //添加该特性后, 自定义结构类型的字段才能出现在Inspector中进行配置
    [System.Serializable]
    //用于包含所有与形状配置有关内容的结构
    public struct SpawnConfiguration {
        //用来为生成区配置要使用的形状工厂
        public ShapeFactory[] factories;
        //控制生成区对形状的配色方式, false表示对构成形状的每个组成部分使用不同颜色
        public bool uniformColor;
        //该字段用于形状的随机颜色范围配置
        public ColorRangeHSV color;
        //SpawnMovementDirection改名后放在该结构内
        public enum MovementDirection {
            Forward,
            Upward,
            Outward,
            Random
        }
        //用来配置该生成区为形状配置哪一个运动方向
        //spawnMovementDirection改名后放在该结构内
        public MovementDirection movementDirection;
        //代表形状可配置的最小速度与最大速度
        //[SerializeField] float spawnSpeedMin, spawnSpeedMax;
        //使用自定义的结构类型代替之前的两个字段进行运动速度的限制
        //spawnSpeed改名后放在该结构内
        public FloatRange speed;
        //配置形状的角速度范围
        public FloatRange angularSpeed;
        //配置形状的缩放范围
        public FloatRange scale;
    }

    [SerializeField]
    //存储与形状配置有关的数据
    SpawnConfiguration spawnConfig;

    //只读属性, 返回形状生成位置
    //public Vector3 SpawnPoint { 
    //声明抽象属性SpawnPoint, 对于抽象属性和抽象方法, 只需做出声明, 不需要也不可以书写任何具体的功能逻辑代码
    public abstract Vector3 SpawnPoint { get; }

    //每个生成区在该方法中进行对自己产生形状的配置
    //以下代码是把Game.CreateShape方法中与形状配置有关的代码移动过来修改而成
    //public virtual void ConfigureSpawn (Shape shape) {
    //想要被派生类重写, 就必须用virtual关键字变成虚拟方法
    //public virtual void ConfigureSpawn (Shape shape) {
    //ConfigureSpawn方法改名
    public virtual Shape SpawnShape()
    {
        //随机获取一个工厂的数组索引
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        //使用随机索引对应的工厂生产形状
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
        //Transform t = instance.transform;
        //shape参数取代instance
        Transform t = shape.transform;
        //t.localPosition = GameLevel.Current.SpawnPoint;
        //在SpawnZone类内部可以直接访问SpawnPoint属性
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        //t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        //使用spawnConfig.scale配置形状的缩放
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;

        if (spawnConfig.uniformColor) {
            //使用spawnConfig.color.RandomInRange代替之前获取随机颜色的代码
            shape.SetColor(spawnConfig.color.RandomInRange);
        }
        //spawnConfig.uniformColor为false, 表示对构成形状的每个组成部分使用不同颜色
        else {
            //根据shape.ColorCount属性得到的颜色数量进行循环, 为形状每一个组成部分分别设置随机颜色
            for (int i = 0; i < shape.ColorCount; i++) {
                //调用两个参数的SetColor方法
                shape.SetColor(spawnConfig.color.RandomInRange, i);
            }
        }
        //AddComponent<脚本类名>()方法会为指定的游戏对象添加指定类型的脚本组件, 并返回该脚本的实例
        var rotation = shape.gameObject.AddComponent<RotationShapeBehavior>();
        //shape.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;
        //不再使用上面这句代码设置形状自身的角速度, 而是使用行为脚本的角速度控制旋转
        rotation.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;
        //设置形状的移动速度
        //instance.Velocity = Random.onUnitSphere * Random.Range(0f, 2f);
        //shape参数取代instance
        //shape.Velocity = Random.onUnitSphere * Random.Range(0f, 2f);
        //将形状的运动速度由随机方向改为生成区自身的z轴正方向, 
        //transform.forwoard代表当前游戏物体本地坐标系下的(0,0,1)向量
        //shape.Velocity = transform.forward * Random.Range(0f, 2f);
        //存储得到的速度方向
        Vector3 direction;

        //switch将会检查每一个case关键字后面的值与其括号内写的值是否相等, 
        //相等则执行case下的语句, 不相等则继续向下寻找下一个case, 直到遇到相等情况或是switch语句全部执行完毕
        switch (spawnConfig.movementDirection) {
            case SpawnConfiguration.MovementDirection.Upward:
                //如果枚举字段代表向上, 则速度方向为transform.up;
                direction = transform.up;
                //该语句用于跳出switch
                break;
            case SpawnConfiguration.MovementDirection.Outward:
                //如果枚举字段代表向外, 则运动方向由生成区中心指向形状当前位置
                direction = (t.localPosition - transform.position).normalized;
                break;
            case SpawnConfiguration.MovementDirection.Random:
                //如果枚举字段代表随机方向, 则运动方向随机设置
                direction = Random.onUnitSphere;
                break;
            default:
                //如果spawnConfig.movementDirection的值与任何case关键字后面的值都不相等, 则速度方向为transform.forward
                direction = transform.forward;
                break;
        }
        //为形状添加移动行为脚本组件
        var movement = shape.gameObject.AddComponent<MovementShapeBehavior>();
        //shape.Velocity = direction * spawnConfig.speed.RandomValueInRange;
        //不再使用上面这句代码设置形状自身的移动速度, 而是使用行为脚本的移动速度控制移动
        movement.Velocity = direction * spawnConfig.speed.RandomValueInRange;
        //返回新生成的形状
        return shape;
    }
}
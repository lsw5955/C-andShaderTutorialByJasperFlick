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
        //配置该生成区产生形状的震荡方向
        public MovementDirection oscillationDirection;
        //配置该生成区产生形状的震荡幅度随机范围
        public FloatRange oscillationAmplitude;
        //配置该生成区产生形状的震荡频率随机范围
        public FloatRange oscillationFrequency;
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
        //获取本次形状生成时随机得到的角速度值
        float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
        //只有角速度值不为0时才有必要添加旋转行为组件
        if (angularSpeed != 0f) {
            //AddComponent<脚本类名>()方法会为指定的游戏对象添加指定类型的脚本组件, 并返回该脚本的实例            
            //使用AddBehavior代替AddComponent
            var rotation = shape.AddBehavior<RotationShapeBehavior>();
            //shape.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;
            //不再使用上面这句代码设置形状自身的角速度, 而是使用行为脚本的角速度控制旋转
            rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
        }

        //获取本次形状生成时随机得到的移动速度值
        float speed = spawnConfig.speed.RandomValueInRange;
        //只有移动速度不为0时才有必要添加移动行为组件
        if (speed != 0f) {            
            //为形状添加移动行为脚本组件            
            //使用AddBehavior代替AddComponent
            var movement = shape.AddBehavior<MovementShapeBehavior>();
            //通过GetDirectionVector方法获得运动方向
            movement.Velocity = GetDirectionVector(spawnConfig.movementDirection, t) * speed;
        }
        //对当前产生的形状配置震荡行为
        SetupOscillation(shape);
        //返回新生成的形状
        return shape;
    }

    /// <summary>
    /// 该方法专门用来将MovementDirection类型的枚举值转换为与形状本地坐标系有关的一个向量
    /// </summary>
    /// <param name="direction">代表生成区的枚举配置</param>
    /// <param name="t">代表目标形状</param>
    /// <returns>与枚举值对应的</returns>
    Vector3 GetDirectionVector(SpawnConfiguration.MovementDirection direction, Transform t)
    {
        switch (direction) {
            case SpawnConfiguration.MovementDirection.Upward:
                return transform.up;
            case SpawnConfiguration.MovementDirection.Outward:
                return (t.localPosition - transform.position).normalized;
            case SpawnConfiguration.MovementDirection.Random:
                return Random.onUnitSphere;
            default:
                return transform.forward;
        }
    }

    //专门为指定形状配置震荡行为的方法
    void SetupOscillation(Shape shape)
    {
        //根据配置随机计算震荡幅度
        float amplitude = spawnConfig.oscillationAmplitude.RandomValueInRange;
        //根据配置随机计算震荡频率
        float frequency = spawnConfig.oscillationFrequency.RandomValueInRange;
        //幅度与频率只要有一个为0, 震荡行为便不能正常配置, 终止方法
        if (amplitude == 0f || frequency == 0f) {
            return;
        }
        //在形状的行为列表中增加一个震荡行为
        var oscillation = shape.AddBehavior<OscillationShapeBehavior>();
        //为增加的震荡行为配置方向与幅度, 一行太长了, 分行写方便阅读
        oscillation.Offset = GetDirectionVector(
            spawnConfig.oscillationDirection,
            shape.transform) * amplitude;
        //为增加的震荡行为配置频率
        oscillation.Frequency = frequency;
    }
}
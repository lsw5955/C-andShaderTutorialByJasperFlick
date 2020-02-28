using UnityEngine;

public class Shape : PersistableObject {
    //默认属性写法
    public int MaterialId { get; set; }

    //形状的代号, 告诉程序是哪一种形状
    //int shapeId;
    //在声明shapeId时使用int.MinValue为其赋默认值
    int shapeId = int.MinValue;

    //代表材质属性块的字段
    static MaterialPropertyBlock sharedPropertyBlock;

    //新增一个静态字段colorPropertyId, 用来存储为名为"_Color"的渲染器属性块
    //Shader.PropertyToID方法会自动帮我们得到一个代表"_Color"的代号, 通过这个方面可以看出来, 其实就是在操作Shader的_Color属性
    static int colorPropertyId = Shader.PropertyToID("_Color");

    //该字段用于存储形状的MeshRenderer组件实例的引用
    //现在MeshRenderer组件将手动指定, 不需要该变量去自动获取了
    //MeshRenderer meshRenderer;

    //其他的类将使用属性ShapeId, 实现对非公开字段shapeId的访问
    public int ShapeId {
        //属性的get方法, 在后面的大括号中书写返回属性值的逻辑
        get {
            //返回shapeId的值
            return shapeId;
        }
        //属性的set方法, 在后面的大括号中书写设置属性值的逻辑
        set {
            //新增if语句, 将之前的赋值语句包围起来
            //if (shapeId == 0) {
            //使用int.MinValue代替0作为判断shapeId默认值的条件, 并且不允许设置int.MinValue这个值
            if (shapeId == int.MinValue && value != int.MinValue) {
                //如果shapeId的值是声明时的默认值0, 则可以对其进行赋值
                //属性接收到的值会放在叫做value的特殊变量中, 我们将其赋值给shapeId字段
                shapeId = value;
            }
            else {
                //如果shapeId的值已经不是声明时的默认值了, 则不对其赋值, 并在控制台打印一条错误信息
                Debug.LogError("ShapeId赋值失败, 因为字段shapeId不可以进行多次赋值.");
            }
        }
    }

    //获取旋转角速度的属性
    public Vector3 AngularVelocity { get; set; }

    //获得形状的移动速度
    public Vector3 Velocity { get; set; }

    [SerializeField]
    //存储脚本要操作的所有MeshRenderer组件的引用
    MeshRenderer[] meshRenderers = null;

    //用来得到颜色数组长度, 也就是复合形状需要设置不同颜色的形状数量
    public int ColorCount {
        get {
            return colors.Length;
        }
    }

    //用来获取生成形状的工厂类型
    public ShapeFactory OriginFactory {
        //get方法直接返回属性值
        get {
            return originFactory;
        }
        //set方法只能被设置一次属性值, 重复设置会显示文字提示
        set {
            if (originFactory == null) {
                originFactory = value;
            }
            else {
                Debug.LogError("不允许篡改形状的生产工厂");
            }
        }
    }

    //存储生产该形状的工厂引用
    ShapeFactory originFactory;

    //方便形状在销毁时调用自身生产工厂的Reclaim方法
    public void Recycle()
    {
        OriginFactory.Reclaim(this);
    }

    //该方法专门用于处理颜色数据加载
    void LoadColors(GameDataReader reader)
    {
        //加载保存的颜色总数
        int count = reader.ReadInt();
        //读取颜色数据的次数取保存的颜色总数与形状所需的颜色总数之间的较小值
        int max = count <= colors.Length ? count : colors.Length;
        //按照计算出来的次数循环读取颜色数据
        //循环控制变量i没有写在循环内部作为局部变量, 因为下面马上还要用它循环后的值做其他处理
        int i = 0;
        for (; i < max; i++) {
            SetColor(reader.ReadColor(), i);
        }
        //如果保存的颜色总数比形状需要的多, 则需要额外的多余的颜色数据读取出来, 
        //否则后续应该读取其他保存数据的代码会接着读取剩余的颜色数据, 就出错了
        if (count > colors.Length) {
            //还需要继续读取颜色数据的次数是(count - i)个
            for (; i < count; i++) {
                reader.ReadColor();
            }
        }
        //如果形状需要的颜色总数比保存的多, 则需要为多出来的部分设置固定颜色 
        //否则如果这个形状用的是回收池里的, 这些多出来的部分就还是销毁前的配色
        else if (count < colors.Length) {
            //还需要配置固定颜色的部分是(count - i)个
            for (; i < colors.Length; i++) {
                SetColor(Color.white, i);
            }
        }
    }

    //重写PersistableObject类的Load方法
    public override void Load(GameDataReader reader)
    {
        //base.方法名()表示, 调用其父类对应名称的方法, 在这里父类就是PersistableObject类
        base.Load(reader);
        //版本号大于等于5才支持读取多个颜色数据
        if (reader.Version >= 5) {
            //按照colors数组的长度, 依次读取每一个颜色数据
            //for (int i = 0; i < colors.Length; i++) {
            //    //调用SetColor方法设置对应索引的形状颜色
            //    SetColor(reader.ReadColor(), i);
            //}
            //删除上方与颜色加载有关的代码, 调用单独的LoadColors方法处理颜色加载过程
            LoadColors(reader);
        }
        //如果版本号小于5, 使用旧的颜色读取代码
        else {
            //从文件中加载形状的颜色数据
            //SetColor(reader.ReadColor());
            //如果版本号大于0, 则表示是带有颜色数据的新版本, 需要读取颜色数据. 否则, 不读取颜色数据, 使用写死的白色代替
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }
        //如果版本号大于等于4, 读取数据作为角速度值, 否则固定设置为Vector3.zero
        AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        //如果版本号大于等于4, 读取数据作为移动速度值, 否则固定设置为Vector3.zero
        Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
    }

    //SetColor的另一个版本, 接受两个参数, 
    //代码及功能与一个参数的版本只有一个区别 : 它每次只按照index参数设置对应索引的形状颜色
    public void SetColor(Color color, int index)
    {
        if (sharedPropertyBlock == null) {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        colors[index] = color;
        meshRenderers[index].SetPropertyBlock(sharedPropertyBlock);
    }

    //FixedUpdate方法
    //void FixedUpdate () {
    //形状不再使用FixedUpdate方法更新自身状态, 而是使用自定义的方法, 通过Game脚本统一管理调用时机
    public void GameUpdate()
    {
        //调用Rotate方法, 形状将使用与参数代表的的欧拉角进行旋转
        //transform.Rotate(Vector3.forward);
        //乘以希望的旋转速度值50 , 再乘以FixedUpdate的执行间隔时间, 
        //这样无论FixedUpdate方法调用的频率是多少, 形状的转动速度都不会受到影响
        //transform.Rotate(Vector3.forward * 50f * Time.deltaTime);
        //使用AngularVelocity属性的值来获取随机角速度, 并代替之前设置的固定速度值
        transform.Rotate(AngularVelocity * Time.deltaTime);
        //实现形状的移动
        transform.localPosition += Velocity * Time.deltaTime;
    }

    //现在MeshRenderer组件将手动指定, 不需要Awake方法去自动获取了
    //void Awake()
    //{
    //    //在Awake方法中获取到MeshRenderer组件实例的引用, 存储到meshRenderer字段中
    //    meshRenderer = GetComponent<MeshRenderer>();
    //}

    //重写PersistableObject类的Save方法
    public override void Save(GameDataWriter writer)
    {
        //base.方法名()表示, 调用其父类对应名称的方法, 在这里父类就是PersistableObject类
        base.Save(writer);
        //将形状所包含的颜色总数写入保存文件
        writer.Write(colors.Length);
        //向文件写入形状的颜色数据
        //writer.Write(color);
        //color字段被colors数组替代, 现在要遍历colors数组, 保存每一个元素代表的颜色数据
        for (int i = 0; i < colors.Length; i++) {
            writer.Write(colors[i]);
        }
        //写入形状的角速度
        writer.Write(AngularVelocity);
        //向保存文件写入移动速度 :
        writer.Write(Velocity);
    }

    //代表形状的颜色数据的字段
    //Color color;
    //使用颜色数组代替单一的颜色
    Color[] colors;

    //新增Awake方法
    void Awake()
    {
        //初始化颜色数组, 其长度与meshRenderers数组一致0
        colors = new Color[meshRenderers.Length];
    }

    //该方法将完成调整材质颜色的任务
    public void SetColor(Color color)
    {
        //使用方法的color参数设置color字段的值
        //color字段已经删除 此处代码也应该一并删除
        //this.color = color;
        //使用方法的color参数设置当前形状使用的材质的颜色
        //GetComponent<MeshRenderer>().material.color = color;
        //使用meshRenderer字段代替GetComponent<MeshRenderer>()
        //meshRenderer.material.color = color;
        //新建一个渲染器的属性块变量, 通过它设置材质颜色有更好的性能
        //var propertyBlock = new MaterialPropertyBlock();
        //删除变量propertyBlock, 使用静态字段sharedPropertyBlock代替它
        if (sharedPropertyBlock == null) {
            //由于MonoBehavior不可以在声明字段时调用字段的构造方法, 所以需要在一个具体的方法内调用
            //如果sharedPropertyBlock为null, 表示还没有初始化, 则调用其构造方法进行初始化
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        //为该属性块设置对应的属性名称"_Color"与属性值color
        //propertyBlock.SetColor("_Color", color);
        //使用colorPropertyId作为"_Color"属性块名称的代号
        //propertyBlock.SetColor(colorPropertyId, color);
        //使用静态字段sharedPropertyBlock代替变量propertyBlock
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        //使用SetPropertyBlock方法, 传上述属性块作为参数, 完成对材质颜色的设置
        //meshRenderer.SetPropertyBlock(propertyBlock);
        //使用静态字段sharedPropertyBlock代替变量propertyBlock   
        //meshRenderer.SetPropertyBlock(sharedPropertyBlock);
        //不再为单个MeshRenderer设置材质属性块, 需要为meshRenderers列表中每一个元素设置材质属性块
        for (int i = 0; i < meshRenderers.Length; i++) {
            //按照索引, 将每一次配置的颜色数据存储colors数组中
            colors[i] = color;
            //设置材质属性块
            meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
        }
    }

    //通过该方法完成对形状材质代号和材质资源的设置
    public void SetMaterial(Material material, int materialId)
    {
        //获取形状的MeshRenderer组件实例, 并使用参数material的值赋值它的material属性
        //GetComponent<MeshRenderer>().material = material;
        //使用meshRenderer字段代替GetComponent<MeshRenderer>()
        //meshRenderer.material = material;
        //不再为单个MeshRenderer设置材质, 需要为meshRenderers类别中每一个元素设置材质
        for (int i = 0; i < meshRenderers.Length; i++) {
            meshRenderers[i].material = material;
        }
        //使用参数值赋值形状的材质代号属性
        MaterialId = materialId;
    }
}
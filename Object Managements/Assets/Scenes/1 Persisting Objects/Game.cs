using UnityEngine;
using System.Collections.Generic;
//Path位于System.IO命名空间中
using System.IO;
using UnityEngine.SceneManagement;
//协程方法的返回类型IEnumerator需要引入该命名空间
using System.Collections;
//滑动条对象类型Slider需要引用该命名空间
using UnityEngine.UI;

//public class Game : MonoBehavior {
//继承PersistableObject, 以便通过storage字段对自身进行保存
public class Game : PersistableObject {
    //存储控制自动创建速度的滑动条引用
    [SerializeField] Slider creationSpeedSlider = null;
    //存储控制自动销毁速度的滑动条引用
    [SerializeField] Slider destructionSpeedSlider = null;

    //false表示加载游戏时不加载之前保存的随机状态
    [SerializeField] bool reseedOnLoad = false;

    //该字段用于存储每次开始新游戏时, 随机计算新游戏使用的随机种子所需的随机状态
    Random.State mainRandomState;

    //访问Game脚本实例的属性, 公开的get方法, 私有的set方法
    //没有任何其他脚本在使用Instance了, 将它删除掉
    //public static Game Instance { get; private set; }

    //用来引用生成区物体 通过生成区得到形状生成的位置
    //public SpawnZone spawnZone;
    //用于获得关卡场景内的生成区
    //现在通过GameLevel的静态属性Current获取当前关卡实例, Game不再需要下面的属性
    //public SpawnZone SpawnZoneOfLevel { get; set; }

    //保存当前加载的关卡索引
    int loadedLevelBuildIndex;

    //代表项目中关卡场景的数量
    //public
    [SerializeField] int levelCount = 0;

    //代表连续创建形状时上一个形状的销毁进度, 当其值变成1表示一个形状已经的销毁进度已经100%, 可以开始销毁下一个形状了
    float destructionProgress;

    //代表形状的连续销毁速度的属性
    public float DestructionSpeed { get; set; }

    //代表连续创建形状时上一个形状的创建进度, 当其值变成1表示一个形状已经的创建进度已经100%, 可以开始创建下一个形状了
    float creationProgress;

    //代表形状的连续产生速度的属性
    public float CreationSpeed { get; set; }

    //该字段用于设置销毁操作的快捷键
    //public
    [SerializeField] KeyCode destroyKey = KeyCode.X;

    //整型常量, 代表存档文件的版本号
    //版本1实现了多种形状类型的保存
    //版本2实现了关卡场景加载情况的保存
    //const int saveVersion = 2;
    //游戏保存文件版本升级为3, 实现了对随机状态Random.state的保存
    //const int saveVersion = 3;
    //版本号升级为4, 实现了保存形状的转动角速度
    //const int saveVersion = 4;
    //版本号升级为5, 实现了分别保存复合形状中每一个形状的颜色
    //const int saveVersion = 5;
    //版本号6实现了保存形状所带有的行为脚本的状态数据
    const int saveVersion = 6;

    //用来获取PersistentStorage实例的字段
    //public
    [SerializeField] PersistentStorage storage = null;

    //用来指定所需预制体的字段
    //public Transform prefab;
    //修改类型
    //	public PersistableObject prefab;
    //存储工厂类资源的引用
    //public ShapeFactory shapeFactory;
    //将shapeFactory改为私有字段, 使其不能被其他脚本的代码方法, 
    //增加[SerializeField]特性使其依然可以通过Inspector进行配置
    //已经被工厂数组shapeFactories取代
    //[SerializeField] ShapeFactory shapeFactory;

    //createKey字段用来指定功能按键, 默认设置为C键
    //public 
    [SerializeField] KeyCode createKey = KeyCode.C;

    //代表开始新游戏的按键
    //public
    [SerializeField] KeyCode newGameKey = KeyCode.N;

    //用来存储所有生成的cube引用
    //List<Transform> objects;
    //修改类型
    //List<PersistableObject> shapes;
    List<Shape> shapes;

    //游戏存档文件的保存路径字段
    //该类不再需要这个字段
    //string savePath;

    //新增saveKey保存触发文件保存过程的按键, 默认设置为S
    //public 
    [SerializeField] KeyCode saveKey = KeyCode.S;

    //设置按键触发数据加载, 设置L为默认按键
    //public
    [SerializeField] KeyCode loadKey = KeyCode.L;

    //将工厂资源文件拖拽分配给该数组, 数组索引代表了该工厂的Id
    //每个工厂资源文件只需要在数组中存在, 因为一个工厂Id只能赋值一次, 分配多次也不会有效果
    [SerializeField] ShapeFactory[] shapeFactories = null;

    //新增OnEnable方法
    void OnEnable()
    {
        //多种情况都可能导致OnEnable方法被触发, 因此先检查工厂Id是否已经赋值过了, 
        //如果是, 则不需要重复赋值
        if (shapeFactories[0].FactoryId != 0) {
            //脚本启用时, 使用数组索引为每个工厂设置其工厂Id
            for (int i = 0; i < shapeFactories.Length; i++) {
                shapeFactories[i].FactoryId = i;
            }
        }
    }

    //void Awake () {
    //使用OnEnable方法可以保障在运行时候重编译后再次设置Instance, 防止引用丢失
    //注意, 教程项目的代码会在重编以后执行两次OnEnable方法, 这是因为在Start方法中调用了LevelLoad方法
    //而LevelLoad方法中会禁用再启用Game脚本
    //没有任何其他脚本在使用Instance了, 将它删除掉
    //void OnEnable()
    //{
    //    //脚本唤醒时, 将当前实例的引用赋值给Instance属性
    //    Instance = this;
    //}

    //void LoadLevel() {
    //将LoadLevel的返回值由void改为IEnumerator, 以便在协程中调用
    IEnumerator LoadLevel(int levelBuildIndex)
    {
        //通过Unity游戏对象内置的enabled属性, 在场景加载完成之前禁用Game脚本
        enabled = false;
        //判断是否存在已加载的关卡
        if (loadedLevelBuildIndex > 0) {
            //如果存在已加载的关卡, 要先异步卸载该关卡场景, 才继续执行该方法后面的代码
            yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
        }
        //加载Level 1场景
        //SceneManager.LoadScene("Level 1");
        //第二个参数LoadSceneMode.Additive表示, 要打开的场景会作为额外场景加入到当前已打开的场景中
        //SceneManager.LoadScene("Level 1", LoadSceneMode.Additive);
        //在加载和激活场景的代码之间加入yield return null语句, 将会使得代码暂停执行一帧, 下一帧再继续执行
        //yield return null;
        //以下代码的效果是, 如果在本帧场景Level 1加载完成, 则执行它之后的下一行代码, 
        //否则, 该方法在本帧将在此结束, 到下一帧继续执行该方法, 以此类推, 直到场景加载完成为止
        //yield return SceneManager.LoadSceneAsync("Level 1", LoadSceneMode.Additive);
        //使用levelBuildIndex参数代替之前的"Level 1"作为第一个参数, 异步加载指定索引的场景
        yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
        //通过SceneManager.SetActiveScene方法将Level 1设置为活跃场景
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName("Level 1"));
        //使用场景在构建设置中分配的索引号来获取指定场景的引用, 并检查该场景是否被加载
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        //加载了指定索引的关卡场景后, 将其索引值保存在loadedLevelBuildIndex字段中
        loadedLevelBuildIndex = levelBuildIndex;
        //通过Unity游戏对象内置的enabled属性, 在场景加载完成之后启用Game脚本
        enabled = true;
    }

    //该方法用于完成销毁形状的任务
    void DestroyShape()
    {
        //只有shapes列表中元素的数量大于0时, 才执行销毁操作
        if (shapes.Count > 0) {
            //获取一个在0和形状总数之间的随机数
            int index = Random.Range(0, shapes.Count);
            //使用获得的随机数作为索引, 在shapes列表中选择一个形状, 并销毁
            //Destroy(shapes[index]);
            //通过gameObjec属性来得到一个脚本组件所属的游戏物体, 要删除的是形状物体, 而不是它的Shape脚本
            //Destroy(shapes[index].gameObject);
            //将Destroy方法替换为Reclaim方法, 由形状工厂决定应该回收还是销毁
            //shapeFactory.Reclaim(shapes[index]);
            //现在要根据每个形状自身的记录来决定使用哪个工厂对其进行回收
            shapes[index].Recycle();
            //在销毁形状后, 将shapes中代表它的元素从列表中移除
            //shapes.RemoveAt(index);
            //不再直接移除index位置的元素, 而是通过将末尾元素交换到移除元素位置, 然后移除末尾元素
            //这样牺牲了列表的原有顺序, 但是得到了移除效率的提升
            // 得到末尾元素索引
            int lastIndex = shapes.Count - 1;
            //将末尾元素交换到要移除的元素位置
            shapes[index] = shapes[lastIndex];
            //移除重复的末尾元素, 完成高效的列表元素移除过程
            shapes.RemoveAt(lastIndex);
        }
    }

    //用于在Game方法中增加新的Save方法, 该方法接受一个GameDataWriter类型的参数
    //public void Save (GameDataWriter writer) {
    //通过override关键字, 告诉编译器, 子类将重写父类的同名同参数方法, 从而使得通过该子类的实例调用该方法时, 始终是执行的子类重写的方法
    public override void Save(GameDataWriter writer)
    {
        //向保存文件写入版本号
        //writer.Write(saveVersion);
        //在Game脚本中删除向文件写入版本号的代码, 该任务现在由PersistentStorage.Save方法完成
        //writer.Write(-saveVersion);
        //保存cube数量
        writer.Write(shapes.Count);
        //向保存文件写入随机状态数据, Random.state属性会返回程序的随机状态数据
        writer.Write(Random.state);
        //保存自动创建进度信息前保存自动创建速度
        writer.Write(CreationSpeed);
        //写入保存时的creationProgress字段值
        writer.Write(creationProgress);
        //保存自动销毁进度信息前保存自动销毁速度
        writer.Write(DestructionSpeed);
        //写入保存时的destructionProgress字段值
        writer.Write(destructionProgress);
        //写入当前加载的关卡场景索引
        writer.Write(loadedLevelBuildIndex);
        //在写入关卡索引数据后调用当前关卡GameLevel实例的Save方法
        GameLevel.Current.Save(writer);
        //遍历objects列表, 调用每一个cube的PersistableObject.Save方法进行数据保存
        for (int i = 0; i < shapes.Count; i++) {
            //加载游戏时, 产生形状的第一步就应该确定使用哪个工厂, 所以在保存游戏时, 也要最先保存工厂Id
            writer.Write(shapes[i].OriginFactory.FactoryId);
            //在每个形状保存自身数据之前, 保存它的形状代号
            writer.Write(shapes[i].ShapeId);
            //向文件写入每个形状的材质代号
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }

    //在Game中重写父类的Load方法
    public override void Load(GameDataReader reader)
    {
        //从保存文件中加载版本号, 
        //int version = reader.ReadInt();
        //由于保存时对版本号数据做了负数处理, 所以加载时也需要对版本号数据做一次负数处理
        //int version = -reader.ReadInt();
        //不再自己去文件中读版本号数据, 而是使用reader.Version属性值代替
        int version = reader.Version;
        //在加载版本号之后进行if判断
        if (version > saveVersion) {
            //如果加载到的版本号大于自身的saveVersion, 则显示错误信息, 并终止执行方法
            Debug.LogError("Unsupported future save version " + version);
            return;
        }
        //开始新增的协程方法, 将在该方法内完成加载关卡场景与加载关卡数据的过程
        //原Load方法中此处之后的代码全部移动到了LoadGame方法中
        StartCoroutine(LoadGame(reader));
    }

    //新增的协程方法, 将在该方法内完成加载关卡场景与加载关卡数据的过程
    IEnumerator LoadGame(GameDataReader reader)
    {
        //获取保存文件版本号
        int version = reader.Version;
        //加载cube的数量
        //int count = reader.ReadInt();
        //使用问号三元操作符赋值, 如果version是负数, 则赋值-version, 否则赋值reader.ReadInt()
        int count = version <= 0 ? -version : reader.ReadInt();
        //判断游戏版本是否大于等3
        if (version >= 3) {
            //版本3开始, 保存文件中才有随机状态数据可以读取
            //Random.state = reader.ReadRandomState();
            //为了保持后续文件数据依然可以按照正确顺序读取, 无论是否要加载随机状态, 
            //均需要先从文件读取流中把随机状态读取出来
            Random.State state = reader.ReadRandomState();
            //判断是否要加载保存时的随机状态
            if (!reseedOnLoad) {
                //reseedOnload为false表示需要加载保存时的随机状态
                Random.state = state;
            }
            //读取自动创建进度信息前读取自动创建速度
            //CreationSpeed = reader.ReadFloat();
            //将自动创建速度字段与对应滑动条的value都设置为读取到的速度数据
            creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
            //读取保存的creationProgress字段值
            creationProgress = reader.ReadFloat();
            //读取自动销毁进度信息前读取自动销毁速度
            //DestructionSpeed = reader.ReadFloat();
            //将自动销毁速度字段与对应滑动条的value都设置为读取到的速度数据
            destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
            //读取保存的destructionProgress字段值
            destructionProgress = reader.ReadFloat();
        }
        //如果版本号小于2,表示保存文件教旧, 还没有关卡数据, 则直接加载索引为1的关卡; 否则加载读取到的关卡索引
        //StartCoroutine(LoadLevel(version < 2 ? 1 : reader.ReadInt()));
        //下面的yield return语句的作用是, 只有LoadLevel方法的全部代码执行完毕, 才会继续执行后面的代码
        yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
        //判断版本号是否大于等于3
        if (version >= 3) {
            //如果大于等于3, 说明保存文件中包括关卡数据, 可以加载
            GameLevel.Current.Load(reader);
        }
        //按照cube的数量, 循环加载每一个cube的数据
        for (int i = 0; i < count; i++) {
            //读取形状的生产工厂Id, 然后以此为索引确定要实例化形状的工厂. 
            //如果保存文件的版本小于5, 说明还没有工厂Id数据, 则使用0作为形状的生产工厂Id
            int factoryId = version >= 5 ? reader.ReadInt() : 0;
            //生成加载的cube, 并获取其PersistableObject实例的引用, 存储到变量o中
            // PersistableObject o = Instantiate(prefab);
            //shapeFactory.Get(0)产生0号形状, 也就是立方体
            //Shape instance = shapeFactory.Get(0);
            //加载形状代号
            // int shapeId = reader.ReadInt();
            // 如果version数据大于0, 表示是新版本, 则使用reader.ReadInt()为shapeId赋值, 否则赋值为0
            int shapeId = version > 0 ? reader.ReadInt() : 0;
            //新增材质代号的加载代码
            int materialId = version > 0 ? reader.ReadInt() : 0;
            //使用形状代号作为参数调用工厂类的Get方法, 得到代号代表的形状实例
            //Shape instance = shapeFactory.Get(shapeId);
            //在调用工厂类的Get方法时, 将加载到的材质代号数据作为第二个参数传递进去
            //Shape instance = shapeFactory.Get(shapeId, materialId);
            //使用工厂Id作为索引确定使用哪个工厂产生形状
            Shape instance = shapeFactories[factoryId].Get(shapeId, materialId);
            //调用PersistableObject.Laod, 从文件读取数据流中获取保存数据并用来设置cube的Transform组件属性
            instance.Load(reader);
            //将新生成的cube添加到objects列表中
            shapes.Add(instance);
        }
    }

    //void Awake () {
    //使用Start方法代替Awake, 代码内容不变, 防止方法执行过早导致错误的判断场景加载状态
    void Start()
    {
        //将游戏开始时的随机状态保存起来, 需要的时候使用
        mainRandomState = Random.state;
        //新建List<Transform>实例, 并将引用赋值给objects字段
        //objects = new List<Transform>();
        //修改类型
        //shapes = new List<PersistableObject>();
        shapes = new List<Shape>();
        //使用Application.persistentDataPath得到游戏文件在文件系统中的路径
        //savePath = Application.persistentDataPath;
        //使用Path.Combine方法将两个字符串作为文件路径进行连接, Unity字段会为我们选择适合当前平台的斜杠或是反斜杠进行连接
        //该类不再需要这个字段
        //savePath = Path.Combine(Application.persistentDataPath, "saveFile");
        //获取Level 1场景的引用, 存储到变量loadedLevel中
        Scene loadedLevel = SceneManager.GetSceneByName("Level 1");
        //检查是否是在编辑环境下, 如果不是编辑环境, 不能在运行前就拖拽额外打开一个场景, 所以也就不会存在二次加载问题
        if (Application.isEditor) {
            //检查Level 1场景是否已经被加载
            //if (loadedLevel.isLoaded) {
                //如果Level 1场景已经被加载了, 将其设置为活跃场景, 然后终止当前Awake方法
                //SceneManager.SetActiveScene(loadedLevel);
                //return;
            //}
            //循环遍历每一个已经加载的场景
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                //通过索引得到对应的场景引用
                Scene loadedScene = SceneManager.GetSceneAt(i);
                //如果得到的场景名称是否包含字符串"Level "
                if (loadedScene.name.Contains("Level ")) {
                    //场景名称包含"Levle "则表示当前存在已加载的关卡场景, 设置该场景为活跃场景, 并终止方法的执行
                    SceneManager.SetActiveScene(loadedScene);
                    //如果已经有某个场景被加载了, 使用该场景的索引值为loadedLevelBuildIndex字段赋值
                    loadedLevelBuildIndex = loadedScene.buildIndex;
                    return;
                }
            }
        }
        //
        //BeginNewGame();
        //加载场景
        //LoadLevel();
        //通过协程调用LoadLevel方法
        //StartCoroutine(LoadLevel());
        //LoadLevel方法现在可以接受一个场景索引作为参数, 加载指定场景
        StartCoroutine(LoadLevel(1));
    }

    void Update()
    {
        //Input.GetKeyDown不会检测持续按键, 也就是在按键弹起之前, 就算是持续按住, 也只在按键刚刚被按下时返回一次true
        if (Input.GetKeyDown(createKey)) {
            //如果createKey代表的按键被按了一次, 就实例化一个prefab预制体
            //Instantiate(prefab);
            //用来处理cube生成的各种逻辑
            CreateShape();
        }
        //原文此处应该是写错了用的GetKey方法, 我改为了GetKeyDown方法
        //去掉该else if分支, 与加载按键的else if代码合并在一起
        else if (Input.GetKeyDown(newGameKey)) {
            //如果没有按下C键, 并且按下了N键, 则执行BeginNewGame方法
            BeginNewGame();
            //开始新游戏后将当其关卡重新加载一遍, 重置所有场景状态
            StartCoroutine(LoadLevel(loadedLevelBuildIndex));
        }
        else if (Input.GetKeyDown(saveKey)) {
            //如果按下了saveKey对应的按键, 则执行Save方法
            //该类不再使用给自己的保存方法
            //Save();
            //通过storage保存自身数据
            //storage.Save(this);
            //将版本号数据作为第二个参数传递个storage.Save方法
            storage.Save(this, saveVersion);
        }
        else if (Input.GetKeyDown(loadKey)) {
            //如果按下loadKey对应按键, 则执行Load方法
            //该类不再使用给自己的加载方法
            //Load();
            //在此处调用BeginNewGame方法将新建游戏的逻辑与加载游戏的逻辑合并在一起
            BeginNewGame();
            //通过storage保存自身数据
            storage.Load(this);
        }
        //新增检测销毁按键是否按下的else if语句块
        else if (Input.GetKeyDown(destroyKey)) {
            //如果按下了destroyKey所代表的按键, 则调用DestroyShape方法
            DestroyShape();
        }
        else {
            //使用关卡场景的数量作为循环次数
            for (int i = 1; i <= levelCount; i++) {
                //KeyCode.Alpha0代表按键0, 将其与1-9的数字相加,就可以得到1-9的按键值
                if (Input.GetKeyDown(KeyCode.Alpha0 + i)) {
                    //每次加载任意关卡场景, 都先调用BeginNewGame方法, 清除所有之前产生的形状
                    BeginNewGame();
                    //如果按下了代表某个场景索引的数字按键, 则加载该场景, 并终止当前脚本Update方法的执行
                    StartCoroutine(LoadLevel(i));
                    return;
                }
            }
        }
    }

    //新增的Update方法, 在FixedUpdate方法中, Tiem.deltaTime每次取值固定, 不受游戏运行帧率的影响
    private void FixedUpdate()
    {
        //遍历存储shapes列表, 即场景中所有已启用的形状
        for (int i = 0; i < shapes.Count; i++) {
            //调用遍历的每一个形状的GameUpdate方法
            shapes[i].GameUpdate();
        }
        //每一次Update时, 计算形状产生的进度增加值, 将其增加到creationProgress字段中
        //从Update中移动到了FixedUpdate中
        creationProgress += Time.deltaTime * CreationSpeed;
        //检测creationProgress的值是否达到了1
        //if (creationProgress == 1f) {
        //由于creationProgress很可能无法通过累加精准的达到1, 所以修改等号判断为大于等于判断
        //if (creationProgress >= 1f) {
        //if改成while, 这样无论进度值此时变成了多少, 只要它还大于等于1, 就执行一次创建形状的过程
        //这样就可以始终正确的随着时间变化而产生应有的形状数量
        while (creationProgress >= 1f) {
            //如果creationProgress达到了1, 重置其为0
            //creationProgress = 0f;
            //由于现在满足条件时, 进度值可能大于1, 所以不再直接重置为0, 而是在原值基础上减去1, 剩余的进度值在后续继续累加
            creationProgress -= 1f;
            //重置creationProgress后, 创建一个新的形状
            CreateShape();
        }
        //随着时间增加自动销毁形状的进度值
        destructionProgress += Time.deltaTime * DestructionSpeed;
        //只要销毁进度值大于等于1, 就不断重复执行自动销毁过程
        while (destructionProgress >= 1f) {
            //每次自动销毁一个形状之前, 将销毁进度-1
            destructionProgress -= 1f;
            //销毁一个形状
            DestroyShape();
        }
    }

    /// <summary>
    ///用来执行开始新游戏的逻辑
    /// </summary>
    void BeginNewGame()
    {
        //开始新游戏时默认自动创建速度为0//CreationSpeed = 0;
        //将自动创建速度字段与对应滑动条的value都设置为0, 下方的写法等同于分别写赋值为0的两句代码
        CreationSpeed = creationSpeedSlider.value = 0;
        //开始新游戏时默认自动销毁速度为0
        //DestructionSpeed = 0;
        //将自动销毁速度字段与对应滑动条的value都设置为0, 下方的写法等同于分别写赋值为0的两句代码
        DestructionSpeed = destructionSpeedSlider.value = 0;
        //将当前随机状态更改为游戏开时保存的随机状态
        Random.state = mainRandomState;
        //基于上述随机状态, 获取一个随机数作为随机种子
        //int seed = Random.Range(0, int.MaxValue);
        //将直接随机的到的结果, 与无缩放的当前时间进行按位异或, 最终结果作为随机种子
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        //Random.InitState方法使用参数作为随机种子计算随机数序列
        Random.InitState(seed);
        //遍历objects列表
        for (int i = 0; i < shapes.Count; i++) {
            //销毁列表中的每一个cube, 由于objects存储的是cube的Transform组件, 所以还需要使用.gameObject属性获取到cube本身
            //Destroy(shapes[i].gameObject);
            //将Destroy方法替换为Reclaim方法, 由形状工厂决定应该回收还是销毁
            //shapeFactory.Reclaim(shapes[i]);
            //现在要根据每个形状自身的记录来决定使用哪个工厂对其进行回收
            shapes[i].Recycle();
        }
        //清空已被销毁的cube的Transform数据
        shapes.Clear();
    }

    //用来处理cube生成的各种逻辑
    void CreateShape()
    {
        //新增变量PersistableObject o    
        //  PersistableObject o = Instantiate(prefab);
        //创建一个随机形状的实例
        //Shape instance = shapeFactory.GetRandom();
        //方法中原来的代码, 除了第一句和最后一句, 全删掉了, 它们的功能通过调用关卡的ConfigureSpawn方法代替
        //Transform t = instance.transform;
        //…
        //instance.Velocity = Random.onUnitSphere * Random.Range(0f, 2f);
        //将需要配置的形状传递给关卡的ConfigureSpawn方法, 完成各种配置, 代替了CreateShape方法中原来相关代码
        //GameLevel.Current.ConfigureSpawn(instance);
        //将新建的cube的Transform组件引用添加到objects中
        //objects.Add(t);
        //objects的类型已经变成了PersistableObject
        //shapes.Add(instance);
        //形状生成的任务已经交给了关卡中的生成区, 
        //Game的CreateShape方法现在只拿到生成的形状并添加到shapes列表中
        shapes.Add(GameLevel.Current.SpawnShape());
    }
}
using UnityEngine;

//保障被附加后的物体同时存在一个Camera组件
[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour {
    //用来指定摄像机要跟随的Transform
    [SerializeField]
    Transform focus;

    //设置摄像机与跟目标之间的距离
    [SerializeField,Range(1f,20f)]
    float distance = 5f;

    //设置摄像机跟随的灵敏度, 0代表最高灵敏度, 摄像机立即与球体位置进行同步; 大于0的情况下, 值越低灵敏度越低
    [SerializeField, Min(0f)]
    float responsiveness = 0f;

    //保存摄像机的朝向角度
    Vector2 orbitAngles = new Vector2(45f, 0f);

    //将focusPoint变量从LateUpdate方法中拿出来, 作为一个字段
    //使用focusPoint存储当前的关注位置点, 使用previousFocusPoint存储上一个关注位置点
    Vector3 focusPoint;
    Vector3 previousFocusPoint;

    //摄像机轨道角速度字段
    [SerializeField, Range(1f, 360f)]
    float rotationSpeed = 90f;

    //设置相机环绕运动时在x轴上的角度变化, 最小30度, 最大60度
    [SerializeField, Range(-89f, 89f)]
    float minVerticalAngle = -30f, maxVerticalAngle = 60f;

    //存储最后一次改变相机X轴角度的时间点
    float lastManualRotationTime;

    //设置用户在水平旋转相机多少秒后才可以进行相机自动对准
    [SerializeField, Min(0f)]
    float alignDelay = 5f;

    [SerializeField, Range(0f, 90f)]
    //当摄像机当前朝向与球体前进方向夹角大于等于
    float alignSmoothRange = 45f;

    //使用该字段存储摄像机引用
    Camera regularCamera;

    [SerializeField]
    //在Inspector中配置该LayerMask列表, 被选中的Layer中的物体才可能引起摄像机推近观察距离
    LayerMask obstructionMask = -1;

    //该属性的get方法将返回盒状投射所需的盒子向量
    //盒状投射是为了解决摄像机防止穿透物体, 但是又距离球体太近这个问题
    Vector3 CameraHalfExtends {
        get {
            //该向量代表盒子尺寸的一半
            Vector3 halfExtends;
            //计算高度
            halfExtends.y = regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            //计算宽度
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            //深度为0
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    //GetAngle方法将二维向量转换为其所在平面内代表的角度
    static float GetAngle(Vector2 direction)
    {
        //Mathf.Acos 将传入的参数值视为余弦值, 并返回该余弦值对应的弧度值
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;        
        //如果向量的x分量小于0, 则说明结果是逆时针角度, 则需要使用360度减去它来得到顺时针角度值
        return direction.x < 0f ? 360f - angle : angle;
    }

    //用来判断是否可以开始进行相机自动对准过程
    bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay) {
            //如果距离lastManualRotationTime记录的时间点还未经过alignDelay秒, 则不进行自动对准, 返回false
            return false;
        }

        //计算到从上一个跟随位置点到当前跟随位置点的运动向量movement
        Vector2 movement = new Vector2(focusPoint.x - previousFocusPoint.x, focusPoint.z - previousFocusPoint.z);
        //计算球体运动向量大小的平方
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.000001f) {
            //如果运动向量大小的平方小于指定的阈值, 则认为球体没有发生移动, 则不进行自动对准, 返回false
            return false;
        }

        //计算球体的前进方向角度, movement / Mathf.Sqrt(movementDeltaSqr) 是用来计算movement的归一化结果的, 原作者的意思是这样利用上之前的向量平方, 比使用Normalize方法更有效率
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        //计算当前摄像机水平角度与球体运动前进方向角度间的最小角度差距的绝对值, Mathf.DeltaAngle方法将得到两个角度值参数直接的最小相差的角度, 比如10度与20度最小相差10度, 370度与20度同样相差10度
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        //使用与手动操作旋转一样的角速度rotationSpeed得到每次自动旋转需要更新的水平角度变化
        //float rotationChange = rotationSpeed * Time.unscaledDeltaTime;
        //使用Time.unscaledDeltaTime与movementDeltaSqr中的较小值来与rotationSpeed相乘去计算角度变化, 进一步印制微小移动下的镜头旋转
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);

        if (deltaAbs < alignSmoothRange) {
            //如果deltaAbs小于alignSmoothRange, 则最终旋转速度要乘以deltaAbs / alignSmoothRange, 也就是降低了实际的旋转速度
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        //当相机水平朝向角度与球体运动方向角度相差大于180-alignSmoothRange时, 也对旋转速度进行限制, 使其由慢渐渐变快
        //我自己改了一下, 原来的代码在180-deltaAbs=0时, 会出现角度不旋转的情况, 因为此时计算出的旋转速度是0度/秒
        else if (180f - deltaAbs < alignSmoothRange) {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }

        //使用计算出的球体前进方向角度为水平方向上的摄像机轨道旋转角度赋值
        //使用MoveTowardsAngle方法与计算出的rotationChange来平滑的进行摄像机自动旋转
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        //Debug.Log("欧耶可以转了嘻嘻");
        //如果距离lastManualRotationTime记录的时间点已经经过了alignDelay秒以上, 表示可以进行自动对准, 返回true
        return true;
    }

    //根据用户输入设置orbitAngles
    bool ManualRotation()
    {
        //使用input向量存的得到两个自定义输入轴的输入数据
        Vector2 input = new Vector2(Input.GetAxis("Vertical Camera"), Input.GetAxis("Horizontal Camera"));
        //常量e, const表示这是一个常量, 只能在初始化时赋值, 不能被其他代码改变值
        const float e = 0.001f;

        if (input.x < -e || input.x > e || input.y < -e || input.y > e) {
            //如果在任意方向上的输入值大于e, 则计算新的orbitAngles值
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            //存储最后一次改变相机X轴角度的时间点
            lastManualRotationTime = Time.unscaledTime;
            //orbitAngles有更改时返回true
            return true;
        }
        //orbitAngles没有更改时返回false
        return false;
    }

    //处理orbitAngles的值, 使得x轴旋转角度处于合法范围内, y轴旋转角度始终转换为360度内的数值
    void ConstrainAngles()
    {
        //处理x轴旋转角度, 使其不会小于最小角度, 也不会最大角度
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);
        if (orbitAngles.y < 0f) {
            //如果y轴旋转角度是负数, 则加上360度
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y > 360f) {
            //如果y轴旋转角度大于360度, 则减去360度
            orbitAngles.y -= 360f;
        }
    }

    //OnValidate方法只会在编辑器环境下自动调用, 调用的条件是脚本初次被加载或是脚本中的字段通过Inspector中被修改时
    void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle) {
            //如果最大角度小于最小角度, 设置最大角度等于最小角度
            maxVerticalAngle = minVerticalAngle;
        }
    }

    //程序运行时, 在Awake方法中初始化focusPoint的值为球体位置
    void Awake()
    {
        //在Awake方法中将脚本所在物体的Camera组件引用赋值给regularCamera
        regularCamera = GetComponent<Camera>();

        //相机初始的跟随点就是球体的初始位置
        focusPoint = focus.position;
        //保障摄像机在最开始的旋转情况与orbitAngles的初始值相匹配
        transform.localRotation = Quaternion.Euler(orbitAngles);
    }

    //LateUpdate方法会在每次Update执行完毕后执行一次
    private void LateUpdate()
    {
        //UpdateFocusPoint方法用来不断修改focusPoint的值
        UpdateFocusPoint();

        //根据orbitAngles得到代表摄像机朝向变化的四元数
        Quaternion lookRotation;

        //ManualRotation方法根据用户输入设置orbitAngles, 有新角度变化则返回true
        //在if判断中新增AutomaticRotation()方法, 注意两个方法的顺序, 先判断是否有手动旋转, 再判断是否有自动旋转
        if (ManualRotation() || AutomaticRotation()) {
            //如果ManualRotation返回true, 说明相机旋转角度发生了改变, 所以要调用ConstrainAngles()方法对新的旋转角度进行约束处理
            ConstrainAngles();
            //使用约束处理后的orbitAngles来计算摄像机新的朝向四元数
            lookRotation = Quaternion.Euler(orbitAngles);
        }
        else {
            //如果ManualRotation返回false, 说明相机旋转角度没有变化, 所以旋转角度与当前一致即可
            lookRotation = transform.localRotation;
        }

        //摄像机的观察方向, 就是自身的z轴正方向, 也就是transform.forward
        //Vector3 lookDirection = transform.forward;
        //根据朝向变化计算摄像机的新朝向
        Vector3 lookDirection = lookRotation * Vector3.forward;

        //定义变量来存储计算出的摄像机最终距离
        float lookDistance;
        //下面if的写法, 是一种语法糖, 直接声明了hit这个变量, 所以可以在后面使用
        //if (Physics.Raycast(focusPoint, -lookDirection, out RaycastHit hit, distance)) {
        //Physics.BoxCast方法功能与RayCast方法类似, 只不过它将沿着指定方向发射出一个盒状区域, 而不是一条射线
        //之所以使用盒状射线代替线状射线, 是为了检测到让摄像机推进到近裁面完全不会与障碍物表面发生穿透的位置
        //if (Physics.BoxCast(focusPoint, CameraHalfExtends, -lookDirection, out RaycastHit hit, lookRotation, distance)) {
        //使用distance - regularCamera.nearClipPlane作为射线长度, 从而只检测跟着位置点到摄像机近裁面之间是否存在物体
        //if (Physics.BoxCast(focusPoint, CameraHalfExtends, -lookDirection, out RaycastHit hit, lookRotation, distance - regularCamera.nearClipPlane)) {
        //新增obstructionMask作为BoxCast方法新增的最后一个参数, 用来指定射线可以检测那些Layer内的物体
        if (Physics.BoxCast(focusPoint, CameraHalfExtends, -lookDirection, out RaycastHit hit, lookRotation, distance - regularCamera.nearClipPlane, obstructionMask)) {
            //如果从跟随目标点向摄像机方向发射的距离为distance的射线击中了什么东西, 那么就让被击中物体的距离作为新的摄像机距离
            //lookDistance = hit.distance;
            //如果盒状射线在跟随位置点与摄像机近裁面之间检测到了物体, 则设置摄像机近裁面处于射线击中的距离处
            lookDistance = hit.distance + regularCamera.nearClipPlane;
        }
        else {
            //如果射线没有击中什么, 则摄像机保持配置的距离不变
            lookDistance = distance;
        }

        //摄像机的位置设置为从focus位置开始, 向负lookDirection方向, 前进distance距离
        //注:由于此时摄像机没有父物体, 所以设置localPosition还是position的效果都是一样的
        //我猜作者这里使用localPosition可能是习惯问题
        //transform.localPosition = focusPoint - lookDirection * distance;
        //新增lookPosition变量存储计算出的摄像机新位置
        //Vector3 lookPosition = focusPoint - lookDirection * distance;
        //摄像机的位置使用lookDistance代替之前的distance来计算
        Vector3 lookPosition = focusPoint - lookDirection * lookDistance;

        //一次性设置摄像机的位置和朝向角度
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    //UpdateFocusPoint方法用来不断修改focusPoint的值
    void UpdateFocusPoint()
    {
        //设置上一个跟随位置点
        previousFocusPoint = focusPoint;

        //用变量p获取到球体的当前位置
        //Vector3 p = focus.position;
        //不再需要变量p, 直接设置新的当前跟随位置点
        focusPoint = focus.position;

        if (responsiveness > 0f) {
            //如果灵敏度设置大于零, 则使用设置计算方法Lerp得到设置摄像机要跟随的位置点
            //focusPoint = Vector3.Lerp(focusPoint, p, responsiveness * Time.deltaTime);
            //focusPoint = Vector3.Lerp(focusPoint, p, distanceSqr * responsiveness * Time.unscaledDeltaTime);
            //使用当前球体位置p于与上次的跟随位置focusPoint直接的距离的平方作为系数, 这样可以让距离小于1时得到更小focusPoint, 距离大于1时得到更大focusPoint
            //float distanceSqr = (focusPoint - p).sqrMagnitude;
            //不再使用变量p来计算distanceSqr
            float distanceSqr = (focusPoint - previousFocusPoint).sqrMagnitude;
            //focusPoint = Vector3.Lerp(focusPoint, p, distanceSqr * Mathf.Pow(responsiveness, Time.unscaledDeltaTime));//responsiveness * Time.unscaledDeltaTime);
            //不再使用变量P来计算focusPoint
            focusPoint = Vector3.Lerp(previousFocusPoint, focusPoint, distanceSqr * responsiveness * Time.unscaledDeltaTime);
        }
        //每次都在if里对focusPoint赋值了, 所以不在需要此处的else处理
        //else {
        //    //如果灵敏度设置不大于0, 则表示摄像机要马上与球体位置同步, 则跟随点就等于球体位置
        //    focusPoint = p;
        //}
    }

    private void Start()
    {
        //Debug.Log("余弦0对应的角度是 :" + Mathf.Acos(0) * Mathf.Rad2Deg + "余弦1对应的角度是 :" + Mathf.Acos(1) * Mathf.Rad2Deg + "余弦10对应的角度是 :" + Mathf.Acos(10) * Mathf.Rad2Deg);
        Debug.Log("10与20相差 :" + Mathf.DeltaAngle(10,20) + " || 370与20相差 :" + Mathf.DeltaAngle(370, 20));
    }
}

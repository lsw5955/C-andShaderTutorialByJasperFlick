using UnityEngine;

public class MovingSphereRigidbody : MonoBehaviour {
    //存储陡峭表面的法线
    Vector3 steepNormal;
    //存储接触到的陡峭表面的数量
    int steepContactCount;
    //标志当前是否处于陡峭表面之上
    bool OnSteep => steepContactCount > 0;

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    public int jumpPhase = 0;

    Vector3 velocity;
    Vector3 desiredVelocity;

    //探测下方地面的范围
    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [SerializeField]
    //该字段代表玩家输入的坐标空间; 第五课增加
    Transform playerInputSpace = default;

    //球体与地面接触时地面的法线
    Vector3 contactNormal;

    Rigidbody body;

    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    //最小地面点积和最小楼梯点积
    float minGroundDotProduct, minStairsDotProduct;

    //bool onGround;
    int groundContactCount;
    //用OnGround属性代替了onGround字段, 使用匿名方法设置OnGround的值, 等同于属性的Set方法
    //bool OnGround => groundContactCount > 0;
    bool OnGround {
        get {
            bool temp = groundContactCount > 0;
            //Debug.Log("OnGround现在是 : " + temp);
            return temp;
        }
    }

    bool desiredJump;

    //自从离开地面后经历了多少次物理更新; 以及跳跃后经历了多少次物理更新
    int stepsSinceLastGrounded, stepsSinceLastJump;


    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1;

    /// <summary>
    /// 检查球体是否被卡在缝隙中了
    /// </summary>
    /// <returns></returns>
    bool CheckSteepContacts()
    {
        if (steepContactCount > 1) {
            steepNormal.Normalize();
            if (steepNormal.y >= minGroundDotProduct) {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获得水平速度在斜坡上的速度投影
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        //下面这个Debug语句能让帧率炸裂到个位数
        //Debug.Log("我是" + (vector - contactNormal * Vector3.Dot(vector, contactNormal)) + "而你是" + Vector3.ProjectOnPlane(vector,contactNormal));
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    /// <summary>
    /// 判断指定的Layer是否存在于stairsMask的设置中
    /// </summary>
    /// <param name="layer">Layer索引</param>
    /// <returns></returns>
    float GetMinDot(int layer)
    {
        //return stairsMask != (1 << layer) ? minGroundDotProduct : minStairsDotProduct;
        return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
    }

    /// <summary>
    /// 
    /// </summary>
    void AdjustVelocity()
    {
        //获得投影平面的x轴和z轴
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;//Vector3.ProjectOnPlane(Vector3.right,contactNormal);//
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;//Vector3.ProjectOnPlane(Vector3.forward, contactNormal);//
        //获得在投影平面两个轴上的投影速度分量
        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        //float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float acceleration;
        if (OnGround) {
            acceleration = maxAcceleration;
        }
        else {
            acceleration = maxAirAcceleration;
        }

        float maxSpeedChange = acceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        //调整运动速度
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    /// <summary>
    /// 计算地面或楼梯角度的最大点积, 也就是余弦
    /// </summary>
    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        OnValidate();
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        GetComponent<Renderer>().material.SetColor("_Color", Color.white * (groundContactCount * 0.25f));
        desiredJump |= Input.GetButtonDown("Jump");
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        //如果playerInputSpace没有分配一个transform组件, 则其值为null, 将会按照false处理, 否则按照true处理
        if (playerInputSpace) {
            //如果playerInputSpace被分配了一个代表自定义坐标空间的transform, 那么就将运动方向设置该transform的本地空间下, 并通过TransformDirection方法将本地方向转换为世界空间下的方向, 使用这个方向向量来计算最终的球体运动速度
            //desiredVelocity = playerInputSpace.TransformDirection(playerInput.x, 0f, playerInput.y) * maxSpeed;
            //直接使用摄像机的本地朝向在世界坐标空间下的方向作为运动方向, 会在该方向与xz平面不平行时降低速度, 所以需要得到摄像机朝向在xz平面的投影向量的归一化结果作为输入数据的自定义空间
            //得到摄像机本地空间 前方向 在世界坐标空间下的向量
            Vector3 forward = playerInputSpace.forward;
            //舍弃前方向的y分量
            forward.y = 0f;
            //将前方向归一化
            forward.Normalize();
            //得到摄像机本地空间 右方向 在世界坐标空间下的向量
            Vector3 right = playerInputSpace.right;
            //舍弃右方向的y分量
            right.y = 0f;
            //将右方向归一化
            right.Normalize();
            //球体的目标运动速度方向就等于摄像机的前方向和右方向与输入数据的乘积之和, 再乘以maxSpeed得到最终速度
            desiredVelocity = (forward * playerInput.y + right * playerInput.x) * maxSpeed;
        }
        //将desiredVelocity赋值的语句放到新增的else语句中
        else {
            desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        }

        //如果球体离开了地面, 设置为白色, 反之设置为黑色
        GetComponent<Renderer>().material.SetColor("_Color", OnGround ? Color.black : Color.white);
    }

    private void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();
        if (desiredJump) {
            desiredJump = false;
            Jump();
        }

        body.velocity = velocity;
        ClearState();
    }

    /// <summary>
    /// 测试测试...
    /// </summary>
    /// <param name="info"></param>
    void TestJump(string info)
    {
        if(Mathf.Abs(body.velocity.magnitude) >0) {
            //Debug.Log(info);
        }
    }

    /// <summary>
    /// 获取球体的当前速度, 并根据球体是否位于地面重置jumpPhase值
    /// </summary>
    void UpdateState()
    {
        stepsSinceLastJump += 1;
        stepsSinceLastGrounded += 1;
        velocity = body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts()) {
            stepsSinceLastGrounded = 0;

            //测试一下
            TestJump("还是给重置了老弟, 接触地面的数量groundContactCount是" + groundContactCount);
            //避免错误的落地判断
            //if (stepsSinceLastJump > 1) {
                jumpPhase = 0;
            //}

            if (groundContactCount > 1) {
                contactNormal.Normalize();
            }
        }
        else {
            //球体在空中时, 设置其接触面法线方向位垂直向上, 也就是代表了跳跃方向
            contactNormal = Vector3.up;
        }
    }

    /// <summary>
    /// 球体进行一次跳跃
    /// </summary>
    void Jump()
    {
        Vector3 jumpDirection;

        if (OnGround) {
            jumpDirection = contactNormal;
        }
        else if (OnSteep) {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps) {
            if (jumpPhase == 0) {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else {
            return;
        }

        //if (OnGround || jumpPhase < maxAirJumps) {
        stepsSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        //跳跃方向增加向上的向量, 结果归一化, 使得跳跃始终偏上
        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        //velocity += contactNormal * jumpSpeed;
        velocity += jumpDirection * jumpSpeed;
        //}
    }

    /// <summary>
    ///  重置地面接触状态和陡峭面接触状态并重置对应法线字段
    /// </summary>
    void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;

        steepContactCount = 0;
        steepNormal = Vector3.zero;
    }

    /// <summary>
    /// 将球体贴合停靠在地面上
    /// </summary>
    /// <returns>依靠方法才让球体处于地面时返回true</returns>
    bool SnapToGround()
    {
        //Debug.Log("外面stepsSinceLastGrounded = "+ stepsSinceLastGrounded + " || stepsSinceLastJump = " + stepsSinceLastJump);
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2) {
            //Debug.Log("=============================================================里面stepsSinceLastGrounded = " + stepsSinceLastGrounded + " || stepsSinceLastJump = " + stepsSinceLastJump);
            return false;
        }

        //大于临界速度则不贴合地面 直接起飞
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed) {
            return false;
        }

        //如果射线没有检测到任何物体, 则表示下方没有地面, 也就不需要执行停靠方法
        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask)) {
            return false;
        }

        //如果击中表面的法线y小于设定的临界点积数, 表示表面的倾斜角度过大, 不被视作地面或楼梯
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer)) {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        //float speed = velocity.magnitude;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f) {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
        TestJump("OnCollisionStay执行了老弟, 接触地面的数量groundContactCount是" + groundContactCount);
    }

    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minDot) {
                groundContactCount += 1;
                contactNormal += normal;
            }
            //使用-0.01代替0来判断是否属于陡峭表面
            else if (normal.y >= -0.01f) {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }
}

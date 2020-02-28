using UnityEngine;

//继承PersistableObject类, 从而获得保存自身数据的功能
public class RotatingObject : PersistableObject {
    [SerializeField]
    //存储自身旋转的角速度
    Vector3 angularVelocity = Vector3.zero;

    //void Update() {
    //在FixedUpdate方法中, Tiem.deltaTime每次取值固定, 不受游戏运行帧率的影响
    void FixedUpdate()
    {
        //根据角速度, 每帧进行旋转
        transform.Rotate(angularVelocity * Time.deltaTime);
    }
}
using UnityEngine;

public class SphereSpawnZone : SpawnZone {
    [SerializeField]
    //该字段为true表示只在生成区代表的三维形状的表面选择生成位置, 反之则生成位置会在整个三维形状空间范围内选择
    bool surfaceOnly = false;

    //返回形状生成位置, 只有get方法, 所以是只读的
    public override Vector3 SpawnPoint {
        get {
            //返回半径为5的球体内一随机位置
            //return Random.insideUnitSphere * 5f;
            //在随机球体位置基础上, 再加上脚本所属游戏对象的世界坐标系位置
            //return Random.insideUnitSphere * 5f + transform.position;
            //TransformPoint方法, 会将游戏对象本地空间坐标系下的位置转换为世界空间坐标系下的位置
            //本地空间坐标系, 即以该游戏对象的当前位置为坐标原点
            //return transform.TransformPoint(Random.insideUnitSphere);
            //surfaceOnly为true, 调用Random.onUnitSphere, 否则调用Random.insideUnitSphere
            return transform.TransformPoint(
                surfaceOnly ? Random.onUnitSphere : Random.insideUnitSphere
            );
        }
    }

    //Unity每次绘制场景窗口图像时都会调用该方法
    void OnDrawGizmos()
    {
        //设置接下来要绘制的Gizmos为青色
        Gizmos.color = Color.cyan;
        //将Gizmo使用的坐标系设置为所属游戏对象的本地坐标系
        Gizmos.matrix = transform.localToWorldMatrix;
        //绘制一个单位球体Gizmos线框
        Gizmos.DrawWireSphere(Vector3.zero, 1f);
    }
}

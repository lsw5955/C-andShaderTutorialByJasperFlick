using UnityEngine;

//该脚本将用来定义立方体生成区
public class CubeSpawnZone : SpawnZone {
    [SerializeField]
    //该字段控制是否只在代表生成区的三维区域的表面选择生成位置
    bool surfaceOnly = false;

    //重写父类的抽象属性SpawnPoint
    public override Vector3 SpawnPoint {
        get {
            //变量p, 存储获得的随机立方体内坐标位置
            Vector3 p;
            //获取单位立方体空间的随机x轴坐标
            p.x = Random.Range(-0.5f, 0.5f);
            //获取单位立方体空间的随机y轴坐标
            p.y = Random.Range(-0.5f, 0.5f);
            //获取单位立方体空间的随机z轴坐标
            p.z = Random.Range(-0.5f, 0.5f);
            //检查是否只在区域表面选择生成位置
            if (surfaceOnly) {
                //如果只在区域表面选择生成位置, 首先随机确定用来向表面停靠位置点的轴的索引
                int axis = Random.Range(0, 3);
                //以axis的值作为索引获取生成位置点对应坐标轴的值, 如果该值为负数, 则将其设置为-0.5, 否则设置为0.5
                p[axis] = p[axis] < 0f ? -0.5f : 0.5f;
            }
            //将得到的单位立方体上随机位置从所属对象的本地坐标系转换为世界坐标系
            return transform.TransformPoint(p);
        }
    }

    //每次场景窗口创建绘制图像时调用
    void OnDrawGizmos()
    {
        //设置Gizomo颜色
        Gizmos.color = Color.cyan;
        //设置Gizomo坐标系为所属对象的本地坐标系
        Gizmos.matrix = transform.localToWorldMatrix;
        //绘制中心点本地坐标在(0,0,0), 三条边长度为(1,1,1)的立方体
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
using UnityEngine;

//实现震荡行为的类
public sealed class OscillationShapeBehavior : ShapeBehavior {
    public override ShapeBehaviorType BehaviorType {
        get {
            return ShapeBehaviorType.Oscillation;
        }
    }
    //代表震荡的幅度
    public Vector3 Offset { get; set; }
    //代表震荡频率 次/秒
    public float Frequency { get; set; }
    //记录上一次震荡偏移情况
    float previousOscillation;

    public override void GameUpdate(Shape shape) {
        //根据频率与当前时间, 计算特定的正弦值
        //float oscillation = Mathf.Sin(2f * Mathf.PI * Frequency * Time.time);
        //使用形状年龄代替游戏当前时间计算正弦结果
        float oscillation = Mathf.Sin(2f * Mathf.PI * Frequency * shape.Age);
        //将正弦结果与偏移向量相乘, 得到形状的新位置
        //将震荡行为的位置变化作用在形状当前位置上
        //shape.transform.localPosition += oscillation * Offset;
        //添加新的震荡偏移时减去上一次的震荡偏移
        shape.transform.localPosition += (oscillation - previousOscillation) * Offset;
        //记录本次震荡偏移的值作为后续的"上一次偏移情况"
        previousOscillation = oscillation;
    }
    public override void Save(GameDataWriter writer)
    {
        //在保存震荡行为数据时, 依次写入震荡幅度, 震荡频率和上次震荡偏移
        writer.Write(Offset);
        writer.Write(Frequency);
        writer.Write(previousOscillation);
    }

    public override void Load(GameDataReader reader)
    {
        //在加载震荡行为数据时, 依次读取震荡幅度, 震荡频率和上次震荡偏移
        Offset = reader.ReadVector3();
        Frequency = reader.ReadFloat();
        previousOscillation = reader.ReadFloat();
    }

    public override void Recycle()
    {
        //行为回收后与之前的状态无关了, 应该将记录的上一次震荡偏移重置为0
        previousOscillation = 0f;
        ShapeBehaviorPool<OscillationShapeBehavior>.Reclaim(this);
    }
}
using UnityEngine;

//所有形状行为脚本组件的抽象基类
public abstract class ShapeBehavior : MonoBehaviour {
    //该抽象方法主要用于更新形状行为的状态, abstract, 前面教程提到过, 声明"抽象"的关键字
    //shape参数用来指定要做出该行为的形状
    public abstract void GameUpdate(Shape shape);
    //定义保存行为配置与状态数据的抽象方法
    public abstract void Save(GameDataWriter writer);
    //定义加载行为配置与状态数据的抽象方法
    public abstract void Load(GameDataReader reader);
}
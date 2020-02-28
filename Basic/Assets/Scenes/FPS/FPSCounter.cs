using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    //记录最高帧率
    public int HighestFPS { get; private set; }
    //记录最低帧率
    public int LowestFPS { get; private set; }

    public int frameRange;

    //存储多次计算出的FPS
    int[] fpsBuffer;
    //存储下一次的FPS存储的数组位置索引
    int fpsBufferIndex;

    public int AverageFPS {
        get;
        private set;
    }

    void InitializeBuffer() {
        if (frameRange <= 0) {
            frameRange = 1;
        }
        fpsBuffer = new int[frameRange];
        fpsBufferIndex = 0;
    }

    void UpdateBuffer() {
        fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);

        if (fpsBufferIndex >= frameRange) {
            fpsBufferIndex = 0;
        }
    }

    void CalculateFPS() {
        int sum = 0;
        //新增变量获取最高值
        int highest = 0;
        //新增变量获取最低值
        int lowest = int.MaxValue;
        for (int i = 0; i < frameRange; i++) {
            //新增变量存储每次求和时用到的FPS值
            int fps = fpsBuffer[i];
            //sum += fpsBuffer[i];        
            sum += fps;

            //判断本次使用的FPS是否比目的最大值大, 如果是, 则将最大值设置本次FPS
            if (fps > highest) {
                highest = fps;
            }
            //判断本次使用的FPS是否比目的最小值小, 如果是, 则将最小值设置本次FPS
            if (fps < lowest) {
                lowest = fps;
            }
        }
        AverageFPS = sum / frameRange;
        //将找到的最大FPS赋值给HighestFPS
        HighestFPS = highest;
        //将找到的最小FPS赋值给LowestFPS
        LowestFPS = lowest;
    }

    // Update is called once per frame
    void Update()
    {
        //AverageFPS = (int)(1f / Time.deltaTime);
        //为null表示程序刚刚运行, 缓存数组长度与frameRange不同表示运行时修改了frameRange的值
        if (fpsBuffer == null || fpsBuffer.Length != frameRange) {
            InitializeBuffer();
        }
        //更新缓存数组方法, 下文会介绍
        UpdateBuffer();
        //计算平均FPS方法, 下文会介绍
        CalculateFPS();
    }
}

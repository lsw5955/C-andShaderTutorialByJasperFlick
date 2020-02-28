using UnityEngine;

public class NucleonSpawner : MonoBehaviour {
    //生成时间间隔
    public float timeBetweenSpawns;
    //生成的位置距离
    public float spawnDistance;
    //核体数组, 存储可以选择生成的核体
    public Nucleon[] nucleonPrefabs;

    //新增记录上一次产生核体后经过的世时间
    float timeSinceLastSpawn;


    private void Awake() {
        Application.targetFrameRate = -1;
        Debug.Log("啊");
    }

    //新增FixedUpdate方法
    void FixedUpdate() {
        //每次FixedUpdate执行都对timeSinceLastSpawn累加经过的时间, Time.deltaTime的值就是每一帧的时间间隔
        timeSinceLastSpawn += Time.deltaTime;
        //如果经过的时间大于我们设置的时间间隔, 就产生新的核体, 并重置timeBetweenSpawns为0, 重新开始记录时间
        if (timeSinceLastSpawn >= timeBetweenSpawns) {
            timeSinceLastSpawn -= timeBetweenSpawns;
            SpawnNucleon();
        }
    }

    //新增方法SpawnNucleon
    void SpawnNucleon() {
        //随机选择预制体
        Nucleon prefab = nucleonPrefabs[Random.Range(0, nucleonPrefabs.Length)];
        //创建上述预制体实例, 代表生成的核子
        Nucleon spawn = Instantiate<Nucleon>(prefab);
        //设置核子的位置
        spawn.transform.localPosition = Random.onUnitSphere * spawnDistance;
    }
}
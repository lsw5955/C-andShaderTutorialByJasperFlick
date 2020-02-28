using UnityEngine;
using System.Collections;

public class Fractal : MonoBehaviour {
    public int maxDepth;
    private int depth;
    public float childScale;
    public Mesh[] meshes;
    public Material material;
    //材质数组
    private Material[,] materials;
    public float spawnProbability;

    //最大旋转速度
    public float maxRotationSpeed;
    //存储随机后获得的旋转速度值
    private float rotationSpeed;

    public float maxTwist;

    private void InitializeMaterials() {
        materials = new Material[maxDepth + 1,2];
        for(int i=0;i<=maxDepth;i++) {
            float t = i / (maxDepth-1f);
            Debug.Log(i / (maxDepth - 1) + " / " + i / (maxDepth - 1f));
            t *= t;
            materials[i,0] = new Material(material);
            materials[i,0].color = Color.Lerp(Color.white, Color.yellow, t);
            materials[i, 1] = new Material(material);
            materials[i, 1].color = Color.Lerp(Color.white, Color.cyan, t);
        }

        materials[maxDepth,0].color = Color.magenta;
        materials[maxDepth, 1].color = Color.red;
    }


    private void Start() {
        transform.Rotate(Random.Range(-maxTwist, maxTwist), 0, 0);
        rotationSpeed = Random.Range(-maxRotationSpeed, maxRotationSpeed);
        if (materials == null) {
            InitializeMaterials();
        }
        gameObject.AddComponent<MeshFilter>().mesh = meshes[Random.Range(0,meshes.Length)];
        gameObject.AddComponent<MeshRenderer>().material = materials[depth,Random.Range(0,2)];
        if (depth < maxDepth) {
            StartCoroutine(CreateChildre());
        }
    }

    private void Update() {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    private static Vector3[] childDirections = {
        Vector3.up,
        Vector3.right,
        Vector3.left,
        Vector3.forward,
        Vector3.back
    };

    private static Quaternion[] childQuaternions = {
        Quaternion.identity,
        Quaternion.Euler(0,0,-90),
        Quaternion.Euler(0, 0, 90),
        Quaternion.Euler(90, 0, 0),
        Quaternion.Euler(-90,0,0)
    };

    private void Initialize(Fractal parent, int i) {
        maxTwist = parent.maxTwist;
        maxRotationSpeed = parent.maxRotationSpeed;
        spawnProbability = parent.spawnProbability;
        meshes = parent.meshes;
        //material = parent.material;
        materials = parent.materials;
        maxDepth = parent.maxDepth;
        depth = parent.depth + 1;
        childScale = parent.childScale;
        transform.parent = parent.transform;
        transform.localScale = Vector3.one * childScale;
        //transform.localPosition = direction * (0.5f + 0.5f * childScale);
        transform.localPosition = childDirections[i] * (0.5f + 0.5f * childScale);
        //transform.localRotation = quaternion;
        transform.localRotation = childQuaternions[i];
    }

    private IEnumerator CreateChildre() {
        if(Random.value < spawnProbability) {
            for (int i = 0; i < childDirections.Length; i++) {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
                new GameObject("Fractal Child").AddComponent<Fractal>().Initialize(this, i);
            }
        }        
    }
}
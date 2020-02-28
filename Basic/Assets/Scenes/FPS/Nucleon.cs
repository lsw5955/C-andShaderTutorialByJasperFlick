using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Nucleon : MonoBehaviour
{
    //代表引力的字段
    public float attractionForce;
    //代表刚体的字段
    Rigidbody body;
    void Awake() {
        //用body存储物体的刚体组件
        body = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        //对刚体施加作用力
        body.AddForce(transform.localPosition * -attractionForce);
    }
}

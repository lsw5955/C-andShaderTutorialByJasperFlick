using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticDestroy : MonoBehaviour
{

    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = Vector3.right;
        Destroy(rb.gameObject, 5);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rb.bodyType = RigidbodyType2D.Static;
        Destroy(rb.gameObject, 3);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.F4)) {
            rb.bodyType = RigidbodyType2D.Static;
        }
        if (Input.GetKeyDown(KeyCode.F1)) {
            rb.velocity = Vector3.right;
        }

        if (Input.GetKeyDown(KeyCode.F2)) {
            Destroy(rb.gameObject);
        }

        if (Input.GetKeyDown(KeyCode.F3)) {
            Destroy(rb.gameObject, 3);
        }
    }
}

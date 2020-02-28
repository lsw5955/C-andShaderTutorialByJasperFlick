using UnityEngine;

public class Graph : MonoBehaviour
{
    const float pi = Mathf.PI;
    static GraphFunction[] functions = {
        SineFunction,
        Sine2DFunction,
        MultiSineFunction,
        MultiSine2DFunction,
        Ripple,
        Cylinder,
        Sphere,
        Torus
    };

    //[Range(0, 1)]
    //public int function;
    public GraphFunctionName function;

    Transform[] points;
    public Transform pointPrefab;
    [Range(10,100)]
    public int resolution = 10;

    static Vector3 Torus(float u, float v, float t) {
        Vector3 p;
        //float r1 = 1f;
        float r1 = 0.65f + Mathf.Sin(pi * (6f * u + t)) * 0.1f;
        //float r2 = 1f;
        float r2 = 0.2f + Mathf.Sin(pi * (4f * v + t)) * 0.05f;
        float s = r2 * Mathf.Cos(pi * v) + r1;
        p.x = s * Mathf.Sin(pi * u);
        p.y = r2*Mathf.Sin(pi * v);
        p.z = s * Mathf.Cos(pi * u);
        return p;
    }

    static Vector3 Sphere(float u, float v, float t) {
        Vector3 p;
        //float r = Mathf.Cos(pi * 0.5f * v);
        float r = 0.8f + Mathf.Sin(pi * (6f * u + t)) * 0.1f;
        //
        r += Mathf.Sin(pi * (4f * v + t)) * 0.1f;
        //
        float s = r * Mathf.Cos(pi * 0.5f * v);
        //p.x = r * Mathf.Sin(pi * u);
        p.x = s * Mathf.Sin(pi * u);
        //p.y = Mathf.Sin(pi * v *0.5f);
        p.y = r * Mathf.Sin(pi * 0.5f * v);
        //p.z = r * Mathf.Cos(pi * u);
        p.z = s * Mathf.Cos(pi * u);
        return p;    
    }

    static Vector3 Ripple(float x, float z, float t) {
        Vector3 p;
        float d = Mathf.Sqrt(x * x + z * z);
        p.y = Mathf.Sin(pi * (4f * d - t));
        p.y /= 1f + 10 * d;
        p.x = x;
        p.z = z;
        return p;
    }

    static Vector3 MultiSine2DFunction(float x, float z, float t) {
        Vector3 p;
        p.y = 4 * Mathf.Sin(x + z + t * 0.5f);
        p.y += Mathf.Sin(pi * (x + t));
        p.y += Mathf.Sin(2 * pi * (z + 2 * t)) * 0.5f;
        p.y *= 1f / 5.5f;
        p.x = x;
        p.z = z;
        return p;
    }

    static Vector3 Sine2DFunction(float x, float z, float t) {
        Vector3 p;
        p.x = x;
        p.y = Mathf.Sin(pi * (x + t));
        p.y += Mathf.Sin(pi * (z + t));
        p.y *= 0.5f;
        p.z = z;
        return p;
    }

    static Vector3 SineFunction(float x, float z,float t) {
        //return Mathf.Sin(pi * (x + t));
        Vector3 p;
        p.x = x;
        p.y = Mathf.Sin(pi * (x + t));
        p.z = z;
        return p;
    }

    static Vector3 MultiSineFunction(float x, float z,float t) {
        Vector3 p;
        p.y = Mathf.Sin(pi * (x + t));
        p.y += Mathf.Sin(2f * pi * (x + 2f*t))/2;
        p.y *= 2f / 3f;
        p.x = x;
        p.z = z;
        return p;
    }

    static Vector3 Cylinder(float u,float v,float t) {
        Vector3 p;
        //float r = 1f + Mathf.Sin(2f*pi*v) * 0.2f;
        float r = 0.8f + Mathf.Sin(pi * (6f * u + 2f * v + t)) * 0.2f;
        p.x = r * Mathf.Sin(pi * u);
        p.y = v;
        p.z = r * Mathf.Cos(pi * u);
        return p;
    }

    private void Awake() {
        points = new Transform[resolution * resolution];
        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;
        //Vector3 position;
        //position.y = 0f;
        //position.z = 0f;

        //for (int i = 0, z = 0; z < resolution; z++) {
        //    position.z = (z + 0.5f) * step - 1f;
        //    for (int x = 0; x < resolution; x++, i++) {
        //        Transform point = Instantiate(pointPrefab);
        //        point.gameObject.name = i.ToString();
        //        position.x = (x + 0.5f) * step - 1f;
        //        point.localPosition = position;
        //        point.localScale = scale;
        //        point.SetParent(transform, false);
        //        points[i] = point;
        //    }
        //}

        for(int i = 0;i<points.Length;i++) {
            Transform point = Instantiate(pointPrefab);
            point.localScale = scale;
            point.SetParent(transform, false);
            points[i] = point;
        }
    }

    private void Update() {
        float t = Time.time;
        GraphFunction f;
        f = functions[(int)function];

        //for(int i = 0; i< points.Length;i++) {
        //    Transform point = points[i];
        //    Vector3 position = point.localPosition;
        //    position.y = f(position.x, position.z, t);
        //    point.localPosition = position;
        //}
        float step = 2f / resolution;
        for(int i=0,z=0;z < resolution;z++) {
            float v = (z + 0.5f) * step - 1f;
            for (int x = 0; x<resolution;x++,i++) {
                float u = (x + 0.5f) * step - 1f;
                points[i].position = f(u, v, t);
            }
        }

    }    
}

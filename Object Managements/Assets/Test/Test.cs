using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class StaticGenericClassTest<T> {

    public static string info = "泛型静态类字符串";

    static StaticGenericClassTest()
    {
        Debug.Log(typeof(T));
        Debug.Log("泛型静态构造被调用了");
    }
}

static class StaticClassTest {

    public static string info = "静态类字符串";

    static StaticClassTest()
    {
        Debug.Log("静态构造被调用了");
    }
}

class StaticMemberTest {
    static int x;
    static StaticMemberTest()
    {
        Debug.Log("static StaticMemberTest()被调用了");
    }    
}

class ChildStaticMemberTest : StaticMemberTest {

}

public class Test : MonoBehaviour{
    private void Start()
    {
    }

    void TestDebug()
    {
        Debug.Log(StaticGenericClassTest<Vector3>.info);
        Debug.Log(StaticGenericClassTest<Vector2>.info);
        Debug.Log(StaticGenericClassTest<Vector3>.info);
        Debug.Log(StaticGenericClassTest<Vector2>.info);
        Debug.Log(StaticClassTest.info);
        Debug.Log(StaticClassTest.info);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) {
            TestDebug();
        }
    }
}

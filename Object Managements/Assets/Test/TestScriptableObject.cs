using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TestScriptableObject : ScriptableObject {
    string s = "我诞生了";
    public string ss = "我SS也诞生了";
    public string kkb = "我倒是要看看有没有变化";

    public string S {
        get {
            return s;
        }
        set {
            s = value;
        }
    }

    public string SS {
        get {
            return ss;
        }
        set {
            ss = value;
        }
    }

    public string KKB {
        get {
            return kkb;
        }
        set {
            kkb = value;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickCleanup : MonoBehaviour {
    public float Lifetime = 1f;
    void Start(){ Destroy(this.gameObject, Lifetime); }
}

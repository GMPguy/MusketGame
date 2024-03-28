using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRui : MonoBehaviour {
    public UnityEvent hoverFunction;
    public UnityEvent function;
    virtual public void Hover () { hoverFunction.Invoke(); }
    virtual public void Click () { function.Invoke(); }
}

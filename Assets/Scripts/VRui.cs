using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRui : MonoBehaviour {
    public UnityEvent function;
    bool hasClicked = false;
    virtual public void Click (float Pinch) { 
        if(Pinch > 0.6f && !hasClicked) {
            function.Invoke();
            hasClicked = true;
        } else if (Pinch < 0.3f && hasClicked) {
            hasClicked = false;
        }
    }
}

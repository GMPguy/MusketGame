using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class LunetaScript : MonoBehaviour {

    public Camera LunetaCamera;
    public Transform Head, View;
    TrackedPoseDriver HP;
    public Transform[] Targets;
    public float FOV;
    public float BarrelLenght;

    void Start(){
        Head = GameObject.Find("Main Camera").transform;
        Disable();
    }

    void LateUpdate(){
        if (Targets.Length > 0){
            this.transform.position = Targets[0].position;
            this.transform.LookAt(Targets[0].position + Targets[0].forward, Vector3.up);
            this.transform.localScale = new Vector3(BarrelLenght, BarrelLenght, 0.05f);
            LunetaCamera.fieldOfView = FOV;
            View.position = Targets[0].GetChild(0).position;
            View.rotation = Targets[0].GetChild(0).rotation;
            Vector3 Scale = Targets[0].GetChild(0).lossyScale;
            Scale.y *= 1.01f;
            View.localScale = Scale;
            float[] Eyes = { Vector3.Dot(Head.position - Targets[0].position, Head.right) / 2f, Vector3.Dot(Head.position - Targets[0].position, Head.up) / 2f};
            LunetaCamera.transform.LookAt(this.transform.position + (this.transform.position - (Head.position+(Head.right*-Eyes[0])+(Head.up*-Eyes[1]))), Vector3.up);
        }
    }

    public void Enable(Transform newTarget){
        List<Transform> nT = new(Targets) {newTarget};
        Targets = nT.ToArray();
        LunetaCamera.enabled = true;
    }

    public void Disable(Transform Spec = default){
        bool enough = true;
        if(Spec != default){
            List<Transform> nT = new(Targets);
            nT.Remove(Spec);
            nT.TrimExcess();
            Targets = nT.ToArray();
            if(Targets.Length > 0) enough = false;
        }

        if(enough){
            LunetaCamera.transform.localEulerAngles = new (180f,0f,0f);
            LunetaCamera.enabled = false;
            View.localScale = Vector3.zero;
        }
    }

}

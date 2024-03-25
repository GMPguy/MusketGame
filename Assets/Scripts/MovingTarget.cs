using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MovingTarget : TargetScript {

    Vector3[] Poses;
    Quaternion[] Angles;
    public float[] Times;
    public bool goBack = false;

    void Start () {
        Poses = new [] {this.transform.position, this.transform.GetChild(0).position};
        Angles = new [] {this.transform.rotation, this.transform.GetChild(0).rotation};
    }

    void Update () {
        int[] mi = {0,1};
        if(Time.timeSinceLevelLoad / (Times[0]+Times[1]) % 2 > 1) mi = new[]{1,0};
        this.transform.position = Vector3.Lerp(Poses[mi[0]], Poses[mi[1]], Time.timeSinceLevelLoad % (Times[0]+Times[1]) / Times[0]);
        this.transform.rotation = Quaternion.Lerp(Angles[mi[0]], Angles[mi[1]], Time.timeSinceLevelLoad % (Times[0]+Times[1]) / Times[0]);
    }

}

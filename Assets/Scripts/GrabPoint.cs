using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPoint : MonoBehaviour {

    public bool isActive = true;
    public float GrabDistance = 1f;
    public bool MultiTask = false;

    public float inhPinch, inchGrab = 0f;
    public Vector3 HandVector;
    public int GrabStatus = 0;
    public bool Changed = false;
    public Transform Hand;
    public int HandIndex;
    public PlayerScript Master;

    public bool checkForGrab(){
        if (Hand == null && GrabStatus == 0) return true;
        else return false;
    }

    public void Grab(Transform newHands, int handIndex){
        if (checkForGrab()){
            Hand = newHands;
            HandIndex = handIndex;
            Master = Hand.parent.parent.GetComponent<PlayerScript>();
            GrabStatus = 1; 
            Changed = true;
        }
    }

    public void Drop(){
        Hand = null; GrabStatus = 0; Changed = true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPoint : MonoBehaviour {

    public ItemScript isItem;
    public bool isActive = true;
    public float GrabDistance = 1f;
    public bool MultiTask = false;
    public bool Switchable = false;
    int GP = -1;

    public float inhPinch, inchGrab = 0f;
    public Vector3[] HandVector;
    public int GrabStatus = 0;
    public bool Changed = false;
    public Transform Hand;
    public int HandIndex;
    public PlayerScript Master;

    void Start(){
        HandVector = new[]{Vector3.zero, Vector3.zero};
        if(this.tag == "Object_Grab") GP = 0;
        else if(this.tag == "Object_Pinch") GP = 1;
    }

    public bool checkForGrab(Vector3 tPos){
        if (Hand == null && GrabStatus == 0 && checkForDist(tPos) <= GrabDistance) return true;
        else return false;
    }

    public float checkForDist(Vector3 tPos){
        return Vector3.Distance(this.transform.position, tPos);
    }

    public void Grab(Transform newHands, int handIndex){
        if (checkForGrab(this.transform.position)){
            Hand = newHands;
            HandIndex = handIndex/2;
            Master = Hand.parent.parent.GetComponent<PlayerScript>();
            Master.Multitask[handIndex/2] = MultiTask;
            Master.CaughtObjects[handIndex] = this.gameObject;
            GrabStatus = 1; 
            Changed = true;
            if(isItem) isItem.HeldBy = Master;
        }
    }

    public void Drop(){
        if(GrabStatus == 1){
            Master.CaughtObjects[HandIndex*2 + GP] = null;
            Master.Multitask[HandIndex] = true;
            Hand = null; 
            GrabStatus = 0; 
            Changed = true;
        }
    }

    public void Switch(Transform otherHand){
        HandIndex = (HandIndex+1)%2;
        Drop();
        Grab(otherHand, HandIndex);
    }

}

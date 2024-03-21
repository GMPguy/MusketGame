using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPoint : MonoBehaviour {

    public ItemScript isItem;
    public bool isActive = true;
    public float GrabDistance = 1f;
    public string GrabAnim;
    public Vector3 GrabOffset;
    public Collider GrabCollider;
    public bool MultiTask = false;
    public bool Switchable = false;
    public AudioSource GrabAudio;
    public AudioClip[] GrabAudioClips;
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
        if(isItem != null) GrabAudio = isItem.ItemSound.GetComponent<AudioSource>();
    }

    public bool checkForGrab(Vector3 tPos){
        if (Hand == null && GrabStatus == 0 && checkForDist(tPos) <= GrabDistance) return true;
        else return false;
    }

    public float checkForDist(Vector3 tPos){
        if(!GrabCollider) return Vector3.Distance(this.transform.position + (transform.right*GrabOffset.x + transform.up*GrabOffset.y + transform.forward*GrabOffset.z), tPos);
        else return Vector3.Distance(GrabCollider.ClosestPoint(tPos), tPos);
    }

    public void Grab(Transform newHands, int handIndex){
        if (checkForGrab(this.transform.position)){
            Hand = newHands;
            HandIndex = handIndex/2;
            Master = Hand.parent.parent.GetComponent<PlayerScript>();
            if(Master.CaughtObjects[handIndex]) Master.CaughtObjects[handIndex].GetComponent<GrabPoint>().Drop();
            Master.Multitask[handIndex/2] = MultiTask;
            Master.CaughtObjects[handIndex] = this.gameObject;
            GrabStatus = 1; 
            Changed = true;
            if(isItem) isItem.HeldBy = Master;
            if(GrabAudio && GrabAudioClips.Length > 0) {
                GrabAudio.transform.position = Hand.position;
                GrabAudio.clip = GrabAudioClips[(int)Random.Range(0f, GrabAudioClips.Length-.1f)];
                GrabAudio.volume = 1f;
                GrabAudio.Play();
            }
        }
    }

    public void Drop(){
        if(GrabStatus == 1){
            Master.CaughtObjects[HandIndex*2 + GP] = null;
            Master.Multitask[HandIndex] = true;
            Master = null;
            Hand = null; 
            GrabStatus = 0; 
            Changed = true;
            if(isItem) isItem.ThrownVelocity = HandVector[1];
        }
    }

    public void Switch(Transform otherHand, int toHand){
        Drop();
        HandIndex = toHand;
        Grab(otherHand, HandIndex);
    }

}

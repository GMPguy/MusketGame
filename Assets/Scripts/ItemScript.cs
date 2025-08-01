using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random=UnityEngine.Random;

public class ItemScript : MonoBehaviour {
    
    protected GameScript GS;
    public GrabPoint simpleHandle;
    public SoundScript ItemSound;
    public string[] DropSounds;
    public Rigidbody Rig;
    protected float[] touching = {1f, 0f}; // touching factor, if is touchin
    bool activated = false;
    public PlayerScript HeldBy;
    public float GrabSmooth = 25f;
    public Vector3 ThrownVelocity;

    void Start () {
        GS = GameObject.FindObjectOfType<GameScript>();
        Rig = this.GetComponent<Rigidbody>();
        if(simpleHandle) simpleHandle.isItem = this;
        ItemStart();
    }

    void Update(){
        if(touching[1] > 0f){
            touching[0] = 0.1f;
            touching[1] -= 0.1f;
        } else touching[0] = 1f;

        if(simpleHandle){
            if(simpleHandle.GrabStatus == 1){
                setPos(true, new[] {simpleHandle.Hand.position, simpleHandle.Hand.position + simpleHandle.Hand.forward, simpleHandle.Hand.up});
            } else {
                setPos(false);
            }
        }

        ItemUpdate();
    }

    void OnCollisionEnter(Collision collision){
        if(DropSounds.Length > 0 && Rig.velocity.magnitude > 0.1f) ItemSound.PlayAudio(DropSounds[(int)Random.Range(0f, DropSounds.Length-0.1f)], Mathf.Clamp(Rig.velocity.magnitude, 0f, 1f), -1, collision.contacts[0].point);
        ItemCollision(collision);
    }

    void OnCollisionStay(Collision collision){
        if(collision.collider.gameObject.layer != 3) touching = new float[]{0.01f, 0.2f};
        ItemCollisionStay(collision);
    }

    protected virtual void ItemStart(){}
    protected virtual void ItemUpdate(){}
    protected virtual void ItemCollision(Collision collision){}
    protected virtual void ItemCollisionStay(Collision collision){}

    public static Vector3 movePivoted(Transform trans, Vector3 Pivot, Vector3 target){
        return target - (Pivot - trans.position);
    }

    protected void setPos(bool Activate, Vector3[] targets = default){
        activated = Activate;
        if(Activate){
            if(HeldBy) this.transform.parent = HeldBy.transform;
            Rig.useGravity = false;
            Rig.angularVelocity = Rig.velocity = Vector3.zero;
            this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position + Vector3.ClampMagnitude(targets[0] - this.transform.position, touching[0]), Time.deltaTime*Mathf.Clamp(GrabSmooth, 0f, Mathf.Lerp(10f, 100f, touching[0])));
            if(targets.Length == 3) this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(targets[1] - this.transform.position, targets[2]), Time.deltaTime*Mathf.Clamp(GrabSmooth, 0f, Mathf.Lerp(10f, 100f, touching[0])*Mathf.Clamp(touching[0]-0.5f, 0.01f, 1f)));
            else this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(targets[1]), Time.deltaTime*10f*Mathf.Clamp(touching[0]-0.5f, 0.01f, 1f));
        } else {
            if(this.transform.parent != null) this.transform.parent = null;
            Rig.useGravity = true;
            if(ThrownVelocity.magnitude > 0.01f) {
                Rig.velocity = ThrownVelocity * 100f;
                ThrownVelocity = Vector3.zero;
            }
        }
    }



}

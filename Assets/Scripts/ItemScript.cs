using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random=UnityEngine.Random;

public class ItemScript : MonoBehaviour {
    
    public GrabPoint simpleHandle;
    public AudioSource ItemSound;
    public string[] DropSounds;
    protected Rigidbody Rig;
    public AudioClip[] ISclips;
    int SoundID;
    protected float[] touching = {1f, 0f}; // touching factor, if is touchin
    bool activated = false;
    public PlayerScript HeldBy;

    void Start () {
        Rig = this.GetComponent<Rigidbody>();
        if(simpleHandle) simpleHandle.isItem = this;
        ItemStart();
    }

    void Update(){
        if(touching[1] > 0f){
            touching[0] = 0.1f;
            touching[1] -= 0.1f;
        } else touching[0] = 1f;

        if(ItemSound.isPlaying == false) SoundID = -9999;

        if(simpleHandle){
            if(simpleHandle.GrabStatus == 1){
                setPos(true, new[] {simpleHandle.Hand.position, simpleHandle.Hand.position + simpleHandle.Hand.forward, simpleHandle.Hand.up});
            } else {
                setPos(false);
            }
        }

        ItemUpdate();
    }

    public void PlayAudio(string AudioName, float Volume = 1f, int Importance = 0, Vector3 AudioPos = default){
        if(Importance >= SoundID && AudioName != ""){
            SoundID = Importance;
            string theAudio = AudioName;
            ItemSound.volume = Volume;
            if(theAudio[0] == '_') theAudio = AudioName[1..];
            if (AudioPos != default) ItemSound.transform.position = AudioPos;
            else ItemSound.transform.position = this.transform.position;
            if (!(AudioName[0] == '_' && ItemSound.isPlaying && ItemSound.clip.name == AudioName[1..])) for(int isc = 0; isc <= ISclips.Length; isc++){
                if(isc == ISclips.Length){
                    Debug.LogError("No item sound clip of name " + AudioName + " found!");
                } else if (ISclips[isc].name == theAudio){
                    ItemSound.clip = ISclips[isc];
                    ItemSound.Play();
                    break;
                }
            }
        } else if (AudioName == ""){
            ItemSound.Stop();
            SoundID = -9999;
        }
    }

    void OnCollisionEnter(Collision collision){
        if(DropSounds.Length > 0 && Rig.velocity.magnitude > 0.1f) PlayAudio(DropSounds[(int)Random.Range(0f, DropSounds.Length-0.1f)], Mathf.Clamp(Rig.velocity.magnitude, 0f, 1f), -1, collision.contacts[0].point);
        ItemCollision(collision);
    }

    void OnCollisionStay(Collision collision){
        touching = new float[]{0.01f, 0.2f};
        ItemCollisionStay(collision);
    }

    public virtual void ItemStart(){}
    public virtual void ItemUpdate(){}
    public virtual void ItemCollision(Collision collision){}
    public virtual void ItemCollisionStay(Collision collision){}

    public static Vector3 movePivoted(Transform trans, Vector3 Pivot, Vector3 target){
        return target - (Pivot - trans.position);
    }

    protected void setPos(bool Activate, Vector3[] targets = default){
        activated = Activate;
        if(Activate){
            if(HeldBy) this.transform.parent = HeldBy.transform;
            Rig.useGravity = false;
            Rig.angularVelocity = Rig.velocity = Vector3.zero;
            this.transform.position = Vector3.Lerp(this.transform.position, targets[0], Time.deltaTime*10f*touching[0]);
            if(targets.Length == 3) this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(targets[1] - this.transform.position, targets[2]), Time.deltaTime*10f* touching[0]);
            else this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(targets[1]), Time.deltaTime*10f* touching[0]);
        } else {
            if(this.transform.parent != null) this.transform.parent = null;
            Rig.useGravity = true;
        }
    }



}

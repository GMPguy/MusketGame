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
    float touching = 0f;

    void Start () {
        Rig = this.GetComponent<Rigidbody>();
        ItemStart();
    }

    void Update(){
        touching = Mathf.Clamp(touching + Time.deltaTime * 100f, 0f, 1f);

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
        if(Importance >= SoundID && AudioName != "" && !(AudioName[0] == '_' && ItemSound.isPlaying && ItemSound.clip.name == AudioName[1..])){
            SoundID = Importance;
            string theAudio = AudioName;
            if(theAudio[0] == '_') theAudio = AudioName[1..];
            if (AudioPos != default) ItemSound.transform.position = AudioPos;
            for(int isc = 0; isc <= ISclips.Length; isc++){
                if(isc == ISclips.Length){
                    Debug.LogError("No item sound clip of name " + AudioName + " found!");
                } else if (ISclips[isc].name == theAudio){
                    ItemSound.clip = ISclips[isc];
                    ItemSound.volume = Volume;
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
        touching = -Time.deltaTime;
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
        if(Activate){
            Rig.useGravity = false;
            Rig.velocity = Rig.angularVelocity = Vector3.zero;
            this.transform.position = Vector3.Lerp(this.transform.position, targets[0], Time.deltaTime*10f*touching);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(targets[1] - this.transform.position, targets[2]), Time.deltaTime*20f*(touching-0.5f));
        } else {
            Rig.useGravity = true;
        }
    }



}

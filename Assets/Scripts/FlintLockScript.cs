using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using Random=UnityEngine.Random;

public class FlintLockScript : ItemScript {

    public GrabPoint Handle, Holder;
    public Transform trigger, powder, Slimend;
    public GrabPoint cock, frizzen, rammingRod;
    public float[] triggerRot, cockRot, frizzenRot = {0.5f, -90f, 90f};
    public float loadedPowder = 0f;
    float cockPosition , prevCock, prevFrizzen, fired, heat, rrPos, rrSlide = 0f;
    int ignite = 0;

    public Vector4[] rrPositions;
    public float[] insertBullet = {1f, 1f};
    string insertedBullet = "";

    public override void ItemUpdate(){
        gunHandling();
        gunMechanics();
    }

    void gunHandling(){

        if(Handle.GrabStatus == 1){
            triggerRot[0] = Handle.inhPinch;
            if(Holder && Holder.GrabStatus == 1){
                Holder.Master.handAnimate("FlintlockHold", Holder.HandIndex, new Vector3[]{Holder.transform.position, Holder.transform.eulerAngles});
                setPos(true, new[]{Handle.Hand.position, Holder.Hand.position, Handle.Hand.up});
            } else {
                setPos(true, new[]{Handle.Hand.position, this.transform.position + Handle.Hand.forward, Handle.Hand.up});
            }
            Handle.Master.handAnimate("FlintlockHandle", Handle.HandIndex, new Vector3[]{Handle.transform.position, Handle.transform.eulerAngles});
        } else if (Holder && Holder.GrabStatus == 1){
            triggerRot[0] = 0f;
            setPos(true, new[]{movePivoted(this.transform, Holder.transform.position, Holder.Hand.position), Holder.Hand.position + Holder.Hand.forward, Holder.Hand.up});
            //this.transform.position = Vector3.Lerp(this.transform.position, adherePivot(Holder.transform.position, Holder.Hand.position), 0.5f);
            Holder.Master.handAnimate("FlintlockHold", Holder.HandIndex, new Vector3[]{Holder.transform.position, Holder.transform.eulerAngles});
        } else {
            triggerRot[0] = 0f;
            setPos(false);
        }

    }

    void gunMechanics(){

        trigger.localEulerAngles = new Vector3(Mathf.Lerp(triggerRot[1], triggerRot[2], triggerRot[0]), 0f, 0f);
        cock.transform.localEulerAngles = new Vector3(Mathf.Lerp(cockRot[1], cockRot[2], cockRot[0]), 0f, 0f);
        frizzen.transform.localEulerAngles = new Vector3(Mathf.Lerp(frizzenRot[1], frizzenRot[2], frizzenRot[0]), 0f, 0f);
        powder.localScale = Vector3.one * Mathf.Clamp(loadedPowder, 0f, 1f);

        // Ramming rod
        if(rrPos < 1f){
            rammingRod.transform.localPosition = new Vector3(rrPositions[0].x, rrPositions[0].y, Mathf.Lerp(rrPositions[0].z, rrPositions[0].z + rrPositions[0].w, rrPos));
            rammingRod.transform.localEulerAngles = new Vector3(-90f, 0f, 0f);
        } else {
            rammingRod.transform.localPosition = new Vector3(rrPositions[1].x, rrPositions[1].y, Mathf.Lerp(rrPositions[1].z, rrPositions[1].z + rrPositions[1].w, rrPos-1f));
            rammingRod.transform.localEulerAngles = new Vector3(-270f, 0f, 0f);
        }
        if(rammingRod.GrabStatus == 1){
            rrSlide = 1f;
            rrPos = Mathf.Clamp(rrPos + Vector3.Dot(-rammingRod.transform.up, cock.HandVector)*10f, 0f, 0.5f + (0.5f * frizzenRot[0]));
            Holder.Master.handAnimate("FlintlockHold", Holder.HandIndex, new Vector3[]{rammingRod.transform.position, rammingRod.transform.eulerAngles});
        } else {
            rrSlide = Mathf.Clamp(rrSlide - Time.deltaTime, 0f, 1f);
            
        }

        if(fired <= 0f){

            // Cock
            if(cock.GetComponent<GrabPoint>().GrabStatus == 1){
                cockRot[0] = Mathf.Clamp(cockRot[0] + Vector3.Dot(this.transform.forward, cock.HandVector)*10f, 0f, 0.5f + (0.5f * frizzenRot[0]));
                if(cockRot[0] < 0.1f) cockPosition = 0f;
                else if (cockRot[0] < 0.9f) cockPosition = 0.5f;
                else cockPosition = 1f;
                if(prevCock != cockPosition){
                    prevCock = cockPosition;
                    PlayAudio("GunCock", 1f, 1, cock.transform.GetChild(0).position);
                }
            } else cockRot[0] = Mathf.MoveTowards(cockRot[0], cockPosition, Time.deltaTime*10f);

            // Frizzen
            if(frizzen.GetComponent<GrabPoint>().GrabStatus == 1){
                frizzenRot[0] = Mathf.Clamp(frizzenRot[0] + Vector3.Dot(this.transform.forward, frizzen.HandVector)*10f, 0f, 1f);
            } else if (frizzenRot[0] < 0.9f) frizzenRot[0] = Mathf.MoveTowards(frizzenRot[0], 0f, Time.deltaTime*10f);

            if((frizzenRot[0] <= 0f && prevFrizzen != 0f) || (frizzenRot[0] >= 1f && prevFrizzen != 1f)){
                PlayAudio("GunFrissen", 1f, 0, cock.transform.GetChild(0).position);
                prevFrizzen = frizzenRot[0];
            }

            // Firing
            if(triggerRot[0] > 0.5f && cockPosition < 0.1f) {
                fired = 1f;
                cock.isActive = frizzen.isActive = false;
            }

            // Ignition
            if(heat > 0f){
                heat -= Time.deltaTime;
            } else if(heat <= 0f && ignite != 0){
                ignite = 0;
                cock.isActive = frizzen.isActive = true;
            }

        } else {

            fired -= Time.deltaTime*30f;
            cockRot[0] = Mathf.Lerp(1f, 0f, fired);
            if(frizzenRot[0] < 0.9f) {
                frizzenRot[0] = Mathf.Lerp(1f, 0f, fired*2f);
                if(ignite == 0) {
                    cock.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                    if(Random.Range(0f, 1f) < loadedPowder){
                        ignite = 1; 
                        heat = Random.Range(0f, 0.25f);
                        powder.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
                        PlayAudio("GunFlint", 1f, 1, cock.transform.GetChild(0).position);
                        loadedPowder = 0f;
                    } else {
                        ignite = 2;
                        PlayAudio("GunFlint", 1f, 1, cock.transform.GetChild(0).position);
                    }
                }
            } else if (ignite == 0) {
                ignite = 2;
                PlayAudio("GunEmpty", 1f, 1, cock.transform.GetChild(0).position);
            }
            cockPosition = 1f;
        }

    }

    public void LoadBullet(string What, Vector3 Where, Vector3 Rot){
        insertedBullet = What;
        insertBullet[0] = 1f;
        foreach(Transform setChild in Slimend.GetChild(0)) {
            if(setChild.name == What) setChild.localScale = Vector3.one;
            else setChild.localScale = Vector3.zero;
        }
    }

}

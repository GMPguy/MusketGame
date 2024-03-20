using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using Random=UnityEngine.Random;

public class FlintLockScript : ItemScript {

    public GameObject Bullet;
    public GrabPoint Handle, Holder;
    Vector3 HolderPos, HolderRot;
    public Transform trigger, powder, Slimend;
    public GrabPoint cock, frizzen, rammingRod;
    public float[] triggerRot, cockRot, frizzenRot = {0.5f, -90f, 90f};
    bool triggerPull = false;
    public float[] loadedPowder, rrPos = {0f, 0f};
    float cockPosition , prevCock, prevFrizzen, fired, heat, rrSlide = 0f;
    int ignite, rrState = 0;

    [System.Serializable] public struct rrFrame {
        public Vector3 rrPos;
        public Vector3 rrRot;
    }
    public rrFrame[] rrPositions;
    Vector3[] prevRR = {Vector3.zero, Vector3.zero};
    public float insertBullet = 1f;
    public string insertedBullet = "";

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
                HolderPos = Holder.transform.position;
                HolderRot = this.transform.eulerAngles;
            } else {
                setPos(true, new[]{Handle.Hand.position, this.transform.position + Handle.Hand.forward, Handle.Hand.up});
            }
            Handle.Master.handAnimate("FlintlockHandle", Handle.HandIndex, new Vector3[]{Handle.transform.position, Handle.transform.eulerAngles});
        } else if (Holder && Holder.GrabStatus == 1){
            triggerRot[0] = 0f;
            HolderPos += Holder.HandVector[0];
            HolderRot += Holder.HandVector[2];
            setPos(true, new[]{movePivoted(this.transform, Holder.transform.position, HolderPos), HolderRot});
            //setPos(true, new[]{movePivoted(this.transform, Holder.transform.position, Holder.Hand.position), Holder.Hand.position + Holder.Hand.forward, Holder.Hand.up});
            //this.transform.position = Vector3.Lerp(this.transform.position, adherePivot(Holder.transform.position, Holder.Hand.position), 0.5f);
            Holder.Master.handAnimate("FlintlockHold", Holder.HandIndex, new Vector3[]{Holder.transform.position, Holder.transform.eulerAngles});
        } else {
            triggerRot[0] = 0f;
            HolderPos = Holder.transform.position;
            HolderRot = this.transform.eulerAngles;
            setPos(false);
        }

    }

    void gunMechanics(){

        trigger.localEulerAngles = new Vector3(Mathf.Lerp(triggerRot[1], triggerRot[2], triggerRot[0]), 0f, 0f);
        cock.transform.localEulerAngles = new Vector3(Mathf.Lerp(cockRot[1], cockRot[2], cockRot[0]), 0f, 0f);
        frizzen.transform.localEulerAngles = new Vector3(Mathf.Lerp(frizzenRot[1], frizzenRot[2], frizzenRot[0]), 0f, 0f);
        powder.localScale = Vector3.one * Mathf.Clamp(loadedPowder[0], 0f, 1f);

        // Ramming rod
        if(rrState == 0) rammingRod.transform.localPosition = Vector3.Lerp(rrPositions[0].rrPos, rrPositions[1].rrPos, rrPos[0]/rrPos[1]);
        else if(rrState == 2) rammingRod.transform.localPosition = Vector3.Lerp(rrPositions[3].rrPos, rrPositions[4].rrPos, rrPos[0]/rrPos[1]);

        if(rrState == 4){
            if(rammingRod.transform.parent == this.transform){
                rammingRod.transform.SetParent(null);
                Rigidbody addRig = rammingRod.AddComponent<Rigidbody>();
                rammingRod.GetComponent<BoxCollider>().isTrigger = false;
                addRig.collisionDetectionMode = CollisionDetectionMode.Continuous;
                addRig.velocity = this.transform.forward * Random.Range(1f, 10f);
            }
            if(rammingRod.GrabStatus == 1){
                rrState = 3;
                rrPos[0] = 0f;
                prevRR = new[]{this.transform.InverseTransformPoint(rammingRod.transform.position), this.transform.InverseTransformPoint(rammingRod.transform.localEulerAngles)};
            }
        } else {
            if(rammingRod.transform.parent != this.transform){
                rammingRod.transform.SetParent(this.transform);
                Destroy(rammingRod.GetComponent<Rigidbody>());
                rammingRod.GetComponent<BoxCollider>().isTrigger = true;
            }

            if(rrState == 0 || rrState == 2){
                float rrpVector = 0f;
                if(rammingRod.GrabStatus == 1){
                    if(rammingRod.Changed && rrPos[0] < 0.1f){
                        rammingRod.Changed = false;
                        rrPos[0] = 0.1f;
                    }
                    rrSlide = 2f;
                    rrpVector = Vector3.Dot(this.transform.forward, rammingRod.HandVector[0]);
                    rrPos[0] = Mathf.Clamp(rrPos[0] + rrpVector, 0f, rrPos[1]*2f);
                    //rammingRod.Master.handAnimate("FlintlockHold", rammingRod.HandIndex, new Vector3[]{rammingRod.transform.position, rammingRod.transform.eulerAngles});
                    if(rrPos[0] > rrPos[1]){
                        prevRR = new[]{rammingRod.transform.localPosition, rammingRod.transform.localEulerAngles};
                        rrPos[0] = 0f;
                        ItemSound.PlayAudio("GunRammingRod2", 1f, 1, rammingRod.transform.position);
                        if(rrState == 0) rrState = 1;
                        else if (rrState == 2) rrState = 3;
                    }
                } else {
                    rrSlide = Mathf.Clamp(rrSlide - Time.deltaTime, 0f, 2f);
                    if(rrPos[0] > 0.1f) rrpVector = Mathf.Lerp(Time.deltaTime*2f, 0f, rrSlide*2f) * Mathf.Clamp((Vector3.Angle(this.transform.forward, Vector3.down)-90f)/90f, 0f, 1f);
                    else rrpVector = 0f;
                    rrPos[0] = Mathf.Clamp(rrPos[0] - rrpVector, 0f, 1f);
                }
                if(rrpVector != 0f) {
                    if(rrPos[0] > 0f) {
                        if(rrPos[0] > 0.1f) ItemSound.PlayAudio("_GunRammingRod", Mathf.Clamp(Mathf.Abs(rrpVector*10f), 0f, 1f), -1, rammingRod.transform.position);
                        if(rrState == 2){
                            insertBullet = Mathf.Clamp(insertBullet, 0f, rrPos[0]/rrPos[1]);
                            Slimend.GetChild(0).localPosition = Vector3.back * (1f-insertBullet) * rrPos[1];
                        }
                    } else if (rrState == 2) {
                        rrPos[0] = 0.1f;
                        ItemSound.PlayAudio("GunRammingRod3", 1f, 1, rammingRod.transform.position);
                        rammingRod.Drop();
                    }
                } else if (ItemSound.clip && ItemSound.clip.name == "GunRammingRod"){
                    ItemSound.PlayAudio("");
                }
            } else {
                rrPos[0] += Time.deltaTime;

                int[] s = {2, 4};
                if(rrState == 3) s = new[] {5, 1};
                if (rrPos[0] < rrPos[1]/2f) {
                    rammingRod.transform.localPosition = Vector3.Slerp(prevRR[0], rrPositions[s[0]].rrPos, rrPos[0] / (rrPos[1]/2f));
                    rammingRod.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(prevRR[1]), Quaternion.Euler(rrPositions[s[0]].rrRot), rrPos[0] / (rrPos[1]/2f));
                } else if (rrPos[0] >= rrPos[1]/2f) {
                    rammingRod.transform.localPosition = Vector3.Slerp(rrPositions[s[0]].rrPos, rrPositions[s[1]].rrPos, (rrPos[0]-0.5f) / (rrPos[1]/2f));
                    rammingRod.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(rrPositions[s[0]].rrRot), Quaternion.Euler(rrPositions[s[1]].rrRot), (rrPos[0]-0.5f) / (rrPos[1]/2f));
                }

                /*Vector3[] switcharoo = new[]{
                    new Vector3(rrPositions[0].x, rrPositions[0].y, rrPositions[0].z + rrPositions[0].w),
                    new Vector3(rrPositions[1].x, rrPositions[1].y, rrPositions[1].z + rrPositions[1].w)
                };
                if(rrState == 1) {
                    rammingRod.transform.localPosition = Vector3.Lerp(prevRR[0], switcharoo[1], rrPos[0] / rrPos[1]);
                    rammingRod.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(prevRR[1]), Quaternion.Euler(new Vector3(-270f, 0f, 0f)), rrPos[0] / rrPos[1]);
                } else if(rrState == 3) {
                    rammingRod.transform.localPosition = Vector3.Lerp(prevRR[0], switcharoo[0], rrPos[0] / rrPos[1]);
                    rammingRod.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(prevRR[1]), Quaternion.Euler(new Vector3(-90f, 0f, 0f)), rrPos[0] / rrPos[1]);
                }*/
                if(rrPos[0] >= rrPos[1]) {
                    rrState = (rrState+1)%4;
                    rrPos[0] = rrPos[1] * 0.9f;
                }
            }
        }

        // trigger
        if(triggerRot[0] > 0.75f && !triggerPull){
            triggerPull = true;
            if(fired <= 0f && cockPosition < 0.1f) {
                fired = 1f;
                cock.isActive = frizzen.isActive = false;
            }
        } else if (triggerRot[0] < 0.25f && triggerPull){
            triggerPull = false;
        }

        if(fired <= 0f){

            // Cock
            if(cock.GetComponent<GrabPoint>().GrabStatus == 1) cockRot[0] = Mathf.Clamp(cockRot[0] + Vector3.Dot(this.transform.forward, cock.HandVector[0])*5f, 0f, 0.5f + (0.5f * frizzenRot[0]));
            else cockRot[0] = Mathf.MoveTowards(cockRot[0], Mathf.Clamp(cockPosition, 0f, 0.5f + frizzenRot[0]*0.5f), Time.deltaTime*10f);

            if(cockRot[0] < 0.1f) cockPosition = 0f;
            else if (cockRot[0] < 0.9f) cockPosition = 0.5f;
            else cockPosition = 1f;
            if(prevCock != cockPosition){
                prevCock = cockPosition;
                ItemSound.PlayAudio("GunCock", 1f, 0, cock.transform.GetChild(0).position);
            }

            // Frizzen
            if(frizzen.GetComponent<GrabPoint>().GrabStatus == 1) frizzenRot[0] = Mathf.Clamp(frizzenRot[0] + Vector3.Dot(this.transform.forward, frizzen.HandVector[0])*20f, 0f, 1f);
            else if (frizzenRot[0] < 0.9f) frizzenRot[0] = Mathf.MoveTowards(frizzenRot[0], 0f, Time.deltaTime*10f);

            if((frizzenRot[0] <= 0f && prevFrizzen != 0f) || (frizzenRot[0] >= 1f && prevFrizzen != 1f)){
                ItemSound.PlayAudio("GunFrissen", 1f, 0, cock.transform.GetChild(0).position);
                prevFrizzen = frizzenRot[0];
            }

            // Ignition
            if(heat > 0f){
                heat -= Time.deltaTime;
            } else if(heat <= 0f && ignite != 0){
                cock.isActive = frizzen.isActive = true;
                if(ignite == 1) FireGun();
                ignite = 0;
            }

        } else {

            fired -= Time.deltaTime*30f;
            cockRot[0] = Mathf.Lerp(1f, 0f, fired);
            if(frizzenRot[0] < 0.9f) {
                frizzenRot[0] = Mathf.Lerp(1f, 0f, fired*2f);
                if(ignite == 0) {
                    cock.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                    ItemSound.PlayAudio("GunFlint", 1f, 1, cock.transform.GetChild(0).position);
                    if(Random.Range(0f, 1f) < loadedPowder[0]){
                        ignite = 1; 
                        heat = Random.Range(0f, 0.5f);
                        powder.transform.GetChild(1).GetComponent<ParticleSystem>().Play();
                        loadedPowder[0] = 0f;
                    } else {
                        ignite = 2;
                    }
                }
            } else if (ignite == 0) {
                ignite = 2;
                ItemSound.PlayAudio("GunEmpty", 1f, 1, cock.transform.GetChild(0).position);
            }
            cockPosition = 1f;
        }

    }

    public void FireGun(){
        if(insertedBullet == "Cartridge" && insertBullet <= 0.1f){
            float firePower = -1f;
            Vector3[] orgPos = new[]{Slimend.position, Slimend.forward};
            if(loadedPowder[1] >= 3f){
                insertedBullet = "";
                this.transform.Rotate(Vector3.right*-30f);
                this.transform.position += Vector3.up/10f;
                touching = new[]{0.1f, 1f};
                Slimend.GetChild(0).localScale = Vector3.zero;
                firePower = (loadedPowder[1]-3f)/3f;
            } else if (loadedPowder[1] > 0f) {
                this.transform.Rotate(Vector3.right*-10f);
                insertedBullet = "Jam";
                firePower = 0f;
            } else {
                insertedBullet = "Jam";
            }

            if(firePower >= 0f){
                if(rrState == 2 || (rrState == 0 && Random.Range(rrPos[1] * 0.25f, rrPos[1]) < rrPos[0])) rrState = 4;

                GameObject Shoot = Instantiate(Bullet);
                Shoot.transform.position = orgPos[0];
                Shoot.transform.forward = orgPos[1];
                Shoot.GetComponent<GunfireScript>().Power = firePower;
            }
        }
    }

    public bool LoadBullet(string What, Vector3 Where, Vector3 Rot, float How = 0f){
        if(insertedBullet == "" && rrState == 0 && Vector3.Distance(Where, Slimend.position) < 0.3f){
            insertedBullet = What;
            insertBullet = 1f;
            Slimend.GetChild(0).localPosition = Vector3.zero;
            foreach(Transform setChild in Slimend.GetChild(0)) {
                if(setChild.name == What) {
                    setChild.localScale = Vector3.one;
                    if(What == "Cartridge") loadedPowder[1] = How;
                } else setChild.localScale = Vector3.zero;
            }
            return true;
        } else {
            return false;
        }
    }

}

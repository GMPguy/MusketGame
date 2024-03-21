using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CartridgeScript : ItemScript {

    public Transform Powder, Spent;

    public bool isBottle;
    public float Lifetime = 100f;
    public float[] PowderGrams;
    public float Ripped = 0f;
    public Transform Head;
    public ParticleSystem Sip;
    ParticleSystem.EmissionModule sipEmission;

    public override void ItemStart() {
        if(!isBottle) PowderGrams = new[]{Random.Range(7f, 7.9f), 8f};
        Head = GameObject.Find("Main Camera").transform;
        sipEmission = Sip.emission;

        if(isBottle){
            Ripped = 3f;
        }
    }

    public override void ItemUpdate() {
        if(simpleHandle.GrabStatus == 1) {
            simpleHandle.Master.handAnimate("Cartridge", simpleHandle.HandIndex, new[]{this.transform.position, this.transform.eulerAngles});
            if(Ripped <= 0f && Head && Vector3.Distance(this.transform.position, Head.transform.position) < 0.2f){
                Ripped = 1f;
                Spent.SetParent(Head);
                ItemSound.PlayAudio("CartridgeTear", 1f, 1, Spent.position);
            }
        }

        if(!simpleHandle){
            Lifetime -= Time.deltaTime;
            if(Lifetime <= 0f){
                if(Spent) Destroy(Spent.gameObject);
                Destroy(this.gameObject);
            }
        }

        if(Ripped > 0f && PowderGrams[0] > 0f){
            Ripped += Time.deltaTime;

            if (Ripped >= 2f && Ripped <= 2.5f){
                Spent.SetParent(null);
                Rigidbody bye = Spent.AddComponent<Rigidbody>();
                SphereCollider spere = Spent.AddComponent<SphereCollider>();
                bye.velocity = Head.transform.forward*Random.Range(0.5f, 5f);
                spere.radius = 0.1f;
                Ripped = 10f;
                ItemSound.PlayAudio("CartridgeSpit", 1f, 1, Spent.position);
            }

            Powder.localScale = new Vector3(1f,1f,PowderGrams[0]/PowderGrams[1]);
            float[] sipAngle = new float[]{Vector3.Angle(Vector3.down, this.transform.up), Mathf.Lerp(30f, 90f, PowderGrams[0]/PowderGrams[1])};
            if(sipAngle[0] < sipAngle[1]){
                Sip.Play();
                sipEmission.rateOverTime = Mathf.Lerp(200f, 100f, sipAngle[0] / sipAngle[1]);
                ItemSound.PlayAudio("_CartridgePour", 1f - (sipAngle[0] / sipAngle[1]), -1);
                float sipPower = Mathf.Lerp(Time.deltaTime * 7f, Time.deltaTime / 2f, sipAngle[0] / sipAngle[1]);
                PowderGrams[0] -= sipPower;
                Ray Checkpowder = new (Sip.transform.position, Sip.transform.forward);
                if(Physics.Raycast(Checkpowder, out RaycastHit Hitpowder, 0.3f) && Hitpowder.collider.name == "MusketBody"){
                    FlintLockScript fs = Hitpowder.collider.transform.parent.GetComponent<FlintLockScript>();
                    if (Vector3.Distance(Hitpowder.point, fs.powder.position) < 0.05f && fs.frizzenRot[0] > 0f) fs.loadedPowder[0] = Mathf.Clamp(fs.loadedPowder[0] + sipPower, 0f, 1f);
                    else if (Vector3.Distance(Hitpowder.point, fs.Slimend.position) < 0.1f && fs.insertedBullet == "") fs.loadedPowder[1] += sipPower;
                }
            } else {
                Sip.Stop();
                if(Ripped >= 11f) ItemSound.PlayAudio("");
            }

        } else if (PowderGrams[0] <= 0f && PowderGrams[1] > 0){
            PowderGrams[1] = 0;
            Sip.Stop();
            ItemSound.PlayAudio("");
        }
    }

    public override void ItemCollision(Collision collision){
        if(!isBottle && collision.collider.name == "MusketBody" && Ripped > 0f && collision.collider.transform.parent.GetComponent<FlintLockScript>().LoadBullet("Cartridge", this.transform.position, this.transform.eulerAngles, PowderGrams[0])){
            if(Spent.gameObject) Destroy(Spent.gameObject);
            Destroy(this.gameObject);
        }
    }

}

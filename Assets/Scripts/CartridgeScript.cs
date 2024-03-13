using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CartridgeScript : ItemScript {

    public Transform Powder, Spent;

    public float Lifetime = 100f;
    public float PowderGrams;
    public float Ripped = 0f;
    public Transform Head;
    public ParticleSystem Sip;
    ParticleSystem.EmissionModule sipEmission;

    public override void ItemStart() {
        PowderGrams = Random.Range(7f, 7.9f);
        Head = GameObject.Find("Main Camera").transform;
        sipEmission = Sip.emission;
    }

    public override void ItemUpdate() {
        if(simpleHandle.GrabStatus == 0) {
            Lifetime -= Time.deltaTime;
            if(Lifetime < 0) {
                Destroy(Spent.gameObject);
                Destroy(this.gameObject);
            }
        } else {
            simpleHandle.Master.handAnimate("Cartridge", simpleHandle.HandIndex, new[]{this.transform.position, this.transform.eulerAngles});
            Lifetime = 100f;
            if(Ripped <= 0f && Head && Vector3.Distance(this.transform.position, Head.transform.position) < 0.2f){
                Ripped = 1f;
                Spent.SetParent(Head);
                PlayAudio("CartridgeTear", 1f, 1, Spent.position);
            } else if (Ripped >= 2f && Ripped <= 2.5f){
                Spent.SetParent(null);
                Rigidbody bye = Spent.AddComponent<Rigidbody>();
                SphereCollider spere = Spent.AddComponent<SphereCollider>();
                bye.velocity = Head.transform.forward*Random.Range(0.5f, 5f);
                spere.radius = 0.1f;
                Ripped = 10f;
                PlayAudio("CartridgeSpit", 1f, 1, Spent.position);
            }
        }

        if(Ripped > 0f && PowderGrams > 0f){
            Ripped += Time.deltaTime;
            Powder.localScale = new Vector3(1f,1f,PowderGrams/8f);
            float[] sipAngle = new float[]{Vector3.Angle(Vector3.down, this.transform.up), Mathf.Lerp(30f, 90f, PowderGrams/8f)};
            if(sipAngle[0] < sipAngle[1]){
                Sip.Play();
                sipEmission.rateOverTime = Mathf.Lerp(200f, 100f, sipAngle[0] / sipAngle[1]);
                PlayAudio("_CartridgePour");
                ItemSound.volume = 1f - (sipAngle[0] / sipAngle[1]);
                float sipPower = Mathf.Lerp(Time.deltaTime * 7f, Time.deltaTime / 2f, sipAngle[0] / sipAngle[1]);
                PowderGrams -= sipPower;
                Ray Checkpowder = new (Sip.transform.position, Sip.transform.forward);
                if(Physics.Raycast(Checkpowder, out RaycastHit Hitpowder, 0.3f) && Hitpowder.collider.name == "MusketBody"){
                    FlintLockScript fs = Hitpowder.collider.transform.parent.GetComponent<FlintLockScript>();
                    if (Vector3.Distance(Hitpowder.point, fs.powder.position) < 0.1f) fs.loadedPowder = Mathf.Clamp(fs.loadedPowder + sipPower, 0f, 1f);
                }
            } else {
                Sip.Stop();
                if(Ripped >= 11f) PlayAudio("");
            }
        } else if (PowderGrams <= 0f && simpleHandle.isActive){
            simpleHandle.isActive = false;
            Lifetime = 10f;
            Sip.Stop();
            PlayAudio("");
        }
    }

    public override void ItemCollision(Collision collision){
        if(collision.collider.name == "MusketBody" && Ripped > 0f && Vector3.Distance(this.transform.position, collision.collider.transform.parent.GetComponent<FlintLockScript>().Slimend.position) < 0.5f){
            collision.collider.transform.parent.GetComponent<FlintLockScript>().LoadBullet("Cartridge", this.transform.position, this.transform.eulerAngles);
            Destroy(Spent.gameObject);
            Destroy(this.gameObject);
        }
    }

}

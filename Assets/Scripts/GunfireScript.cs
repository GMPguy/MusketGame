using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GunfireScript : MonoBehaviour {
    
    public int State = 0;
    public float Power, MaxAngle;
    public float[] Distances, Speeds;
    float Gravity, Lifetime;
    public AudioClip[] FireSounds;
    public GameObject WhoShot;

    public Transform Bullet;
    public ParticleSystem Smoke;
    public ParticleSystem Fire;
    Vector3 PrevPos;

    void Start(){
        ParticleSystem.EmissionModule mSmoke = Smoke.emission;
        mSmoke.rateOverTime = Mathf.Lerp(25, 500, Power);
        Smoke.Play();
        PrevPos = this.transform.position;
        if(Power > 0f){
            Bullet.parent = null;
            Fire.Play();
            this.GetComponent<AudioSource>().clip = FireSounds[0];
            this.GetComponent<AudioSource>().Play();
        } else {
            Destroy(Bullet.gameObject);
            State = 1;
            this.GetComponent<AudioSource>().clip = FireSounds[1];
            this.GetComponent<AudioSource>().Play();
        }
    }

    void Update(){
        switch(State){

            case 0:
                Lifetime += Time.deltaTime;
                Bullet.position += Bullet.forward * Mathf.Lerp(Speeds[0], Speeds[1], Power) + Vector3.down * (Gravity*Time.deltaTime*Lifetime*Lifetime);

                Ray Tracer = new Ray(PrevPos, Bullet.position - PrevPos);
                if(Physics.Raycast(Tracer, out RaycastHit TracerHIT, Vector3.Distance(Bullet.position, PrevPos))) Hit(TracerHIT.collider, new[]{TracerHIT.point, TracerHIT.normal});
                
                if(Distances[1] >= Mathf.Lerp( Distances[0], Distances[1], Distances[2] )) Hit(null);
                Distances[2] += Vector3.Distance(PrevPos, Bullet.position);
                PrevPos = Bullet.position;
                break;
            case 1:
                Lifetime -= Time.deltaTime;
                if(Lifetime <= 0f) {
                    Destroy(Bullet.gameObject);
                    Destroy(this.gameObject);
                }
                break;

        }
    }

    void Hit(Collider Victim = null, Vector3[] hitPoints = default){

        if(State == 0){
            bool hasHit = false;
            if(hitPoints == default) hitPoints = new[]{Bullet.position, Bullet.forward};

            if(!Victim){
                hasHit = true;
            } else if(Victim.gameObject != WhoShot){
                if(Victim.GetComponent<MaterialScript>()){
                    GameObject GroundHit = Instantiate(Victim.GetComponent<MaterialScript>().HitEffect);
                    GroundHit.transform.position = hitPoints[0];
                    GroundHit.transform.forward = -hitPoints[1];
                } else if (Victim.GetComponent<TargetScript>()){
                    Victim.GetComponent<TargetScript>().GotHit(hitPoints[0], hitPoints[1]);
                }
                hasHit = true;
            }

            if(hasHit){
                Bullet.GetChild(0).GetComponent<ParticleSystem>().Stop();
                Bullet.position = hitPoints[0];
                Lifetime = 10f;
                State = 1;
                SphereCollider bCol = Bullet.gameObject.AddComponent<SphereCollider>();
                bCol.radius = 0.019f;
                Rigidbody bRig = Bullet.gameObject.AddComponent<Rigidbody>();
                bRig.collisionDetectionMode = CollisionDetectionMode.Continuous;
                bRig.velocity = Bullet.transform.forward;
            }
        }

    }

}

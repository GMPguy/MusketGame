using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GunfireScript : MonoBehaviour {
    
    public int State = 0;
    public float Power, MaxAngle;
    public float[] Distances, Speeds;
    const float Gravity = 9.8f; 
    float Lifetime;
    public AudioClip[] FireSounds;
    public GameObject WhoShot;
    public PhysicMaterial BulletMaterial;

    public Transform Bullet;
    public ParticleSystem Smoke;
    public ParticleSystem Fire;
    Vector3 Origin, PrevPos;
    List<Vector3> Laser;

    void Start(){
        Speeds[0] = Mathf.Lerp(Speeds[0], Speeds[1], Power);
        Distances[0] = Mathf.Lerp(Distances[0], Distances[1], Power);
        Laser = new List<Vector3>();
        ParticleSystem.EmissionModule mSmoke = Smoke.emission;
        mSmoke.rateOverTime = Mathf.Lerp(25, 500, Power);
        Smoke.Play();
        PrevPos = Origin = this.transform.position;
        if(Power > 0f){
            Bullet.parent = null;
            Fire.Play();
            this.GetComponent<AudioSource>().clip = FireSounds[0];
            this.GetComponent<AudioSource>().Play();
        } else {
            Bullet.localScale = Vector3.zero;
            Bullet.transform.Rotate(Random.Range(.5f, .5f), Random.Range(-.5f, .5f), Random.Range(-.5f, .5f) * MaxAngle);
            Destroy(Bullet.GetChild(0).gameObject);
            Lifetime = 10f;
            State = 1;
            this.GetComponent<AudioSource>().clip = FireSounds[1];
            this.GetComponent<AudioSource>().Play();
        }
    }

    void Update(){
        switch(State){

            case 0:
                Lifetime += Time.deltaTime;
                float mainSpeed = Mathf.Lerp(Speeds[0], 0f, (Vector3.Distance(Bullet.position, Origin) - Distances[0]) / (Distances[0]*3f));
                Bullet.position += (Bullet.forward*mainSpeed*Time.deltaTime) + (Vector3.down * (Gravity*Lifetime*Time.deltaTime));
                Laser.Add(PrevPos);
                Laser.Add(Bullet.position);

                Ray Tracer = new Ray(PrevPos, Bullet.position - PrevPos);
                if(Physics.Raycast(Tracer, out RaycastHit TracerHIT, Vector3.Distance(Bullet.position, PrevPos))) Hit(TracerHIT.collider, new[]{TracerHIT.point, TracerHIT.normal});
                else if (Bullet.position.y < 0f) Hit(null);

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

        float Lethality = Mathf.Lerp(1f, 0f, (Vector3.Distance(Origin, Bullet.position)-Distances[0]) / Distances[0]);

        if(State == 0){
            bool hasHit = false;
            if(hitPoints == default) hitPoints = new[]{Bullet.position, Bullet.forward};

            if(!Victim){
                hasHit = true;
            } else if(Victim.gameObject != WhoShot){
                if(Victim.GetComponent<MaterialScript>()){
                    GameObject GroundHit = Instantiate(Victim.GetComponent<MaterialScript>().HitEffect);
                    GroundHit.transform.position = hitPoints[0];
                    GroundHit.transform.forward = hitPoints[1];
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
                bCol.material = BulletMaterial;
            }

            for(int dl = 0; dl < Laser.ToArray().Length-1; dl+=2){
                Debug.DrawLine(Laser[dl], Laser[dl+1], Color.red, 100f);
            }
        }

    }

}

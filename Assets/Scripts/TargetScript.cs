using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour {
    public GameObject DestroyNow;
    public GameObject HitEffect;
    public void GotHit(Vector3 HitPoint, Vector3 hitDir){
        if(HitEffect){
            GameObject Hit = Instantiate(HitEffect);
            Hit.transform.position = HitPoint;
            Hit.transform.forward = hitDir;
            Hit.transform.SetParent(this.transform);
        }
        if(DestroyNow != null){
            GameObject bye = Instantiate(DestroyNow);
            bye.transform.position = this.transform.position;
            Destroy(this.gameObject);
            foreach(Transform flu in bye.transform) if (flu.GetComponent<Rigidbody>()){
                flu.GetComponent<Rigidbody>().velocity = new(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(0f, 4f));
                GameObject Shard = Instantiate(flu.gameObject);
                Shard.transform.localScale = Vector3.one/2f;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelInsertScript : ItemScript {
    float Lifetime = 30f;
    public string InsertType;
    protected override void ItemUpdate() {
        if(simpleHandle.GrabStatus == 1) Lifetime = 15f;
        else {
            if(Lifetime > 0f) Lifetime -= Time.deltaTime;
            else Destroy(this.gameObject);
        }
    }

    protected override void ItemCollision(Collision collision) {
        if(simpleHandle.GrabStatus == 1 && collision.collider.name == "MusketBody" && collision.collider.transform.parent.GetComponent<FlintLockScript>().LoadBullet(InsertType, this.transform.position, this.transform.eulerAngles)){
            simpleHandle.Drop();
            Destroy(this.gameObject);
        }
    }
}

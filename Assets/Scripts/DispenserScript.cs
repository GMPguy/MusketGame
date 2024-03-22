using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispenserScript : GrabPoint {

    public GameObject Item;
    public int HandleID;
    void Update(){
        if(Changed && GrabStatus == 1){
            Changed = false;
            GameObject newItem = Instantiate(Item);
            if(HandleID == -1) newItem.GetComponent<GrabPoint>().Grab(Hand, HandIndex+GP);
            else newItem.transform.GetChild(HandleID).GetComponent<GrabPoint>().Grab(Hand, HandIndex+GP);
            newItem.transform.position = this.transform.GetChild(0).position;
            newItem.transform.rotation = this.transform.GetChild(0).rotation;
        }
    }

}

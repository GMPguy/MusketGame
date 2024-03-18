using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderScript : GrabPoint {
    
    PlayerScript prevOwner;
    void Update(){
        if(Changed){
            Changed = false;
            if(GrabStatus == 1){
                prevOwner = Master;
                prevOwner.isClimbing = this;
            } else {
                if(prevOwner != null && prevOwner.isClimbing == this) prevOwner.isClimbing = null;
                prevOwner = null;
            }
        }
    }

}

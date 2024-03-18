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
            } else {
                prevOwner = null;
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour {
    public Terrain terrain;

    void Start(){
        terrain.detailObjectDistance = 2000;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour {
    public Terrain terrain;
    void Start(){
        terrain.detailObjectDistance = 2000;
        Physics.IgnoreLayerCollision(3, 6, true);

        foreach (GameObject tree in GameObject.FindGameObjectsWithTag("Tree")) {
            tree.transform.Rotate(Vector3.forward * Random.Range(0f, 360f));
            tree.transform.localScale = Vector3.one * Random.Range(0.75f, 2f);
            tree.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        }
    }
}

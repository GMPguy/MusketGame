using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour {
    public Terrain terrain;
    public Light MainLight;
    void Start(){
        MainLight = GameObject.Find("Sun").GetComponent<Light>();

        terrain.detailObjectDistance = 2000;
        Physics.IgnoreLayerCollision(3, 6, true);
        Physics.IgnoreLayerCollision(8, 8, true);

        foreach (GameObject tree in GameObject.FindGameObjectsWithTag("Tree")) {
            tree.transform.Rotate(Vector3.forward * Random.Range(0f, 360f));
            tree.transform.localScale = Vector3.one * Random.Range(0.75f, 2f);
            tree.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        }
    }

    void Update(){
        MainLight.intensity = Mathf.Clamp(Mathf.Lerp(0f, 2f, Mathf.PerlinNoise(Time.timeSinceLevelLoad/10f, Time.timeSinceLevelLoad/10f)), 0f, 1f);
    }
}

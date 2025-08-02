using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadScript : MonoBehaviour {

    public List<GameObject> LowEndTermination;

    void Start() {
        GameScript GS = FindObjectOfType<GameScript>();

        if (GS.LowEnd)
            for (int i = LowEndTermination.Count - 1; i >= 0; i--)
                Destroy(LowEndTermination[i]);

        if (GameObject.Find("Terrain"))
            GS.terrain = GameObject.Find("Terrain").GetComponent<Terrain>();

        if (GameObject.Find("MainSun"))
            GS.MainLight = GameObject.Find("MainSun").GetComponent<Light>();

        if (GS.terrain)
            GS.terrain.detailObjectDistance = GS.LowEnd ? 0 : 2000;

        foreach (GameObject tree in GameObject.FindGameObjectsWithTag("Tree")) {
            tree.transform.Rotate(Vector3.forward * Random.Range(0f, 360f));
            tree.transform.localScale = Vector3.one * Random.Range(0.75f, 2f);
        }

        Destroy(gameObject);
    }

}

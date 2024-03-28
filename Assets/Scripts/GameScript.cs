using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour {

    // Options
    public bool BulletLaser = false;
    public bool GunSmoke = true;
    public bool AutomaticReload = false;
    // Options

    // References
    public Terrain terrain;
    public Light MainLight;
    // References

    // Bullet trace
    List<GameObject> traces;
    // Bullet trace

    void Start(){
        traces = new();
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

    public void DrawTraces(List<Vector3> Lasers){
        foreach(GameObject cleanup in traces) Destroy(cleanup);
        traces = new();
        GameObject refCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        refCube.GetComponent<MeshRenderer>().materials[0].shader = Shader.Find("Unlit/Color");
        refCube.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        refCube.GetComponent<BoxCollider>().isTrigger = true;
        for(int dl = 0; dl < Lasers.ToArray().Length-1; dl+=2){
            GameObject lp = Instantiate(refCube);
            lp.transform.position = Vector3.Lerp(Lasers[dl], Lasers[dl+1], 0.5f);
            lp.transform.LookAt(Lasers[dl+1]);
            lp.transform.localScale = new (0.01f, 0.01f, Vector3.Distance(Lasers[dl], Lasers[dl+1]));
            traces.Add(lp);
        }
        Destroy(refCube);
    }
}

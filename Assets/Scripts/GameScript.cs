using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameScript : MonoBehaviour {

    // Options
    // Sound
    float[] Volume = {1f, 1f, 1f, 1f}; // Master, SFX, EnvSFX, Music
    float[] prevVolume = {-1f, -1f, -1f, -1f};
    float[] deafenFactor = {1f, 1f};
    // Sound
    public AudioMixer VolumeBank;
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

        string[] vNames = new[]{"Master", "SFX", "EnvSFX", "Music"};
        Volume[2] = Volume[1] * deafenFactor[0];
        deafenFactor[0] = Mathf.MoveTowards(deafenFactor[0], 1f, Time.deltaTime / deafenFactor[1]);
        for (int sv = 0; sv < 3; sv++) if (prevVolume[sv] != Volume[sv]) {
            prevVolume[sv] = Volume[sv];
            VolumeBank.SetFloat(vNames[sv], Mathf.Log10(Volume[sv])*20);
        }

    }

    public void Deafen(float Amount, float recTime, int method = 0){
        if(method == 0) deafenFactor[0] = Amount;
        else deafenFactor[0] += Amount;
        deafenFactor[1] = recTime;
    }

    public void DrawTraces(List<Vector3> Lasers){
        foreach(GameObject cleanup in traces) Destroy(cleanup);
        traces = new();
        GameObject refCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        refCube.GetComponent<MeshRenderer>().materials[0].shader = Shader.Find("Unlit/Color");
        refCube.GetComponent<MeshRenderer>().materials[0].color = Color.red;
        for(int dl = 0; dl < Lasers.ToArray().Length-1; dl+=2){
            GameObject lp = Instantiate(refCube);
            Destroy(lp.GetComponent<BoxCollider>());
            lp.transform.position = Vector3.Lerp(Lasers[dl], Lasers[dl+1], 0.5f);
            lp.transform.LookAt(Lasers[dl+1]);
            float dist = Vector3.Distance(GameObject.Find("Main Camera").transform.position, Lasers[dl+1]);
            lp.transform.localScale = new (0.001f * dist, 0.001f * dist, Vector3.Distance(Lasers[dl], Lasers[dl+1]));
            traces.Add(lp);
        }
        Destroy(refCube);
    }
}

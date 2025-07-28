using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Random=UnityEngine.Random;

public class GameScript : MonoBehaviour {

    // Options
    // Sound
    public float[] Volume = {1f, 1f, 1f, 1f}; // Master, SFX, EnvSFX, Music
    float[] prevVolume = {-1f, -1f, -1f, -1f};
    float[] deafenFactor = {1f, 1f};
    // Sound
    public AudioMixer VolumeBank;
    public bool BulletLaser = false;
    public bool GunSmoke = true;
    public bool AutomaticReload = false;
    public int InvertedThumbsticks = 0;
    // Options

    // References
    public Terrain terrain;
    public Light MainLight;
    public Material TracerMaterial;
    // References

    // Bullet trace
    List<GameObject> traces;
    // Bullet trace

    void Start(){

        if (GameObject.Find("GameScript")) {
            Destroy(gameObject);
            return;
        } else {
            this.name = "GameScript";
            DontDestroyOnLoad(this.gameObject);
        }

        traces = new();
    }

    void Update(){

        if (MainLight)
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

    public void DrawTraces(List<Vector3> Lasers = default){
        foreach(GameObject cleanup in traces) Destroy(cleanup);
        traces = new();
        if(Lasers != default){
            GameObject refCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            refCube.GetComponent<MeshRenderer>().sharedMaterial = TracerMaterial;
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
}

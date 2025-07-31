using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingScript : TargetScript {
    public Texture[] textures;
    void Start(){
        Texture choosen = textures[(int)Random.Range(0f, textures.Length-0.1f)];
        this.GetComponent<MeshRenderer>().materials[1].mainTexture = choosen;
        this.GetComponent<MeshRenderer>().materials[1].color = Color.Lerp(new Color(0.78f, 0.78f, 0.78f, 1f), new Color(0.26f, 0.13f, 0f, 1f), Random.Range(0f, 1f));
        if(choosen.width > choosen.height) this.transform.localScale = new Vector3(1f, choosen.width/choosen.height, 1f);
        else this.transform.localScale = new Vector3(1f, 1f, (float)choosen.height / (float)choosen.width);
    }
}

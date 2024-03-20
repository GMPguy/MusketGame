using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundScript : MonoBehaviour {

    public AudioClip clip;
    int SoundID = -9999;
    public AudioSource Sound;
    public AudioClip[] Sounds;

    void Start () {
        Sound = this.GetComponent<AudioSource>();
    }
    
    public void PlayAudio(string AudioName, float Volume = 1f, int Importance = 0, Vector3 AudioPos = default){
        if(!Sound.isPlaying) SoundID = -9999;
        if(Importance >= SoundID && AudioName != ""){
            SoundID = Importance;
            string theAudio = AudioName;
            Sound.volume = Volume;
            if(theAudio[0] == '_') theAudio = AudioName[1..];
            if (AudioPos != default) Sound.transform.position = AudioPos;
            else Sound.transform.position = this.transform.position;
            if (!(AudioName[0] == '_' && Sound.isPlaying && Sound.clip.name == AudioName[1..])) for(int isc = 0; isc <= Sounds.Length; isc++){
                if(isc == Sounds.Length){
                    Debug.LogError("No item sound clip of name " + AudioName + " found!");
                } else if (Sounds[isc].name == theAudio){
                    Sound.clip = Sounds[isc];
                    clip = Sounds[isc];
                    Sound.Play();
                    break;
                }
            }
        } else if (AudioName == ""){
            Sound.Stop();
            SoundID = -9999;
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {

    GameScript GS;
    public Text[] ButtonTextes;
    public string[] Buttons;

    void Start(){
        GS = GameObject.FindObjectOfType<GameScript>();
        Buttons = new[]{"","laser","smoke","graphics","reload","", "", "sounds",""};
        setupButtons();
    }

    void setupButtons(int spec = -1){
        int[] lv = {0, 8}; if (spec >= 0) lv = new[]{spec, spec};
        for(int setB = lv[0]; setB <= lv[1]; setB++){
            switch(Buttons[setB]){
                case "laser": ButtonTextes[setB].text = "Bullet tracking - " + GS.BulletLaser; break;
                case "smoke": ButtonTextes[setB].text = "Gun smoke - " + GS.GunSmoke; break;
                case "reload": ButtonTextes[setB].text = "Automatic reload - " + GS.AutomaticReload; break;
                case "sounds": ButtonTextes[setB].text = "Sound volumes"; break;
                case "graphics": 
                    string[] levels = {"Low", "Medium", "High"};
                    ButtonTextes[setB].text = "Graphics - " + levels[QualitySettings.GetQualityLevel()]; 
                    break;
                case "master_volume": ButtonTextes[setB].text = "Master volume - " + (int)(GS.Volume[0]*100f); break;
                case "sfx_volume": ButtonTextes[setB].text = "SFX volume - " + (int)(GS.Volume[1]*100f); break;
                case "music_volume": ButtonTextes[setB].text = "Music volume - " + (int)(GS.Volume[3]*100f); break;
                case "back": ButtonTextes[setB].text = "Back"; break;
                case "": ButtonTextes[setB].text = ""; break;
                default: Debug.LogError("No button name " + Buttons[setB] + " found!"); break;
            }
        }
    }

    public void Button(int WhichOne){
        bool updateAll = false;
        switch(Buttons[WhichOne]){
            case "laser": 
                GS.BulletLaser = !GS.BulletLaser;
                if(!GS.BulletLaser) GS.DrawTraces();
                break;
            case "smoke": GS.GunSmoke = !GS.GunSmoke; break;
            case "reload": GS.AutomaticReload = !GS.AutomaticReload; break;
            case "graphics": QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel()+1)%3); break;
            case "sounds": 
                Buttons = new[] {"", "master_volume", "music_volume", "sfx_volume", "", "", "", "back", ""};
                updateAll = true;
                break;
            case "master_volume": GS.Volume[0] = (GS.Volume[0] + 0.1f)%1.1f; break;
            case "music_volume": GS.Volume[3] = (GS.Volume[3] + 0.1f)%1.1f; break;
            case "sfx_volume": GS.Volume[1] = (GS.Volume[1] + 0.1f)%1.1f; break;
            case "back": 
                Buttons = new[]{"", "laser", "smoke", "graphics", "reload", "", "", "sounds", ""};
                updateAll = true;
                break;
            case "": break;
            default: Debug.LogError("No button functionality of name " + Buttons[WhichOne] + " found!"); break;
        }
        if(updateAll) setupButtons();
        else setupButtons(WhichOne);
    }

}

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
        Buttons = new[]{"","","","laser","smoke","graphics","reload","",""};
        setupButtons();
    }

    void setupButtons(int spec = -1){
        int[] lv = {0, 8}; if (spec >= 0) lv = new[]{spec, spec};
        for(int setB = lv[0]; setB <= lv[1]; setB++){
            switch(Buttons[setB]){
                case "laser": ButtonTextes[setB].text = "Bullet tracking - " + GS.BulletLaser; break;
                case "smoke": ButtonTextes[setB].text = "Gun smoke - " + GS.GunSmoke; break;
                case "reload": ButtonTextes[setB].text = "Automatic reload - " + GS.AutomaticReload; break;
                case "graphics": 
                    string[] levels = {"Low", "Medium", "High"};
                    ButtonTextes[setB].text = "Graphics - " + levels[QualitySettings.GetQualityLevel()]; 
                    break;
                default: ButtonTextes[setB].text = ""; break;
            }
        }
    }

    public void Button(int WhichOne){
        switch(Buttons[WhichOne]){
            case "laser": 
                GS.BulletLaser = !GS.BulletLaser; 
                setupButtons(WhichOne);
                break;
            case "smoke": 
                GS.GunSmoke = !GS.GunSmoke; 
                setupButtons(WhichOne);
                break;
            case "reload": 
                GS.AutomaticReload = !GS.AutomaticReload; 
                setupButtons(WhichOne);
                break;
            case "graphics": 
                QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel()+1)%3); 
                setupButtons(WhichOne);
                break;
            case "": break;
            default: Debug.LogError("No button functionality of name " + Buttons[WhichOne] + " found!"); break;
        }
    }

}

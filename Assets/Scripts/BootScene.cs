using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootScene : MonoBehaviour {

    // Scene loading
    public BoxCollider SceneLoader;
    public Transform PlayerHead;
    bool initiateLoad;

    // Ink
    public Transform Pen;
    public BoxCollider Agreement;
    public Transform AgreementMark;
    public AudioSource PenSound;
    bool agreed;

    // Door
    public AudioSource DoorSound;
    public Transform LeftDoor, RightDoor;
    public float DoorOpen = 0f; // -1z for left, +1z for right

    void Update() {
        
        // Load scene after entering to light
        if (SceneLoader.bounds.Contains(PlayerHead.position) && !initiateLoad) {
            initiateLoad = true;
            SceneManager.LoadScene("MainScene");
        }

        // Signing the agreement
        if (!agreed) {
            if (Agreement.bounds.Contains(Pen.position)) {
                agreed = true;
                DoorOpen = 0f;
                AgreementMark.localScale = Vector3.one;

                PenSound.Play();
                DoorSound.Play();
            }
        } else if (DoorOpen < 2f) {
            DoorOpen += Time.deltaTime;

            LeftDoor.eulerAngles = Vector3.Lerp(
                new (-90f, 0f, -90f),
                new (-90f, 0f, -180f),
                DoorOpen / 2f
            );

            RightDoor.eulerAngles = Vector3.Lerp(
                new (-90f, 0f, -90f),
                new (-90f, 0f, 0f),
                DoorOpen / 2f
            );
        }

    } 

}

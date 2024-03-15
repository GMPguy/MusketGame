using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour {

    // References
    public Transform Head;
    // References

    // Hand visuals
    public Transform[] ActualHands;
    public Transform[] HandsVisible;
    public Animator[] HandsAnims;
    float[] quitAnim = {0f, 0f};
    // Hand visuals

    // Hand actions
    public float[] HandPinch = {0f, 0f};
    public bool[] hasPinched = {false, false};
    public float[] HandGrab = {0f, 0f};
    public bool[] hasGrabbed = {false, false};
    public bool[] Multitask = {true, true};
    public Vector3[] HandVector = {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero}; // rPos, rRot, lPos, lRot
    Vector3[] prevHandVectors = {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};

    public InputActionProperty[] TriggerDet;
    public InputActionProperty[] GrabDet;
    public InputActionProperty[] ThumbstickDet;

    public GameObject[] CaughtObjects = {null, null, null, null}; // rGrab, rPinch, lGrab, lPinch
    // Hand actions

    string[] gpTag = {"Object_Grab", "Object_Pinch"};

    void Start(){
        HandsAnims[0] = HandsVisible[0].GetComponent<Animator>();
        HandsAnims[1] = HandsVisible[1].GetComponent<Animator>();
        for(int sh = 0; sh <= 1; sh++){
            TriggerDet[sh].action.Enable();
            GrabDet[sh].action.Enable();
        }
    }

    void Update(){
        Movement();
        HandBehaviour();
    }

    void Movement(){
        this.transform.position += Head.transform.forward * ThumbstickDet[0].action.ReadValue<Vector3>().y + Head.transform.right * ThumbstickDet[0].action.ReadValue<Vector3>().x;
    }

    void HandBehaviour() {
        for(int sh = 0; sh <= 1; sh++){
            HandPinch[sh] = TriggerDet[sh].action.ReadValue<float>();
            HandsAnims[sh].SetFloat("Pinch", HandPinch[sh]);
            HandGrab[sh] = GrabDet[sh].action.ReadValue<float>();
            HandsAnims[sh].SetFloat("Grab", HandGrab[sh]);

            if(quitAnim[sh] > 0f){
                quitAnim[sh] -= Time.deltaTime;
            } else {
                HandsAnims[sh].Play("Idle", 0);
                HandsVisible[sh].position = ActualHands[sh].position;
                HandsVisible[sh].rotation = ActualHands[sh].rotation;
            }

            if(ActualHands[sh].position != prevHandVectors[sh*2]){
                HandVector[sh*2] = ActualHands[sh].position - prevHandVectors[sh*2];
                prevHandVectors[sh*2] = ActualHands[sh].position;
            }
            if(ActualHands[sh].eulerAngles != prevHandVectors[sh*2+1]){
                HandVector[sh*2+1] = ActualHands[sh].eulerAngles - prevHandVectors[sh*2+1];
                prevHandVectors[sh*2+1] = ActualHands[sh].eulerAngles;
            }
            
            if(HandPinch[sh] > 0.8f && !hasPinched[sh]) { Catch(sh, 1, false); hasPinched[sh] = true; }
            else if(HandPinch[sh] < 0.1f && hasPinched[sh]) { Catch(sh, 1, true); hasPinched[sh] = false; }

            if(HandGrab[sh] > 0.8f && !hasGrabbed[sh]) { Catch(sh, 0, false); hasGrabbed[sh] = true; }
            else if(HandGrab[sh] < 0.1f && hasGrabbed[sh]) { Catch(sh, 0, true); hasGrabbed[sh] = false; }

            for(int iv = 0 + (sh*2); iv <= 1 + (sh*2); iv++) if (CaughtObjects[iv]) {
                GrabPoint gp = CaughtObjects[iv].GetComponent<GrabPoint>();
                gp.inhPinch = HandPinch[sh];
                gp.inchGrab = HandGrab[sh];
                gp.HandVector = new[]{ HandVector[sh*2], HandVector[sh*2 + 1] };
                if(!gp.isActive || (gp.tag == "Object_Grab" && gp.inchGrab < 0.1f) || (gp.tag == "Object_Pinch" && gp.inhPinch < 0.1f) ) Catch(sh, iv%2, true);
            }
        }
    }

    public void Catch(int Hand, int GP, bool LetGo){
        if(!LetGo && Multitask[Hand] && !CaughtObjects[Hand*2 + GP]){
            GameObject[] getObjects = GameObject.FindGameObjectsWithTag(gpTag[GP]);
            float nearest = Mathf.Infinity;
            GameObject potential = null;
            for(int po = 0; po < getObjects.Length; po++){
                GrabPoint tGP = getObjects[po].GetComponent<GrabPoint>();
                float dist = tGP.checkForDist(ActualHands[Hand].position);
                if(dist < tGP.GrabDistance && dist <= nearest && tGP.checkForGrab(ActualHands[Hand].position)) {
                    potential = getObjects[po];
                    nearest = dist;
                }
            }
            if(potential){
                CaughtObjects[Hand*2 + GP] = potential;
                Multitask[Hand] = potential.GetComponent<GrabPoint>().MultiTask;
                potential.GetComponent<GrabPoint>().Grab(ActualHands[Hand].transform, Hand);
            }
        } else if (LetGo && CaughtObjects[Hand*2 + GP]) {
            CaughtObjects[Hand*2 + GP].GetComponent<GrabPoint>().Drop();
        }
    }

    public void handAnimate(string newState, int theHand, Vector3[] pos){
        quitAnim[theHand] = Time.deltaTime*3f;
        HandsAnims[theHand].Play(newState);
        HandsVisible[theHand].position = pos[0];
        HandsVisible[theHand].eulerAngles = pos[1];
    }

}

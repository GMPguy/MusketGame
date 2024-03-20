using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour {

    // Movement
    public Transform Head;
    public Vector3 MovementVector; 
    Vector3 prevPos, prevHead;
    readonly float Speed = 2f;
    readonly float RotationSpeed = 90f;
    // Movement

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
    public Vector3[] HandVector = {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero}; // rPos, rLocPos, rRot, lPos, lLocPos, lRot
    Vector3[] prevHandVectors = {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};

    public InputActionProperty[] TriggerDet;
    public InputActionProperty[] GrabDet;
    public InputActionProperty[] ThumbstickDet;

    public GameObject[] CaughtObjects = {null, null, null, null}; // rGrab, rPinch, lGrab, lPinch
    // Hand actions

    // Misc
    Vector3 Footstep;
    public LayerMask MovementLayerMask;
    public LadderScript isClimbing;
    public Rigidbody Rig;
    // Misc

    // Audio
    public SoundScript PlayerAudio;
    // Audio

    readonly string[] gpTag = {"Object_Grab", "Object_Pinch"};

    void Start(){
        Rig = this.GetComponent<Rigidbody>();
        HandsAnims[0] = HandsVisible[0].GetComponent<Animator>();
        HandsAnims[1] = HandsVisible[1].GetComponent<Animator>();
        for(int sh = 0; sh <= 1; sh++){
            ThumbstickDet[sh].action.Enable();
            TriggerDet[sh].action.Enable();
            GrabDet[sh].action.Enable();
        }
    }

    void Update(){
        if(isClimbing != null) Movement("Ladder");
        else Movement();
        HandBehaviour();
    }

    void Movement(string way = "", bool isMovement = true){

        if(isMovement){
            if(prevHead != Head.position){
                Vector3 yHead = new (Head.localPosition.x, 0f, Head.localPosition.z);
                this.transform.position += this.transform.TransformDirection( yHead - prevHead );
                prevHead = yHead;
                Head.parent.localPosition = new Vector3(-Head.localPosition.x, 0f, -Head.localPosition.z);
            }
            MovementVector = this.transform.position - prevPos;
            prevPos = this.transform.position;
        }
        
        switch(way){
            case "RigEnable":
                Rig.useGravity = this.GetComponent<CapsuleCollider>().enabled = true;
                break;
            case "RigDisable":
                Rig.velocity = Vector3.zero;
                Rig.useGravity = this.GetComponent<CapsuleCollider>().enabled = false;
                break;
            case "Ladder":
                Movement("RigDisable", false);
                this.transform.position -= this.transform.TransformDirection(isClimbing.HandVector[1]);
                break;
            default:
                Movement("RigEnable", false);
                Vector2[] thumbs = new []{ThumbstickDet[0].action.ReadValue<Vector2>(), ThumbstickDet[1].action.ReadValue<Vector2>()};
                Vector3 normalFoward = new Vector3(Head.forward.x, 0f, Head.forward.z);
                Vector3 normalRight = new Vector3(Head.right.x, 0f, Head.right.z);
                if(thumbs[0].magnitude > 0.5f) {
                    Vector3 dmv = (normalFoward * thumbs[0].y + normalRight * thumbs[0].x) * (Time.deltaTime * Speed);
                    Collider[] pc =Physics.OverlapCapsule(this.transform.position+(Vector3.up*0.25f)+dmv, Head.transform.position-(Vector3.up*0.25f)+dmv, 0.1f, MovementLayerMask);
                    if(pc.Length <= 0) this.transform.position += dmv;
                }
                if(thumbs[1].magnitude > 0.5f) this.transform.Rotate(Vector3.up * thumbs[1].x *  (Time.deltaTime * RotationSpeed));
                
                if(Vector3.Distance(this.transform.position, Footstep) > 1f){
                    Footstep = this.transform.position;
                    Ray checkFS = new (this.transform.position + Vector3.up/2f, Vector3.down);
                    if(Physics.Raycast(checkFS, out RaycastHit FShit, 0.6f, MovementLayerMask) && FShit.collider.GetComponent<MaterialScript>()){
                        MaterialScript ms = FShit.collider.GetComponent<MaterialScript>();
                        PlayerAudio.PlayAudio(ms.Footsteps[(int)Random.Range(0f, ms.Footsteps.Length-0.1f)].name, 1f, 0, this.transform.position);
                        Footstep = this.transform.position;
                    }
                }
                break;
        }
        
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

            Vector3[] pHVref = new[] {ActualHands[sh].position, ActualHands[sh].localPosition, ActualHands[sh].eulerAngles};
            for(int pHV = 0; pHV < 3; pHV++) if (pHVref[pHV] != prevHandVectors[sh*3+pHV]){
                HandVector[sh*3+pHV] = pHVref[pHV] - prevHandVectors[sh*3+pHV];
                prevHandVectors[sh*3+pHV] = pHVref[pHV];
            }
            
            if(HandPinch[sh] > 0.8f && !hasPinched[sh]) { Catch(sh, 1, false); hasPinched[sh] = true; }
            else if(HandPinch[sh] < 0.1f && hasPinched[sh]) { Catch(sh, 1, true); hasPinched[sh] = false; }

            if(HandGrab[sh] > 0.8f && !hasGrabbed[sh]) { Catch(sh, 0, false); hasGrabbed[sh] = true; }
            else if(HandGrab[sh] < 0.1f && hasGrabbed[sh]) { Catch(sh, 0, true); hasGrabbed[sh] = false; }

            for(int iv = 0 + (sh*2); iv <= 1 + (sh*2); iv++) if (CaughtObjects[iv]) {
                GrabPoint gp = CaughtObjects[iv].GetComponent<GrabPoint>();
                gp.inhPinch = HandPinch[sh];
                gp.inchGrab = HandGrab[sh];
                gp.HandVector = new[]{ HandVector[sh*3], HandVector[sh*3 + 1], HandVector[sh*3+2] };
                if(!gp.isActive || (gp.tag == "Object_Grab" && gp.inchGrab < 0.1f) || (gp.tag == "Object_Pinch" && gp.inhPinch < 0.1f) ) Catch(sh, iv%2, true);
            }
        }
    }

    public void Catch(int Hand, int GP, bool LetGo){

        if(!LetGo && Multitask[Hand] && !CaughtObjects[Hand*2 + GP]){

            if(CaughtObjects[(Hand+1)%2*2 + GP] && CaughtObjects[(Hand+1)%2*2 + GP].GetComponent<GrabPoint>().checkForDist(ActualHands[Hand].position) < 0.1f){

                CaughtObjects[(Hand+1)%2*2 + GP].GetComponent<GrabPoint>().Switch(ActualHands[Hand].transform, Hand*2+GP);

            } else {

                GameObject[] getObjects = GameObject.FindGameObjectsWithTag(gpTag[GP]);
                float nearest = Mathf.Infinity;
                GameObject potential = null;
                for(int po = 0; po < getObjects.Length; po++){
                    GrabPoint tGP = getObjects[po].GetComponent<GrabPoint>();
                    float dist = tGP.checkForDist(ActualHands[Hand].position);
                    if(dist <= tGP.GrabDistance && dist <= nearest && tGP.checkForGrab(ActualHands[Hand].position)) {
                        potential = getObjects[po];
                        nearest = dist;
                    }
                }
                if(potential) potential.GetComponent<GrabPoint>().Grab(ActualHands[Hand].transform, Hand*2 + GP);

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

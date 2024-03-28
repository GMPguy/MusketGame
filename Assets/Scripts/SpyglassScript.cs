using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpyglassScript : ItemScript {
 
    public GrabPoint Handle, Holder;
    public Transform View;
    public Transform[] Segments;
    public float[] SegmentOffsets;
    public float Zoom = 0f;
    float ZoomDist = 0f;
    LunetaScript Glass;
    bool enableGlass = false;

    protected override void ItemStart() {
        Glass = GameObject.FindObjectOfType<LunetaScript>();
    }

    protected override void ItemUpdate() {

        if(Holder.GrabStatus == 1){
            Handle.isActive = true;
            if(Handle.GrabStatus == 1) setPos(true, new[]{Handle.Hand.position + (Vector3.up/20f), Holder.Hand.position, Holder.Hand.forward});
            else setPos(true, new[]{movePivoted(this.transform, Holder.transform.position, Holder.Hand.position), Holder.transform.position - Holder.Hand.up, Holder.Hand.forward});
        
            if(Handle.inhPinch > 0.5f) ZoomDist += Vector3.Dot(Handle.HandVector[1], this.transform.forward) * -4f;
            else if(Holder.inhPinch > 0.5f) ZoomDist += Vector3.Dot(Holder.HandVector[1], this.transform.forward) * 4f;

            if(ZoomDist > 0.025f || ZoomDist < -0.025f){
                Zoom = Mathf.Clamp(Zoom + ZoomDist, 0f, 1f);
                Glass.FOV = Mathf.Lerp(11f, 1f, Zoom);
                Glass.BarrelLenght = Mathf.Lerp(0.06f, 0.006f, Zoom);
                ItemSound.PlayAudio("SpyglassCrank", 1f, 0, this.transform.position);
                Segments[0].localPosition = Vector3.forward * Mathf.Lerp(0.235f, 0.4847f, Zoom);
                for(int ss = 1; ss < Segments.Length; ss++) Segments[ss].localPosition = Vector3.forward * SegmentOffsets[ss-1] * Zoom;
                ZoomDist = 0f;
            }

            if(!enableGlass){
                enableGlass = true;
                Glass.Enable(View);
            }

        } else {
            Handle.isActive = false;
            setPos(false);

            if(enableGlass){
                enableGlass = false;
                Glass.Disable(View);
            }
        }

    }

}

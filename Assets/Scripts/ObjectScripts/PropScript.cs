using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropScript : MonoBehaviour
{
    // The point of which is the anchor for this part (usually 0,0,0...but could be different)
    public Vector3 AnchorPoint;
    // NOTE FOR THE BELOW: IT IS LOCAL DISPLACEMENT, MEANING IT WILL ALWAYS BE WITH RESPECT TO THE ROTATION OF THE OBJECT!
    // The amount of displacement variance when placing this object (so not everything seems so cut and dry) This controls 
    //     the amount of sway (goes from +-value (seperate random for each axis))
    public Vector3 PositionVariance;
    // The pre-offset for the displacement variance, basically added to before hand (useful for ensuring that the prop can 
    //    only display in the +x direction). 
    public Vector3 PositionVarianceOffset;
    public bool isPuzzle = false;// If it is a puzzle prop, then we should probs treat it different.
    [HideInInspector] public bool isUsedAsProp = false; // This is to let this prop know that it should NOT run it's puzzle
    //                                  code (the thing is for looks basically. This is useful for things like a painting).
    public bool ShouldRotate = false; // Note for corners, will orient itself to north or south facing (if SX or NX respectively)
    [Header("Prop Placement Affinities")]
    public Affinity Aff_Center;
    public Affinity Aff_West;
    public Affinity Aff_East;
    public Affinity Aff_North;
    public Affinity Aff_South;
    public Affinity Aff_NW;
    public Affinity Aff_NE;
    public Affinity Aff_SW;
    public Affinity Aff_SE;
    private float MyRandom(float lower, float upper)
    {
        if(Mathf.Approximately(lower, upper))
        {
            return lower;
        }
        return Random.Range(lower, upper);
    }
    public Vector3 GetFinalAnchor()
    {
        return new Vector3(MyRandom(-PositionVariance.x,PositionVariance.x), MyRandom(-PositionVariance.y, PositionVariance.y),
            MyRandom(-PositionVariance.z, PositionVariance.z)) + PositionVarianceOffset;
    }

}
[System.Serializable]
public class Affinity
{
    [Range(0,1f)]public float probPlaced; // How likely the item can be placed in this config. If it is 0 it will never be placed there, 
    //  if it is 1 it will always be ALLOWED to generate here. 
}

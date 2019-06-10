using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryProtector : MonoBehaviour
{

    public GameObject[] locals;
    public int CP = 0;
    public Vector3 targetSize;
    public void AddNewObject(Item go)
    {
        // The item is parented to the displayOrientation
        Quaternion localRot = go.displayOrientation.transform.localRotation;
        Vector3 size = go.displayOrientation.transform.localScale;    
        go.displayOrientation.transform.SetParent(locals[CP].transform);
        go.displayOrientation.transform.localPosition = Vector3.zero;
        // Rather, transform it by the given value in the item
        go.displayOrientation.transform.localScale = size;
        go.displayOrientation.transform.localRotation = localRot;
        CP++;
        
    }
}

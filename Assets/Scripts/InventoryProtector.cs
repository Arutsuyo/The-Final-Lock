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
        go.transform.SetParent(locals[CP].transform);
        // Rather, transform it by the given value in the item
        go.transform.rotation = Quaternion.identity;
        go.transform.localPosition = Vector3.zero;
        //locals[CP].transform.localScale = new Vector3(targetSize.x/bb.extents.x, targetSize.y / bb.extents.y, targetSize.z / bb.extents.z);
        
    }
}

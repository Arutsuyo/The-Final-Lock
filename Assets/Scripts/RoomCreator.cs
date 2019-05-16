using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCreator : MonoBehaviour
{
    // Yeah...basically this spawns a room and handles trying to create it for other people as well.
    public ObjStrPair[] SpawnableObjects;
    private Dictionary<string, GameObject> spawnables;

    public void Start()
    {
        foreach(ObjStrPair OSP in SpawnableObjects)
        {
            spawnables.Add(OSP.name, OSP.go);
        }
    }
    public void CreateRoom(EscapeRoom er)
    {
        // Technically I should go an look through the ER and do stuff..but for now lets just 
        // do a static spawn list.


    }
}
[System.Serializable]
public class ObjStrPair
{
    public string name;
    public GameObject go;
    public bool networked;
    
}

public class Room
{

}
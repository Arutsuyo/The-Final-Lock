using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable CS0618 // Type or member is obsolete
public class RoomObj : MonoBehaviour
{
    public Vector3 RoomSize;
    public SpawnableElement[] spawns;
    public Interactable[] toSpawn;
    public RequiredSpawning[] toImmediatelySpawn;
    private List<GraphNode<int>> SGT = new List<GraphNode<int>>();
    private LinkedList<int> topo = new LinkedList<int>();
    private int timed = 0;
    private bool goodSort = true;

    // Basically, go through the spawns, pick an item that must be spawned and place it in something that has a Requires of -1.
    public void DoTheSpawns()
    {

        // Build a tree... :(
        timed = 0;
        for(int i = 0; i < spawns.Length; i++)
        {
            SGT.Add(new GraphNode<int>() { node = i, par = -1, nodeColor = 0, discover = -1, finished = -1 });
        }
        foreach(GraphNode<int> I in SGT)
        {
            if(I.nodeColor == 0)
            {
                DFS_VISIT(I.node);
            }
        }
        if (goodSort)
        {
            foreach (int i in topo)
            {
                Debug.Log(i);
            }
        }
        else
        {
            Debug.Log("A cycle was detected.");
        }
        


    }
    public void Start()
    {
        DoTheSpawns();
    }
    public void DFS_VISIT(int i)
    {
        if (!goodSort) { return; }
        if(SGT[i].nodeColor == 1) { goodSort = false;  return; }
        timed++;
        SGT[i].discover = timed;
        SGT[i].nodeColor = 1;
        foreach(int j in spawns[i].Requires)
        {
            if(SGT[j].nodeColor == 0)
            {
                SGT[j].par = i;
                DFS_VISIT(j);
            }
        }
        SGT[i].nodeColor = 2;
        timed++;
        SGT[i].finished = timed;
        topo.AddLast(i);
    }


}
public class GraphNode<T>
{
    public T node;
    public T par;
    public int nodeColor = 0;
    public int discover = 0;
    public int finished = 0;
}
[System.Serializable]
public enum ObjectType { DOOR, PROPWALL, PROP, GAME, GAMEWALL};
[System.Serializable]
public class SpawnableElement
{
    public List<int> Requires = new List<int>(); // If empty, then this furniture doesn't require any puzzles to be solved. Otherwise, the puzzle item with this ID must be obtained before this can be opened. (Unused...?)
    public Transform location;

}


[System.Serializable]
public class RoomElement
{
    public ObjectType PieceType;
    public GameObject Piece; // Assumes it is a cube, will resize such that the anchorPosition is held still...
    public Vector3 AnchorPosition;
}

[System.Serializable]
public class RequiredSpawning
{
    public GameObject parentPrefab;
    public Transform location;
    public Reconnect callReconnect;
    public Interactable spawnPrefab;

}
#pragma warning restore CS0618 // Type or member is obsolete
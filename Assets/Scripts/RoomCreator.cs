using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class RoomCreator : MonoBehaviour
{
	// Yeah...basically this spawns a room and handles trying to create it for other people as well.
	public ObjStrPair[] SpawnableObjects;
	private Dictionary<string, SpaceDivisor> spawnables;
    
    public bool[][] map = new bool[50][];
    
    public List<GameObject> games = new List<GameObject>(); // Actual game objects in the game...

    public MDict<Vector3Int, Tile> tiles = new MDict<Vector3Int, Tile>();
    public bool restart = false;
    public float divisor = 3.0f;
    public float BSRLow = 0.0f;
    public float BSRHigh = 0.1f;
    public float cutoff = 0.5f;
    public int samples = 10;
    public int tileCount = 5;
    public int roomSize = 25;
    public CameraController cc;
    public int height = 3;
    public Dictionary<Vector3Int, DoorwayObj> doorways = new Dictionary<Vector3Int, DoorwayObj>();

    public List<int> actualTileCount = new List<int>();
    public PriorityQueue<Tile> canExpand = new PriorityQueue<Tile>();

    public GameObject player;
    public GameObject tempObject;
    public bool playerSpawned = false;


    int totalGenWeight = 0;
    int totalSeaWeight = 0;
    public List<int> generatorsWeights = new List<int>();
    public List<int> sealersWeights = new List<int>();
    public List<SpaceDivisor> roomGenerators = new List<SpaceDivisor>();
    public List<SpaceDivisor> roomSealers = new List<SpaceDivisor>();
    public List<Prop> props = new List<Prop>();
    [Header("Debugging/ToChangeToLoading")]
    public int numSections = 1;
    public int propCount = 2;
#pragma warning disable CS0618 // Type or member is obsolete
    public NetworkManager NM;
#pragma warning restore CS0618 // Type or member is obsolete
    public int puzzleCount = 2;
    public int floorSquareSize = 13;
    public int tries = 1000;
    public GameObject x1;
    public GameObject doorwayTemp;
    public int roomTileCount;
    public bool continues = false;
    int counter = 0;
    public IEnumerator StartB()
	{
        foreach(GameObject game in games)
        {
            Destroy(game);
        }
        games.Clear();



        // Start with 2 tiles, one active, one inactive (the inactive is to create a wall that will house the timer...

        Tile bt = new Tile(new Vector3Int(0, 0, 0));
        Tile bt1 = new Tile(new Vector3Int(0, 0, 1));
        Tile bt2 = new Tile(new Vector3Int(1, 0, 0));
        Tile bt3 = new Tile(new Vector3Int(1, 0, 1));
        bt.section = bt1.section = bt2.section = bt3.section = numSections;
        bt.fake = bt1.fake = bt2.fake = bt3.fake = false;
        GenerateWalls(bt, new bool[]{ true, false, false, false});
        GenerateWalls(bt1, new bool[] { true, false, false, false });
        bt.walls[2] = true;
        bt.walls[1] = true;
        bt1.walls[3] = true;
        bt1.walls[1] = true;
        bt2.walls[0] = true;
        bt3.walls[0] = true;
        bt2.walls[2] = true;
        bt3.walls[3] = true;
        List<Vector3Int> gj = bt.getWallsOpen();
        foreach (Vector3Int g in gj)
        {
            Debug.Log("\t"+g);
        }
        actualTileCount.Add(0);
        for(int i = 0; i < numSections; i++)
        {
            actualTileCount.Add(0);
        }
        actualTileCount[numSections] = 4;
        canExpand.Enqueue(bt, numSections);
        canExpand.Enqueue(bt1, numSections);
        canExpand.Enqueue(bt2, numSections);
        canExpand.Enqueue(bt3, numSections);
        
        tiles[bt.position] = bt;
        tiles[bt1.position] = bt1;
        tiles[bt2.position] = bt2;
        tiles[bt3.position] = bt3;
        while(canExpand.actCount != 0)
        {
            counter++;
            if(counter % 5 == 0)
            {
                continues = true;
                do
                {
                    yield return new WaitForSeconds(0.05f);
                } while (!continues);
                continues = false;
            }
            Tile VI = canExpand.Dequeue();
            
            SpaceDivisor SD = GetDivisor(VI.section);
            Debug.Log(VI + " " + SD.ToString());
            SpaceResult sr = new SpaceResult();
            if ((SD.GetType() == typeof(Doorway) && numSections >= 1) || SD.GetType() != typeof(Doorway))
            {
                sr = SD.Generate(Random.Range(0, 4), VI, this);
            }
            else
            {
                sr.success = false;
            }

            if (sr.success)
            {
                
                if (SD.GetType() == typeof(Doorway) && numSections >= 1 && sr.toAddToMaster.Count != 0)
                {
                    Debug.LogWarning("Type claimed: " + SD.GetType().Name + " " + sr.toAddToMaster.Count);
                    numSections--;
                    // Set the "other side" to the numSections...
                    Tile tts = null;
                    foreach(Tile t in sr.toAddToMaster)
                    {
                        Debug.Log((VI.position - t.position) + " " + VI.position + " " + t.position);
                        if((VI.position - t.position).sqrMagnitude == 1)
                        {
                            tts = t;
                            break;
                        }
                    }
                    Vector3Int NVP = tts.position + (tts.position - VI.position) * 2;
                    Tile tt2 = null;
                    foreach (Tile t in sr.toAddToMaster)
                    {
                        Debug.Log((NVP - t.position) + " " + NVP + " " + t.position);
                        if ((NVP - t.position).sqrMagnitude == 0)
                        {
                            tt2 = t;
                            break;
                        }
                    }
                    tt2.section = numSections;
                }
                Debug.Log("Success :D");
                foreach (Tile t in sr.toAddToMaster)
                {
                    tiles[t.position] = t;
                    if(t.blank || t.fake) { continue; }
                    if (t.section <= -1)
                    {
                        actualTileCount[VI.section]++;
                        t.section = VI.section;
                    }
                    else
                    {
                        actualTileCount[t.section]++;
                    }
                }
                foreach(Tile t in sr.toAddToQueue)
                {
                    canExpand.Enqueue(t, t.section);
                }
                foreach(Prop p in sr.toAddToProps)
                {
                    props.Add(p);
                }
            }
            else
            {
                // Try again with next generator...
                Debug.Log("Failed D:");
                canExpand.Enqueue(VI, numSections+1);
            }
        }
        Debug.Log("Initiating filler algorithm!");
        foreach(Vector3Int t in tiles.Keys)
        {
            this.GenerateWalls(tiles[t], new bool[] { true, true, true, true });
            if (tiles[t].connectionPending)
            {
                Prop p = SpaceDivisor.GeneratePropForTile(tiles[t], this);
                if (p != null)
                    props.Add(p);
            }
        }

        // Now lets do a room count... figure out what doors are COMPLETELY useless.
        List<HashSet<Tile>> sections = new List<HashSet<Tile>>();
        HashSet<Tile> seen = new HashSet<Tile>();
        HashSet<Vector3Int> doors = new HashSet<Vector3Int>();
        HashSet<Vector3Int> storedDoors = new HashSet<Vector3Int>();
        Stack<Tile> toVisit = new Stack<Tile>();
        Queue<Vector3Int> exploreQ = new Queue<Vector3Int>();
        exploreQ.Enqueue(new Vector3Int(0, 0, 0));
        while(exploreQ.Count != 0)
        {
            Vector3Int vv = exploreQ.Dequeue();
            if((seen.Contains(tiles[vv]) && vv != new Vector3Int(0, 0, 0)) || tiles[vv].fake || tiles[vv].blank)
            {
                continue;
            }
            // Otherwise >:3
            sections.Add(new HashSet<Tile>());
            storedDoors.UnionWith(doors);
            doors.Clear();
            toVisit.Push(tiles[vv]);
            while(toVisit.Count != 0)
            {
                counter++;
                if (counter % 5 == 0)
                {
                    continues = true;
                    do
                    {
                        yield return new WaitForSeconds(0.05f);
                    } while (!continues);
                    continues = false;
                }
                Tile tt = toVisit.Pop();
                if (seen.Contains(tt)) { continue; }
                if (doorways.ContainsKey(tt.position) && tt.position  != vv)
                {
                    // D:
                    if (doors.Contains(tt.position)){
                        // :D
                        // Remove from door, call it a day.
                        doors.Remove(tt.position);
                        //Destroy(doorways[tt.position].gogo);
                        doorways[tt.position].gogo.SetActive(false);
                        // >:D
                        //seen.Add(tiles[tt.position]);
                        toVisit.Push(tiles[tt.position]);
                    }
                    else
                    {
                        doors.Add(tt.position);
                    }
                }
                else
                {
                    seen.Add(tt);
                    sections[sections.Count - 1].Add(tt);
                    GenerateWallObjLastPass(tt, sections.Count - 1);
                    GenerateFloor(tt.position, sections.Count - 1);
                    // Now look at neighbors XD
                    Tile u = tiles[tt.position + new Vector3Int(0, 0, 1)];
                    Tile d = tiles[tt.position + new Vector3Int(0, 0, -1)];
                    Tile l = tiles[tt.position + new Vector3Int(-1, 0, 0)];
                    Tile r = tiles[tt.position + new Vector3Int(1, 0, 0)];
                    Debug.Log(tt +  "::\n" + (u != null ? u + " " + u.blank : "")+"\n" + (d != null ? d + " " + d.blank :"") + "\n" + (l != null ? l + " " + l.blank : "")+ "\n" + (r != null ? r + " " + r.blank :""));
                    if((!tt.realWalls[2]) && u != null && (!u.fake) && (!u.realWalls[3]) && (!u.blank))
                    {
                        toVisit.Push(u);
                    }
                    if((!tt.realWalls[3]) && d != null && (!d.fake) && (!d.realWalls[2]) && (!d.blank))
                    {
                        toVisit.Push(d);
                    }
                    if((!tt.realWalls[0]) && l != null && (!l.fake) && (!l.realWalls[1]) && (!l.blank))
                    {
                        toVisit.Push(l);
                    }
                    if((!tt.realWalls[1]) && r != null && (!r.fake) && (!r.realWalls[0]) && (!r.blank))
                    {
                        toVisit.Push(r);
                    }
                }

            }
            foreach(Vector3Int dor in doors)
            {
                Debug.Log("Got some doors: " + dor);
                exploreQ.Enqueue(dor);
            }
        }
        // Generate the search tree here....


        // Just generate ....:|
        Node<Mix<PropScript, List<Requires>>> propTree = new Node<Mix<PropScript, List<Requires>>>();
        propTree.node = new Mix<PropScript, List<Requires>>(null, new List<Requires>());
        propTree.node.second.Add(new Requires() { isItem = false, pt=PuzzleType.PUZZLE });
        propTree.node.second.Add(new Requires() { isItem = false, pt = PuzzleType.PUZZLE });


        // Root node :P

        Stack<Mix<Node<Mix<PropScript, List<Requires>>>, int>> treeSearch = new Stack<Mix<Node<Mix<PropScript, List<Requires>>>, int>>();
        treeSearch.Push(new Mix<Node<Mix<PropScript, List<Requires>>>, int>(propTree, 0));
        int puzzlesA = puzzleCount;
        while(treeSearch.Count != 0)
        {
            if(puzzlesA == 0)
            {
                // :D
                break;
            }
            // only pop if fails depth...
            Node<Mix<PropScript, List<Requires>>> pq = treeSearch.Peek().first;
            if(puzzlesA < pq.leafs.Count + treeSearch.Peek().second && Random.Range(0,1f) < Mathf.Atan(pq.leafs.Count / 5f) / (Mathf.PI/2f))
            {
                treeSearch.Pop();
                continue;
            }
            else
            {
                // Add stuff o-o
                // First determine if there is a requires... (main puzzle requires only 2...)
                // If you have a choice, remove puzzles first.....
                int RR = 0;
                bool breaks = false;
                Requires rrm = null;
                foreach(Requires r in pq.node.second)
                {
                    if(r.pt == PuzzleType.PUZZLE)
                    {
                        // REMOVE THIS FIRST!
                        breaks = true;
                        rrm = r;
                        break;
                    }
                    RR++;
                }
                Node<Mix<PropScript, List<Requires>>> mp = new Node<Mix<PropScript, List<Requires>>>();
                if (breaks)
                {
                    pq.node.second.RemoveAt(RR);
                    PropScript rp = puzzles[Random.Range(0, puzzles.Count)];
                    Requires[] ss = rp.puzzle.puzzleRequirements;
                    mp.node = new Mix<PropScript, List<Requires>>(rp, new List<Requires>(ss));
                    pq.leafs.Add(mp);
                    treeSearch.Push();
                }
                
                
                // If ss is null... :P

            }
        }

        /*
            [HideInInspector] public List<PropScript> puzzles;
            [HideInInspector] public List<PropScript> simpleProps;
            [HideInInspector] public List<PropScript> itemProps;
            [HideInInspector] public List<PropScript> hintProps;
         */
        foreach (Prop p in props){
			GenerateProp(p);
		}

        // Now for the pass of replacing props for valid stuff...







        // Finally, the player
        if (playerSpawned)
        {
            ResetPlayer();
        }
        else
        {
            SpawnPlayer();
            playerSpawned = true;
        }

    }
    public SpaceDivisor GetDivisor(int section)
    {
        float prob = (roomTileCount - actualTileCount[section] / Mathf.Sqrt(roomTileCount)) - .5f;
        if((roomTileCount - actualTileCount[section])/(roomTileCount) < -0.5f)
        {
            prob = -1;
        }
        if(Random.Range(0,1f) > prob || tries < 0)
        {
            if(numSections >= 1 && Random.Range(0, 1f) >= .35f + (Mathf.Sqrt(Mathf.Sqrt(numSections))/3f))
            {
                return new Doorway();
            }
            int i = Random.Range(0, totalSeaWeight);
            for(int j = 0; j < roomSealers.Count; j++)
            {
                if(i < sealersWeights[j])
                {
                    return roomSealers[j];
                }
                i -= sealersWeights[j];
            }
            return roomSealers[roomSealers.Count - 1];
        }
        else
        {
            tries--;
            int i = Random.Range(0, totalGenWeight);
            for (int j = 0; j < roomGenerators.Count; j++)
            {
                if (i < generatorsWeights[j])
                {
                    return roomGenerators[j];
                }
                i -= generatorsWeights[j];
            }
            return roomGenerators[roomGenerators.Count - 1];
        }

    }
    public void SpawnPlayer()
    {
        GameObject p = Instantiate(player);
        CameraController cc1 = p.GetComponentInChildren<CameraController>();
        cc1.iconPick = cc.iconPick;
        cc1.iconTension = cc.iconTension;
        cc1.iconKey = cc.iconKey;
        cc1.zoomCameraTarget = cc.zoomCameraTarget;
        cc1.centerMarker = cc.centerMarker;
        cc1.hitMarker = cc.hitMarker;
        p.transform.position = new Vector3(0, 2, 0);
        cc.player = p;
        //cc.enabled = true;


    }
    public void ResetPlayer()
    {
        cc.player.transform.position = new Vector3(0, 2, 0);
    }
    public void GenerateTimer(Vector3Int pos)
    {
        // need to do this o-o
    }
   
    public void GenerateWalls(Tile t, bool[] newWalls)
    {
        if (t.fake || t.blank)
        {
            return;
        }
        if(newWalls.Length != 4)
        {
            throw new System.Exception("Generate walls expects EXACTLY 4 ARGUMENTS!");
        }
        Debug.Log("Wall generation for "+t+": {"+newWalls[0] + newWalls[1] + newWalls[2] + newWalls[3]+"}");
        Vector2Int vt = new Vector2Int(0, 0);
        if(newWalls[0] && !t.walls[0])
        {
            vt.x = -1;
        }
        if(newWalls[1] && !t.walls[1])
        {
            vt.x = (vt.x == -1 ? 10 : 1);
        }
        if (newWalls[2] && !t.walls[2])
        {
            vt.y = 1;
        }
        if (newWalls[3] && !t.walls[3])
        {
            vt.y = (vt.y == 1 ? 10 : -1);
        }
        GenerateWallObj(t, vt);
        t.walls[0] = t.walls[0] || newWalls[0];
        t.walls[1] = t.walls[1] || newWalls[1];
        t.walls[2] = t.walls[2] || newWalls[2];
        t.walls[3] = t.walls[3] || newWalls[3];
    }
    private void GenerateWallObjLastPass(Tile t, int refSection)
    {
        // Generates walls based off various designs. Will pick out a random wall design...
        if(t.blank || t.fake) { return; }
        if (t.realWalls[0])
        {
            GameObject go = Instantiate(x1);
            go.transform.localScale = new Vector3(floorSquareSize / 10.0f, 0.1f, 0.8f);
            go.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 90));
            go.transform.position = new Vector3((t.position.x - .5f) * floorSquareSize, t.position.y + 4f, t.position.z * floorSquareSize);
            go.SetActive(true);
            games.Add(go);
        }
        if (t.realWalls[1])
        {
            GameObject go = Instantiate(x1);
            go.transform.localScale = new Vector3(floorSquareSize / 10.0f, 0.1f, 0.8f);
            go.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 90));
            go.transform.position = new Vector3((t.position.x + .5f) * floorSquareSize, t.position.y + 4f, t.position.z * floorSquareSize);
            go.SetActive(true);
            games.Add(go);
        }
        if (t.realWalls[2])
        {
            GameObject go = Instantiate(x1);
            go.transform.localScale = new Vector3(floorSquareSize / 10.0f, 0.1f, 0.8f);
            go.transform.localRotation = Quaternion.Euler(new Vector3(90, 90, 90));
            go.transform.position = new Vector3((t.position.x) * floorSquareSize, t.position.y + 4f, (t.position.z+.5f) * floorSquareSize);
            go.SetActive(true);
            games.Add(go);
        }
        if (t.realWalls[3])
        {
            GameObject go = Instantiate(x1);
            go.transform.localScale = new Vector3(floorSquareSize / 10.0f, 0.1f, 0.8f);
            go.transform.localRotation = Quaternion.Euler(new Vector3(90, 90, 90));
            go.transform.position = new Vector3((t.position.x)* floorSquareSize, t.position.y + 4f, (t.position.z - .5f) * floorSquareSize);
            go.SetActive(true);
            games.Add(go);
        }

    }
    public void GenerateWallObj(Tile t, Vector2Int wallPos)
    {
        // If Vector2Int.x/y is equal to 10, then it refers to BOTH sides
        if (wallPos.x == -1 || wallPos.x == 10)
        {
            t.realWalls[0] = true;
            if (tiles[t.position + new Vector3Int(-1, 0, 0)] != null)
            {
                tiles[t.position + new Vector3Int(-1, 0, 0)].realWalls[1] = true;
                tiles[t.position + new Vector3Int(-1, 0, 0)].walls[1] = true;
            }
        }
        if(wallPos.x == 1 || wallPos.x == 10)
        {
            t.realWalls[1] = true;
            if (tiles[t.position + new Vector3Int(1, 0, 0)] != null)
            {
                tiles[t.position + new Vector3Int(1, 0, 0)].realWalls[0] = true;
                tiles[t.position + new Vector3Int(1, 0, 0)].walls[0] = true;
            }
        }
        if (wallPos.y == -1 || wallPos.y == 10)
        {
            t.realWalls[3] = true;
            if (tiles[t.position + new Vector3Int(0, 0, -1)] != null)
            {
                tiles[t.position + new Vector3Int(0, 0, -1)].realWalls[2] = true;
                tiles[t.position + new Vector3Int(0, 0, -1)].walls[2] = true;
            }
        }
        if (wallPos.y == 1 || wallPos.y == 10)
        {
            t.realWalls[2] = true;
            if (tiles[t.position + new Vector3Int(0, 0, 1)] != null)
            {
                tiles[t.position + new Vector3Int(0, 0, 1)].realWalls[3] = true;
                tiles[t.position + new Vector3Int(0, 0, 1)].walls[3] = true;
            }
        }
        //GenerateWallObjLastPass(t, 0);
    }
    
    public void GenerateDoorway(Vector3Int pos, bool dirIsUpDown)
    {
        
        // Generate a doorway object?
        GameObject go = Instantiate(doorwayTemp);
        go.transform.localRotation = (dirIsUpDown ? new Quaternion() : Quaternion.Euler(0,90,0));
        // Should be the right scale o-o
        Vector3 vv = new Vector3(0,0,0) + pos;
        vv.Scale(GetFloorScaling());
        go.transform.localPosition = vv;
        go.SetActive(true);
        doorways.Add(pos, new DoorwayObj() { pos = pos, UD = dirIsUpDown ,gogo = go});
    }

    [HideInInspector] public List<PropScript> puzzles;
    [HideInInspector] public List<PropScript> simpleProps;
    [HideInInspector] public List<PropScript> itemProps;
    [HideInInspector] public List<PropScript> hintProps;

    private PropScript nextProp = null;
    public List<PropScript> allProps;
    public int PropTrials = 10; // will simply randomly pick one this many times, if it fails beyond that it returns a failure.
    // Ush

    private void SpawnProp(Prop p)
    {
        GameObject ggo = Instantiate(nextProp.gameObject);
        
        // Technically do more...
        if (nextProp.ShouldRotate)
        {
            if(p.alignment.x != p.alignment.y || p.alignment.x != 0) 
            {
                // UGH D:
                // do an actual rotation by rotating the transform with a *=
                Quaternion q = Quaternion.Euler(0, (p.alignment.x == -1 ? -90 : (p.alignment.x == 1 ? 90 : (p.alignment.y == -1 ? 180 : 0))), 0);
                ggo.transform.rotation *= q;
            }
        }
        ggo.transform.position = p.anchorPos;
        ggo.transform.localPosition += ggo.transform.rotation *(nextProp.AnchorPoint + nextProp.GetFinalAnchor());
        ggo.SetActive(true);
        if (nextProp.isNetworked)
        {
            // Search and destroy T_T
            NetworkIdentity[] dd = ggo.GetComponentsInChildren<NetworkIdentity>();
            foreach(NetworkIdentity ni in dd)
            {
                NetworkServer.Spawn(ni.gameObject);
                
            }
        }
        if (nextProp.isPuzzle || nextProp.isDynamicProp)
        {
            if (nextProp.isDynamicProp || nextProp.isUsedAsProp)
            {
                ggo.GetComponent<PropScript>().puzzle.GenerateAsProp((long)Random.Range(0, 5555555));
            }
            else
            {
                //Requires[] ss = ggo.GetComponent<PropScript>().puzzle.GenerateAsPuzzle((long)Random.Range(0, 5555555));

                // TODO deal with the dependencies.. :|
            }
        }
    }
    public bool GenerateProp(Prop p)
    {
        // Attempt to generate a prop given the position...however, we will scan for about
        for(int i = 0; i < PropTrials; i++)
        {
            if(p.alignment.x == p.alignment.y && p.alignment.x == 0)
            {
                // Center
                if (nextProp.Aff_Center.probPlaced >= Random.Range(Mathf.Epsilon, 1f))
                {
                    SpawnProp(p);
                    return true;
                }
            }
            else if(p.alignment.x == 0 && p.alignment.y == -1)
            {
                // S
                if (nextProp.Aff_South.probPlaced >= Random.Range(Mathf.Epsilon, 1f))
                {
                    SpawnProp(p);
                    return true;
                }
            }
            else if(p.alignment.x == 0 && p.alignment.y == 1)
            {
                if (nextProp.Aff_North.probPlaced >= Random.Range(Mathf.Epsilon, 1f))
                {
                    SpawnProp(p);
                    return true;
                }
            }
            else if (p.alignment.x == 1 && p.alignment.y == 1)
            {
                if (nextProp.Aff_NE.probPlaced >= Random.Range(Mathf.Epsilon, 1f))
                {
                    SpawnProp(p);
                    return true;
                }
            }
            else if (p.alignment.x == -1 && p.alignment.y == 1)
            {
                if (nextProp.Aff_NW.probPlaced >= Random.Range(Mathf.Epsilon, 1f))
                {
                    SpawnProp(p);
                    return true;
                }
            }
            else if (p.alignment.x == 1 && p.alignment.y == -1)
            {
                if (nextProp.Aff_SE.probPlaced >= Random.Range(Mathf.Epsilon, 1f))
                {
                    SpawnProp(p);
                    return true;
                }
            }
            else if (p.alignment.x == -1 && p.alignment.y == -1)
            {
                if (nextProp.Aff_SW.probPlaced >= Random.Range(Mathf.Epsilon, 1f))
                {
                    SpawnProp(p);
                    return true;
                }
            }
            else if (p.alignment.x == 1 && p.alignment.y == 0)
            {
                if (nextProp.Aff_East.probPlaced >= Random.Range(Mathf.Epsilon, 1f))
                {
                    SpawnProp(p);
                    return true;
                }
            }
            else if (p.alignment.x == -1 && p.alignment.y == 0)
            {
                if (nextProp.Aff_West.probPlaced >= Random.Range(Mathf.Epsilon, 1f))
                {
                    SpawnProp(p);
                    return true;
                }
            }
            PickNextProp();
        }
        return false;
    }

    public void PickNextProp()
    {
        nextProp = simpleProps[Random.Range(0,simpleProps.Count)];
    }

    public void GenerateFloor(Vector3Int pos, int refSection)
    {
        GameObject go = Instantiate(x1);
        go.transform.localScale = new Vector3(floorSquareSize / 10.0f, 0.1f, floorSquareSize / 10.0f);
        go.transform.position = new Vector3(floorSquareSize*pos.x, pos.y * 5, floorSquareSize*pos.z);
        go.SetActive(true);
        go.name = "Floor (" + pos.x + ", "+ pos.y + ", " + pos.z+")";
        Material m = go.GetComponentInChildren<MeshRenderer>().material;
        m.color = Color.Lerp(new Color(1f, 0, 0), new Color(0, 1f, 1f), Mathf.Repeat(refSection / (5f + Mathf.PI), 1f));
        games.Add(go);
    }
    public Vector3 GetFloorScaling()
    {
        return new Vector3(floorSquareSize, 5, floorSquareSize);
    }

    public void AttachCamera(GameObject tr)
    {
        cc.player = tr;
        cc.enabled = true;
    }

    public static V gSD<K,V>(Dictionary<K,V> v, K k, V def) // get safe dictionary
    {
        if (v.ContainsKey(k))
        {
            return v[k];
        }
        else
        {
            v.Add(k, def);
            return def;
        }
    }

    public void Update()
    {
        if (restart)
        {
            restart = false;
            StartCoroutine(StartB());
        }
    }
	public void CreateRoom(EscapeRoom er)
	{
		// Technically I should go an look through the ER and do stuff..but for now lets just 
		// do a static spawn list.


	}
    public void AddRG(SpaceDivisor div, int weight)
    {
        roomGenerators.Add(div);
        generatorsWeights.Add(weight);
        totalGenWeight += weight;
    }
    public void AddRS(SpaceDivisor div, int weight)
    {
        roomSealers.Add(div);
        sealersWeights.Add(weight);
        totalSeaWeight += weight;
    }
    public void Start()
    {
        AddRS(new WallSpace(), 50);
        AddRG(new Hallway(), 40);
        AddRG(new SmallRoom(), 50);
        
        nextProp = allProps[0];
        simpleProps.AddRange(allProps);
        foreach(PropScript p in allProps)
        {
            if (p.isPuzzle)
            {
                puzzles.Add(p);
            }
            if (p.canHint)
            {
                hintProps.Add(p);
            }
            if (p.canHoldItem)
            {
                itemProps.Add(p);
            }
            if (p.canBeProp) { 
                simpleProps.Add(p);
            }
        }
        NM.StartHost();
        StartCoroutine(StartB());
    }
}

public class Mix<M, N>
{
    public M first;
    public N second;
    public Mix(M m, N n)
    {
        first = m;
        second = n;
    }
}
public class WallSpace : SpaceDivisor
{
    public override SpaceResult Generate(int param, Tile tiq, RoomCreator c)
    {
        SpaceResult sr = new SpaceResult();
        if(tiq.wallsOcc() == 4)
        {
            sr.success = true;
            return sr;
        }
        List<Vector3Int> newWall = tiq.getWallsOpen(c);
        if(newWall.Count == 0)
        {
            sr.success = true;
            return sr;
        }
        bool[] walls = tiq.getCWalls();
        walls[tiq.getWallFromDir(newWall[Random.Range(0, newWall.Count)])]= true;
        c.GenerateWalls(tiq, walls);
        if(newWall.Count > 1)
        {
            sr.toAddToQueue.Add(tiq);
        }
        sr.success = true;
        return sr;
    }
}
public class Doorway : SpaceDivisor
{
    public override SpaceResult Generate(int param, Tile tiq, RoomCreator c)
    {
        SpaceResult sr = new SpaceResult();
        int[][] kernel = new int[3][];
        for(int i = 0; i < 3; i++)
        {
            kernel[i] = new int[3];
        }
        if (tiq.wallsOcc() == 4)
        {
            sr.success = false;
            return sr;
        }
        List<Vector3Int> newWalls = tiq.getWallsOpen(c);
        if (newWalls.Count == 0)
        {
            sr.success = false;
            return sr;
        }

        int mc = newWalls.Count; // To ensure that you remove stuff :|
        while (newWalls.Count != 0 && mc >= 0)
        {
            mc--;
            Vector3Int dir = newWalls[Random.Range(0, newWalls.Count)] - tiq.position;
            newWalls.Remove(dir);
            if(dir.x != 0)
            {
                kernel[0][0] = kernel[0][2] = kernel[2][0] = kernel[2][2] = 0;
                kernel[1][1] = 1;
                kernel[1][0] = kernel[1][2] = 0;
                kernel[0][1] = (dir.x == -1 ? 3 : 2);
                kernel[2][1] = (dir.x == -1 ? 2 : 3);
            }
            else
            {
                kernel[0][0] = kernel[0][2] = kernel[2][0] = kernel[2][2] = -1;
                kernel[1][1] = 1;
                kernel[0][1] = kernel[2][1] = 0;
                kernel[1][0] = (dir.z == -1 ? 3 : 2);
                kernel[1][2] = (dir.z == -1 ? 2 : 3);
            }

            if (SpaceDivisor.CanGenerate(kernel, c, tiq.position + dir))
            {
                sr = SpaceDivisor.GenerateMap(kernel, c, tiq.position + dir);
                tiq.walls[tiq.getWallFromDir(dir + tiq.position)] = true;
                c.GenerateDoorway(tiq.position + dir + dir, dir.z != 0);
                break;
            }
        }
        
        return sr;
    }
}
public class SmallRoom : SpaceDivisor
{
    public static float DoorDecr = .85f;
    public override SpaceResult Generate(int param, Tile tiq, RoomCreator c)
    {
        SpaceResult sr = new SpaceResult();
        /*
         * Param: odd
         *      Square of random size (2x2, 3x3 or 4x4)
         * Param: even
         *      Diamond of random size ("3x3", or "5x5"),
         */
        float size = Random.Range(0f, 1f);
        int[][] kernel;
        if(size < .33f)
        {
            kernel = new int[3][];
        }else if(size < .66f)
        {
            kernel = new int[4][];
        }else{
            kernel = new int[6][];
        }
        for(int i = 0; i < kernel.Length; i++)
        {
            kernel[i] = new int[kernel.Length];
            for (int j = 0; j < kernel.Length; j++)
            {
                float k = Random.Range(0f, 1f);
                if (k < 0.85f)
                {
                    kernel[i][j] = 1; // Fill it...
                }
                else
                {
                    kernel[i][j] = 0; // Make it a void...
                }
                //Debug.Log(i + " " + j + " " + kernel[i][j]);
            }
        }
        // Need to do a "can I get to every spot from here"
        // go along outer edge, add either a 2 or a 3...

        float numDoors = DoorDecr;
        
        for(int i = 0; i < kernel.Length; i++)
        {
            if(kernel[i][0] == 1)
            {
                if(Random.Range(0f, 1f) <= numDoors)
                {
                    if (Mathf.Approximately(numDoors, .85f))
                        kernel[i][0] = 2;
                    else
                        kernel[i][0] = 3;

                    numDoors *= DoorDecr;
                }
            }
            if (kernel[i][kernel[0].Length-1] == 1)
            {
                if (Random.Range(0f, 1f) <= numDoors)
                {
                    if (Mathf.Approximately(numDoors, .85f))
                        kernel[i][kernel[0].Length-1] = 2;
                    else
                        kernel[i][kernel[0].Length-1] = 3;

                    numDoors *= DoorDecr;
                }
            }
        }
        for (int i = 0; i < kernel[0].Length; i++)
        {
            if (kernel[0][i] == 1)
            {
                if (Random.Range(0f, 1f) <= numDoors)
                {
                    if (Mathf.Approximately(numDoors, .85f))
                        kernel[0][i] = 2;
                    else
                        kernel[0][i] = 3;

                    numDoors *= DoorDecr;
                }
            }
            if (kernel[kernel.Length-1][i] == 1)
            {
                if (Random.Range(0f, 1f) <= numDoors)
                {
                    if (Mathf.Approximately(numDoors, .85f))
                        kernel[kernel.Length-1][i] = 2;
                    else
                        kernel[kernel.Length-1][i] = 3;

                    numDoors *= DoorDecr;
                }
            }
        }
        SpaceDivisor.PrintKernel(kernel);
        if (!SpaceDivisor.IsConnected(kernel))
        {
            Debug.Log("Unsuccessful generation.");
            sr.success = false;
            return sr;
        }
        Debug.Log("Successful generation.");
        if (tiq.wallsOcc() == 4)
        {
            sr.success = false;
            return sr;
        }
        List<Vector3Int> newWalls = tiq.getWallsOpen(c);
        if (newWalls.Count == 0)
        {
            sr.success = false;
            return sr;
        }
        
        int mc = newWalls.Count; // To ensure that you remove stuff :|
        while(newWalls.Count != 0 && mc >= 0)
        {
            mc--;
            Vector3Int dir = newWalls[Random.Range(0, newWalls.Count)] - tiq.position;
            newWalls.Remove(dir);
            if(SpaceDivisor.CanGenerate(kernel, c, tiq.position + dir))
            {
                sr = SpaceDivisor.GenerateMap(kernel, c, tiq.position + dir);
                tiq.walls[tiq.getWallFromDir(dir + tiq.position)] = true;
                break;
            }
            
            
        }

        if (!sr.success)
        {
            sr.toAddToQueue.Add(tiq);
        }
        else
        {
            // We will now perform a pass for item generation!
            foreach(Tile t in sr.toAddToMaster)
            {
                if (!t.connectionPending)
                {
                    Prop sr1 = SpaceDivisor.GeneratePropForTile(t, c);
                    if (sr1 != null)
                    {
                        sr.toAddToProps.Add(sr1);
                    }
                }
            }
        }
        return sr;
    }
}

public class Hallway : SpaceDivisor
{
    public static float BranchChance = .5f;
    public override SpaceResult Generate(int param, Tile tiq, RoomCreator c)
    {
        SpaceResult sr = new SpaceResult();
        if(param <= 1)
        {
            sr.success = true;
            sr.toAddToQueue.Add(tiq);
            return sr;
        }
        if (tiq.wallsOcc() == 4)
        {
            sr.success = false;
            return sr;
        }
        List<Vector3Int> newWalls = tiq.getWallsOpen(c);
        if(newWalls.Count == 0)
        {
            sr.success = false;
            return sr;
        }
        int mc = 4;
        while(newWalls.Count != 0 && mc >= 0)
        {
            mc--;
            Vector3Int dir = newWalls[Random.Range(0, newWalls.Count)]  - tiq.position;
            newWalls.Remove(dir);
            // Now generate a hallway kernel...
            // Simple 1xn or nx1
            int length = (Random.Range(0,1f) <= .33f ? 2 : (Random.Range(0,1f) >= .5f ? 4 : 5));
            int[][] kernel = null;
            if(dir.x != 0)
            {
                kernel = new int[length][];
                for (int i = 0; i < kernel.Length; i++)
                {
                    kernel[i] = new int[1];
                    kernel[i][0] = (i == 0 && dir.x == 1? 2 : (i == length - 1 && dir.x == -1 ? 2 : ((i == length-1 && dir.x == 1) || (i == 0 && dir.x == -1) ? 3 : (Random.Range(0f, 1f) >= BranchChance ? 1 : 3))));
                }
            }
            else
            {
                kernel = new int[1][];
                kernel[0] = new int[length];
                for (int i = 0; i < kernel[0].Length; i++)
                {
                    kernel[0][i] = (i == 0 && dir.z == 1 ? 2 : (i == length - 1 && dir.z == -1 ? 2 : ((i == length - 1 && dir.z == 1) || (i == 0 && dir.z == -1) ? 3 : 1)));
                }
            }
            if (SpaceDivisor.CanGenerate(kernel, c, tiq.position + dir))
            {
                sr = SpaceDivisor.GenerateMap(kernel, c, tiq.position + dir);
                tiq.walls[tiq.getWallFromDir(dir + tiq.position)] = true;
                break;
            }
        }
        if (!sr.success)
        {
            sr.toAddToQueue.Add(tiq);
        }
        else
        {
            // We will now perform a pass for item generation!
            foreach (Tile t in sr.toAddToMaster)
            {
                if (!t.connectionPending)
                {
                    Prop sr1 = SpaceDivisor.GeneratePropForTile(t, c);
                    if (sr1 != null)
                    {
                        sr.toAddToProps.Add(sr1);
                    }
                }
            }
        }
        return sr;

    }
}


public class Tile
{
    public int section = -1; // <=-1 means whatever the reference tile is, any other is an actual value. (Should be decreasing...)
    //                                x-     x+      z+      z-
    public bool[] walls = new bool[]{false, false, false, false };
    public Vector3Int position;
    public bool fake = true;
    public bool blank = false; // If it is blank, it shall never be a walkable tile!
    public bool[] realWalls = new bool[] {false, false, false, false }; // DO NOT CHANGE! Only read from these....
    public bool connectionPending = false;
    public Tile(Vector3Int pos)
    {
        position = pos;
    }
    public bool[] getCWalls()
    {
        return new bool[] {walls[0], walls[1], walls[2], walls[3] };
    }
    public override string ToString()
    {
        return "Tile: " + position + " (L:" + walls[0] + ", R:" + walls[1]+ ", U:" + walls[2]+ ", D:" + walls[3]+"). " + fake + " " + blank;
    }
    public int wallsOcc()
    {
        return (walls[0] ? 1 : 0) + (walls[1] ? 1 : 0) + (walls[2] ? 1 : 0) + (walls[3] ? 1 : 0);
    }
    public List<Vector3Int> getWallsOpen()
    {
        List<Vector3Int> nIL = new List<Vector3Int>();
        if (!walls[0]) { nIL.Add(position - new Vector3Int(1, 0, 0)); }
        if (!walls[1]) { nIL.Add(position + new Vector3Int(1, 0, 0)); }
        if (!walls[2]) { nIL.Add(position + new Vector3Int(0, 0, 1)); }
        if (!walls[3]) { nIL.Add(position - new Vector3Int(0, 0, 1)); }
        return nIL;
    }

    public static Vector3Int getLeftOfDir(Vector3Int dir)
    {
        Vector3 v = Vector3.Cross(dir, Vector3.up);
        return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }

    public int getWallFromDir(Vector3Int pos)
    {
        if((pos-position).sqrMagnitude != 1) { throw new System.Exception("Bad direction! Position: " + position + " Try to ref to: " + pos); }

        if (pos.y != position.y) { throw new System.Exception("Bad direction! Can not be going down or up!"); }

        if(pos.x != position.x)
        {
            return (position.x < pos.x) ? 1 : 0;
        }
        return (position.z < pos.z) ? 2 : 3;

    }

    public List<Vector3Int> getWallsOpen(RoomCreator c)
    {   
        List<Vector3Int> n = getWallsOpen(); // Quick side call..
        int i = 0;
        for(; i < n.Count; i++)
        {
            Vector3Int cc = n[i];
            if(c.tiles[cc] == null)
            {
                continue;
            }
            if (c.tiles[cc].walls[c.tiles[cc].getWallFromDir(position)])
            {
                // NOT OPEN :(
                // Set ours like that D:
                walls[getWallFromDir(cc)] = true; // D:
                n.RemoveAt(i);
                i--;
                continue;
            }

        }

        return n;
    }
}

[System.Serializable]
public class ObjStrPair
{
	public string name;
	public SpaceDivisor go;
	public bool networked;

}

public class Room
{

}

public class MDict<K,V> : Dictionary<K, V>
{
    public new V this[K key]
    {
        get
        {
            if (this.ContainsKey(key))
            {
                return base[key];
            }
            else
            {
                return default;
            }
        }
        set
        {
            if (this.ContainsKey(key))
            {
                base[key] = value;
            }
            else
            {
                base.Add(key, value);
            }
        }
    }
}


// FROM https://stackoverflow.com/questions/1937690/c-sharp-priority-queue
public class PriorityQueue<T> 
{
    SortedList<Pair<int>, T> _list;
    int count;
    public int actCount { get; private set; }

    public PriorityQueue()
    {
        _list = new SortedList<Pair<int>, T>(new PairComparer<int>());
        actCount = 0;
    }

    public void Enqueue(T item, int priority)
    {
        _list.Add(new Pair<int>(priority, -count), item);
        count++;
        actCount++;
    }
    public T Peek()
    {
        T item = _list[_list.Keys[0]];
        return item;
    }
    public bool isEmpty()
    {
        return actCount== 0;
    }
    public T Dequeue()
    {
        T item = _list[_list.Keys[0]];
        _list.RemoveAt(0);
        actCount--;
        return item;
    }
}

class Node<T>
{
    public T node;
    public List<Node<T>> leafs = new List<Node<T>>();
}

class Pair<T>
{
    public T First { get; private set; }
    public T Second { get; private set; }

    public Pair(T first, T second)
    {
        First = first;
        Second = second;
    }

    public override int GetHashCode()
    {
        return First.GetHashCode() ^ Second.GetHashCode();
    }

    public override bool Equals(object other)
    {
        Pair<T> pair = other as Pair<T>;
        if (pair == null)
        {
            return false;
        }
        return (this.First.Equals(pair.First) && this.Second.Equals(pair.Second));
    }
}

class PairComparer<T> : IComparer<Pair<T>> where T : System.IComparable
{
    public int Compare(Pair<T> x, Pair<T> y)
    {
        if (x.First.CompareTo(y.First) < 0)
        {
            return -1;
        }
        else if (x.First.CompareTo(y.First) > 0)
        {
            return 1;
        }
        else
        {
            return x.Second.CompareTo(y.Second);
        }
    }
}
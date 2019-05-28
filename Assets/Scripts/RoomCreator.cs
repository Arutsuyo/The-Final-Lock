using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCreator : MonoBehaviour
{
	// Yeah...basically this spawns a room and handles trying to create it for other people as well.
	public ObjStrPair[] SpawnableObjects;
	private Dictionary<string, SpaceDivisor> spawnables;
    public GameObject x1;
    public GameObject doorwayTemp;
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
    public CameraController cc;
    public int height = 3;
    public List<DoorwayObj> doorways = new List<DoorwayObj>();

    public int actualTileCount = 0;
    public Queue<Tile> canExpand = new Queue<Tile>();

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
    public int puzzleCount = 2;
    public int floorSquareSize = 13;
    public int tries = 1000;
    public int approxNumTiles;
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
        GenerateFloor(bt.position);
        GenerateFloor(bt1.position);
        GenerateFloor(bt2.position);
        GenerateFloor(bt3.position);
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
        actualTileCount = 4;
        canExpand.Enqueue(bt);
        canExpand.Enqueue(bt1);
        canExpand.Enqueue(bt2);
        canExpand.Enqueue(bt3);
        if (playerSpawned) {
            ResetPlayer();
        }
        else
        {
            SpawnPlayer();
            playerSpawned = true;
        }
        
        tiles[bt.position] = bt;
        tiles[bt1.position] = bt1;
        tiles[bt2.position] = bt2;
        tiles[bt3.position] = bt3;
        while(canExpand.Count != 0)
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
            
            SpaceDivisor SD = GetDivisor();
            Debug.Log(VI + " " + SD.ToString());
            SpaceResult sr =  SD.Generate(Random.Range(0,4), VI, this);
            if (sr.success)
            {
                Debug.Log("Success :D");
                foreach (Tile t in sr.toAddToMaster)
                {
                    tiles[t.position] = t;
                    actualTileCount++;
                }
                foreach(Tile t in sr.toAddToQueue)
                {
                    canExpand.Enqueue(t);
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
                canExpand.Enqueue(VI);
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

        // Now for the pass of replacing props for valid stuff...
	}
    public SpaceDivisor GetDivisor()
    {
        float prob = Random.Range(-0.1f, 0.1f) + 1.0f / (1.0f + Mathf.Exp((Mathf.Sqrt(counter)/(approxNumTiles)) - (approxNumTiles - actualTileCount)/(Mathf.Sqrt(approxNumTiles))));
        if(Random.Range(0,1f) > prob || tries < 0)
        {
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
    public void GenerateWallObj(Tile t, Vector2Int wallPos)
    {
        // If Vector2Int.x/y is equal to 10, then it refers to BOTH sides
        if (wallPos.x == -1 || wallPos.x == 10)
        {
            GameObject go = Instantiate(x1);
            go.transform.localScale = new Vector3(floorSquareSize / 10.0f, 0.1f, 0.8f);
            go.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 90));
            go.transform.position = new Vector3((t.position.x - .5f) * floorSquareSize, t.position.y + 4f, t.position.z * floorSquareSize);
            go.SetActive(true);
            t.realWalls[0] = true;
            games.Add(go);
            if (tiles[t.position + new Vector3Int(-1, 0, 0)] != null)
            {
                tiles[t.position + new Vector3Int(-1, 0, 0)].realWalls[1] = true;
                tiles[t.position + new Vector3Int(-1, 0, 0)].walls[1] = true;
            }
        }
        if(wallPos.x == 1 || wallPos.x == 10)
        {
            GameObject go = Instantiate(x1);
            go.transform.localScale = new Vector3(floorSquareSize / 10.0f, 0.1f, 0.8f);
            go.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 90));
            go.transform.position = new Vector3((t.position.x + .5f) * floorSquareSize, t.position.y + 4f, t.position.z * floorSquareSize);
            go.SetActive(true);
            t.realWalls[1] = true;
            games.Add(go);
            if (tiles[t.position + new Vector3Int(1, 0, 0)] != null)
            {
                tiles[t.position + new Vector3Int(1, 0, 0)].realWalls[0] = true;
                tiles[t.position + new Vector3Int(1, 0, 0)].walls[0] = true;
            }
        }
        if (wallPos.y == -1 || wallPos.y == 10)
        {
            GameObject go = Instantiate(x1);
            go.transform.localScale = new Vector3(floorSquareSize / 10.0f, 0.1f, 0.8f);
            go.transform.localRotation = Quaternion.Euler(new Vector3(90, 90, 90));
            go.transform.position = new Vector3((t.position.x) * floorSquareSize, t.position.y + 4f, (t.position.z-.5f) * floorSquareSize);
            go.SetActive(true);
            t.realWalls[3] = true;
            games.Add(go);
            if (tiles[t.position + new Vector3Int(0, 0, -1)] != null)
            {
                tiles[t.position + new Vector3Int(0, 0, -1)].realWalls[2] = true;
                tiles[t.position + new Vector3Int(0, 0, -1)].walls[2] = true;
            }
        }
        if (wallPos.y == 1 || wallPos.y == 10)
        {
            GameObject go = Instantiate(x1);
            go.transform.localScale = new Vector3(floorSquareSize / 10.0f, 0.1f, 0.8f);
            go.transform.localRotation = Quaternion.Euler(new Vector3(90, 90, 90));
            go.transform.position = new Vector3((t.position.x)* floorSquareSize, t.position.y + 4f, (t.position.z + .5f) * floorSquareSize);
            go.SetActive(true);
            t.realWalls[2] = true;
            games.Add(go);
            if (tiles[t.position + new Vector3Int(0, 0, 1)] != null)
            {
                tiles[t.position + new Vector3Int(0, 0, 1)].realWalls[3] = true;
                tiles[t.position + new Vector3Int(0, 0, 1)].walls[3] = true;
            }
        }


    }
    
    public void GenerateDoorway(Vector3Int pos, bool dirIsUpDown)
    {
        doorways.Add(new DoorwayObj() { pos = pos, UD = dirIsUpDown });
        // Generate a doorway object?
        GameObject go = Instantiate(doorwayTemp);
        go.transform.localRotation = (dirIsUpDown ? new Quaternion() : Quaternion.Euler(0,90,0));
        // Should be the right scale o-o
        Vector3 vv = new Vector3(0,0,0) + pos;
        vv.Scale(GetFloorScaling());
        go.transform.localPosition = vv;
        go.SetActive(true);
    }

    [HideInInspector] public List<PropScript> puzzles;
    [HideInInspector] public List<PropScript> simpleProps;
    private PropScript nextProp = null;
    public List<PropScript> allProps;
    public int PropTrials = 10; // will simply randomly pick one this many times, if it fails beyond that it returns a failure.
    // Ush

    private void SpawnProp(Prop p)
    {
        GameObject ggo = Instantiate(nextProp.gameObject);
        ggo.transform.localPosition = p.anchorPos + nextProp.AnchorPoint + nextProp.GetFinalAnchor();
        // Technically do more...
        ggo.SetActive(true);
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
            PickNextProp();
        }
        return false;
    }

    public void PickNextProp()
    {
        nextProp = simpleProps[Random.Range(0,simpleProps.Count)];
    }

    public void GenerateFloor(Vector3Int pos)
    {
        GameObject go = Instantiate(x1);
        go.transform.localScale = new Vector3(floorSquareSize / 10.0f, 0.1f, floorSquareSize / 10.0f);
        go.transform.position = new Vector3(floorSquareSize*pos.x, pos.y * 5, floorSquareSize*pos.z);
        go.SetActive(true);
        go.name = "Floor (" + pos.x + ", "+ pos.y + ", " + pos.z+")";
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
        AddRG(new Doorway(), 15);
        nextProp = allProps[0];
        simpleProps.AddRange(allProps);
        StartCoroutine(StartB());
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
                kernel[0][0] = kernel[0][2] = kernel[2][0] = kernel[2][2] = -1;
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
        return "Tile: " + position + " (L:" + walls[0] + ", R:" + walls[1]+ ", U:" + walls[2]+ ", D:" + walls[3]+").";
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
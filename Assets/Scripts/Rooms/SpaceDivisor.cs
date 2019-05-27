using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class SpaceDivisor
{
    public abstract SpaceResult Generate(int param, Tile tiq, RoomCreator c);

    public static SpaceResult GenerateMap(int[][] kernel, RoomCreator c, Vector3Int pos)
    {
        /*
         * 0: Must be empty (or a fake tile!)
         * 1: Must be filled.
         * 2: Anchor point for the kernel (it acts like a filled tile! So don't use your true anchor point (you must do 1 off!)
         * -1: Can be a wall or empty. Not enforced. Actually ignored.
         * 3: leave all walls off unless connected via a normal tile without a wall facing this position (may not be occupied!)
         */
        // Find the 2...
        // ABSOLUTELY ASSUMES THAT IT PASSED CANGENERATE!!!!
        Vector2Int vp = new Vector2Int(-1, -1);
        SpaceResult sr = new SpaceResult();
        for (int x = 0; x < kernel.Length; x++)
        {
            for (int y = 0; y < kernel[x].Length; y++)
            {
                if (kernel[x][y] == 2 && vp.x != -1)
                {
                    throw new System.Exception("Found more than one anchor position! You are required to run canGenerate before running this function!");
                }
                else if (vp.x == -1 && kernel[x][y] == 2)
                {
                    vp = new Vector2Int(x, y);
                }
            }
        }
        Vector3Int off = new Vector3Int(-vp.x, 0, -vp.y);
        for(int x = 0; x < kernel.Length; x++)
        {
            for(int y = 0; y < kernel[0].Length; y++)
            {
                pos += new Vector3Int(x, 0, y);
                switch (kernel[x][y])
                {
                    case -1:
                        break;
                    case 0:
                        c.tiles[pos + off] = new Tile(pos + off); // make it a fake one!
                        c.tiles[pos + off].fake = true;
                        break;
                    
                    case 1:
                    case 2:
                    case 3:
                        c.tiles[pos + off] = new Tile(pos + off);
                        c.tiles[pos + off].fake = false;
                        List<Vector3Int> wp = c.tiles[pos + off].getWallsOpen();
                        bool[] bb = new bool[4];
                        // Left
                        // T_T need to modify for fake tiles :|
                        bb[0] = (!(((x == 0 || kernel[x - 1][y] == -1) && c.tiles[pos + off + new Vector3Int(-1, 0, 0)] != null
                            && !c.tiles[pos + off + new Vector3Int(-1, 0, 0)].fake && !c.tiles[pos + off + new Vector3Int(-1, 0, 0)].walls[c.tiles[pos + off +
                            new Vector3Int(-1, 0, 0)].getWallFromDir(pos + off)]) || (x != 0 && kernel[x - 1][y] >= 1)));
                        bb[1] = (!(((x == kernel.Length-1 || kernel[x + 1][y] == -1) && c.tiles[pos + off + new Vector3Int(1, 0, 0)] != null
                            && !c.tiles[pos + off + new Vector3Int(1, 0, 0)].fake && !c.tiles[pos + off + new Vector3Int(1, 0, 0)].walls[c.tiles[pos + off +
                            new Vector3Int(1, 0, 0)].getWallFromDir(pos + off)]) || (x != kernel.Length-1 && kernel[x + 1][y] >=1)));

                        bb[2] = (!(((y == kernel[0].Length - 1 || kernel[x][y+1] == -1) && c.tiles[pos + off + new Vector3Int(0, 0, 1)] != null
                            && !c.tiles[pos + off + new Vector3Int(0, 0, 1)].fake && !c.tiles[pos + off + new Vector3Int(0, 0, 1)].walls[c.tiles[pos + off +
                            new Vector3Int(0, 0, 1)].getWallFromDir(pos + off)]) || (y != kernel[0].Length - 1 && kernel[x][y+1] >=1)));
                        bb[3] = (!(((y == 0 || kernel[x][y - 1] == -1) && c.tiles[pos + off + new Vector3Int(0, 0, -1)] != null
                            && !c.tiles[pos+off+new Vector3Int(0,0,-1)].fake && !c.tiles[pos + off + new Vector3Int(0, 0, -1)].walls[c.tiles[pos + off +
                            new Vector3Int(0, 0, -1)].getWallFromDir(pos + off)]) || (y != 0 && kernel[x][y - 1] >=1)));
                        // if 1 or 2 continue, otherwise break.
                        bool[] xx = new bool[4];
                        for(int i = 0; i < 4; i++)
                        {
                            xx[i] = !bb[i];
                        }
                        c.GenerateFloor(pos + off);
                        c.tiles[pos + off].walls = xx;
                        if (kernel[x][y] != 3)
                        {
                            c.GenerateWalls(c.tiles[pos + off], bb);
                        }
                        else
                        {
                            // Special case, we need to redo all the checks up above, and check to see if any are 0 (in which we KNOW that there is a wall...)
                            bb[0] = (x != 0 && kernel[x-1][y] == 0) || ((x == 0 || kernel[x - 1][y] == -1) && c.tiles[pos+off+new Vector3Int(-1,0,0)] != null &&
                                (c.tiles[pos+off+new Vector3Int(-1,0,0)].fake||c.tiles[pos+off+new Vector3Int(-1,0,0)].walls[c.tiles[
                                    pos + off + new Vector3Int(-1, 0, 0)].getWallFromDir(pos + off)]));
                            bb[1] = (x != kernel.Length - 1 && kernel[x + 1][y] == 0) || ((x == kernel.Length - 1 || kernel[x + 1][y] == -1) && c.tiles[pos + off + new Vector3Int(1, 0, 0)] != null &&
                                (c.tiles[pos + off + new Vector3Int(1, 0, 0)].fake || c.tiles[pos + off + new Vector3Int(1, 0, 0)].walls[c.tiles[
                                    pos + off + new Vector3Int(1, 0, 0)].getWallFromDir(pos + off)]));
                            bb[3] = (y != 0 && kernel[x][y-1] == 0) || ((y == 0 || kernel[x][y-1] == -1) && c.tiles[pos + off + new Vector3Int(0, 0, -1)] != null &&
                                (c.tiles[pos + off + new Vector3Int(0, 0, -1)].fake || c.tiles[pos + off + new Vector3Int(0, 0, -1)].walls[c.tiles[
                                    pos + off + new Vector3Int(0, 0, -1)].getWallFromDir(pos + off)]));
                            bb[2] = (y != kernel[0].Length-1 && kernel[x][y + 1] == 0) || ((y == kernel[0].Length-1 || kernel[x][y + 1] == -1) && c.tiles[pos + off + new Vector3Int(0, 0, 1)] != null &&
                                (c.tiles[pos + off + new Vector3Int(0, 0, 1)].fake || c.tiles[pos + off + new Vector3Int(0, 0, 1)].walls[c.tiles[
                                    pos + off + new Vector3Int(0, 0, 1)].getWallFromDir(pos + off)]));
                            Debug.Log(c.tiles[pos + off] + " " + x + " " + y + " " + PrintArray(bb));
                            // Basically if the bb is true, then there is a wall that needs to be placed...
                            c.GenerateWalls(c.tiles[pos + off], bb);
                        }
                        sr.toAddToMaster.Add(c.tiles[pos + off]);
                        if (kernel[x][y] == 3 && c.tiles[pos + off].wallsOcc() != 4)
                        {
                            c.tiles[pos + off].connectionPending = true; // Aka, do a final pass with these items to generate props
                            sr.toAddToQueue.Add(c.tiles[pos + off]);
                        }
                            
                        Debug.Log(c.tiles[pos + off] + " " + x + " " + y + " " + kernel[x][y]);
                        break;
                }
                pos -= new Vector3Int(x, 0, y);
            }
        }
        if (c.tiles[pos] != null)
        {
            if (c.tiles[pos].getWallsOpen(c).Count != 0)
            {
                sr.toAddToQueue.Add(c.tiles[pos]);
            }
        }
        sr.success = true;
        // Post fill, add 3's if not all walls added
        return sr;
    }
    public static bool IsConnected(int[][] kernel)
    {
        // Simply, it will test to see if all points connect to each other [will treat 1==2==3, and 0==-1]

        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
        Stack<Vector2Int> s = new Stack<Vector2Int>();
        for(int x = 0; x < kernel.Length; x++)
        {
            for(int y = 0; y < kernel[x].Length; y++)
            {
                if (kernel[x][y] <= 0)
                {
                    continue;
                }
                if (!seen.Contains(new Vector2Int(x, y)) && seen.Count != 0){
                    return false; // Found a disjoint point
                }
                if(seen.Contains(new Vector2Int(x, y)))
                {
                    continue;
                }
                
                // :D
                s.Push(new Vector2Int(x, y));
                while(s.Count != 0)
                {
                    Vector2Int si = s.Pop();
                    if (seen.Contains(si))
                    {
                        continue;
                    }
                    seen.Add(si);
                    if(si.x != 0 && kernel[si.x-1][si.y] >= 1)
                    {
                        s.Push(si + new Vector2Int(-1, 0));
                    }
                    if (si.y != 0 && kernel[si.x][si.y-1] >= 1)
                    {
                        s.Push(si + new Vector2Int(0, -1));
                    }
                    if (si.y != kernel[0].Length-1 && kernel[si.x][si.y+1] >= 1)
                    {
                        s.Push(si + new Vector2Int(0,1));
                    }
                    if (si.x != kernel.Length-1 && kernel[si.x + 1][si.y] >= 1)
                    {
                        s.Push(si + new Vector2Int(1, 0));
                    }
                }
            }
        }
        return true;
    }
    public static void PrintKernel(int[][] kernel)
    {
        StringBuilder sb = new StringBuilder();
        for(int y = kernel[0].Length-1; y >= 0; y--)
        {
            for(int x = 0; x < kernel.Length; x++)
            {
                sb.Append(kernel[x][y] + " ");
            }
            sb.Append("\n");
        }
        Debug.Log(sb);
    }
    public static bool CanGenerate(int[][] kernel, RoomCreator c, Vector3Int pos)
    {

        /*
         * 0: Must be empty (or a fake tile!)
         * 1: Must be filled.
         * 2: Anchor point for the kernel (it acts like a filled tile! So don't use your true anchor point (you must do 1 off!)
         * -1: Can be a wall or empty. Not enforced.
         * 3: leave all walls off unless connected via a normal tile without a wall facing this position (may not be occupied!)
         */
        // Find the 2...
        int fp = kernel[0].Length;
        for(int i = 0; i < kernel.Length; i++)
        {
            if(kernel[i].Length != fp)
            {
                Debug.LogWarning("You ONLY can use square kernels.");
                return false;
            }
        }

        Vector2Int vp = new Vector2Int(-1, -1);
        for(int x = 0; x < kernel.Length; x++)
        {
            for(int y = 0; y < kernel[x].Length; y++)
            {
                if(kernel[x][y] == 2 && vp.x != -1)
                {
                    Debug.LogError("CanGenerate found more than 1 anchor point! Please fix this!!");
                    return false;
                }
                else if(vp.x == -1 && kernel[x][y]==2)
                {
                    vp = new Vector2Int(x, y);
                }
            }
        }

        // See if we COULD place it.
        for(int x = 0; x < kernel.Length; x++)
        {
            for(int y = 0; y < kernel[x].Length; y++)
            {
                switch (kernel[x][y])
                {
                    case 1:
                    case 2:
                    case 3:
                    case 0:
                        //Debug.Log(x + " " + y + " " + (pos - new Vector3Int(vp.x, 0, vp.y) + new Vector3Int(x, 0, y)) + " " + c.tiles[pos - new Vector3Int(vp.x, 0, vp.y) + new Vector3Int(x, 0, y)]);
                        if((c.tiles[pos - new Vector3Int(vp.x, 0, vp.y) + new Vector3Int(x, 0, y)] == null || c.tiles[pos - new Vector3Int(vp.x, 0, vp.y) + new Vector3Int(x, 0, y)].fake))
                        {
                            // pass :|
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case -1:
                        // Ya good :P
                        break;
                }
            }
        }
        

        return true;
    }
    public static string PrintArray<T>(T[] b)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(typeof(T).FullName + " [");
        for(int i = 0; i < b.Length; i++)
        {
            sb.Append(b[i] + (i != b.Length-1 ? "," :""));
        }
        sb.Append("]");
        return sb.ToString();
    }

    public static Prop GeneratePropForTile(Tile t, RoomCreator c)
    {
        Vector3 fs = c.GetFloorScaling();
        float ff = Random.Range(0f, 1f);
        if (ff <= 1f)
        {
            Debug.Log("Wall PROP: " + t.position);
            Debug.Log("PROP: " + t.position);
            Prop p = new Prop();
            if (!t.realWalls[0] && !t.realWalls[1] && !t.realWalls[2] && !t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = new Vector3(0, 0, 0) + t.position;
            else if (t.realWalls[0] && !t.realWalls[1] && !t.realWalls[2] && !t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = new Vector3(-.5f, 0, 0) + t.position;
            else if (!t.realWalls[0] && t.realWalls[1] && !t.realWalls[2] && !t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = new Vector3(.5f, 0, 0) + t.position;
            else if (!t.realWalls[0] && !t.realWalls[1] && t.realWalls[2] && !t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = new Vector3(0, 0, .5f) + t.position;
            else if (!t.realWalls[0] && !t.realWalls[1] && !t.realWalls[2] && t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = new Vector3(0, 0, -.5f) + t.position;   
            else if (!t.realWalls[0] && !t.realWalls[1] && t.realWalls[2] && t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = new Vector3(0, 0, Random.Range(-1f, 1f) >= 0 ? .5f : -.5f) + t.position;
            else if (t.realWalls[0] && t.realWalls[1] && !t.realWalls[2] && !t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = new Vector3(Random.Range(-1f, 1f) >= 0 ? .5f : -.5f, 0, 0) + t.position;
            else if (t.realWalls[0] && !t.realWalls[1] && t.realWalls[2] && !t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = (Random.Range(-1f, 1f) >= 0 ? new Vector3(-.5f,0,0) : new Vector3(0,0,.5f)) + t.position;
            else if (t.realWalls[0] && !t.realWalls[1] && !t.realWalls[2] && t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = (Random.Range(-1f, 1f) >= 0 ? new Vector3(-.5f, 0, 0) : new Vector3(0, 0, -.5f)) + t.position;
            else if (!t.realWalls[0] && t.realWalls[1] && t.realWalls[2] && !t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = (Random.Range(-1f, 1f) >= 0 ? new Vector3(.5f, 0, 0) : new Vector3(0, 0, .5f)) + t.position;
            else if (!t.realWalls[0] && t.realWalls[1] && !t.realWalls[2] && t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = (Random.Range(-1f, 1f) >= 0 ? new Vector3(.5f, 0, 0) : new Vector3(0, 0, -.5f)) + t.position;
            else if (t.realWalls[0] && !t.realWalls[1] && t.realWalls[2] && t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = (Random.Range(0, 1f) <= .33f ? new Vector3(-.5f, 0, 0) : (Random.Range(0,1f) >= .5f ?
                    new Vector3(0, 0, .5f) : new Vector3(0,0,-.5f))) + t.position;
            else if (!t.realWalls[0] && t.realWalls[1] && t.realWalls[2] && t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = (Random.Range(0, 1f) <= .33f ? new Vector3(.5f, 0, 0) : (Random.Range(0, 1f) >= .5f ?
                    new Vector3(0, 0, .5f) : new Vector3(0, 0, -.5f))) + t.position;
            else if (t.realWalls[0] && t.realWalls[1] && !t.realWalls[2] && t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = (Random.Range(0, 1f) <= .33f ? new Vector3(-.5f, 0, 0) : (Random.Range(0, 1f) >= .5f ?
                    new Vector3(.5f, 0, 0) : new Vector3(0, 0, -.5f))) + t.position;
            else if (t.realWalls[0] && t.realWalls[1] && t.realWalls[2] && !t.realWalls[3] && t.wallsOcc() == 4)
                p.anchorPos = (Random.Range(0, 1f) <= .33f ? new Vector3(-.5f, 0, 0) : (Random.Range(0, 1f) >= .5f ?
                    new Vector3(.5f, 0, 0) : new Vector3(0, 0, .5f))) + t.position;

            Vector3 temp = p.anchorPos + new Vector3();
            p.anchorPos.Scale(fs);
            p.maxSize = new Vector3(2f, 2f, 2f);
            p.spawnTemp(c.tempObject, new Vector3(0, -.5f, 0) + temp - t.position, true);
            return p;
        }
        return null;
    }

}
public class Prop
{
    public Vector3 maxSize;
    public Vector3 anchorPos; // This is the game position XD

    public Quaternion rotations = Quaternion.identity;
    public void spawnTemp(GameObject temp, Vector3 oap, bool useScale)
    {
        // Spawns temp at the location provided...
        // rotated how it should...
        GameObject bb = GameObject.Instantiate(temp);
        bb.transform.localRotation = rotations;
        if (useScale)
            bb.transform.localScale = maxSize;

        bb.transform.localPosition = Vector3.zero; // We assume the parent is the world...but still 
        // Need to transform it so that oap == anchorPos
        // If oap is (0,0,0), then the center is the anchor point...
        // but we rotated and scaled the object T_T
        bb.transform.localPosition -= bb.transform.TransformPoint(oap);
        bb.transform.localPosition += anchorPos;
        bb.SetActive(true);
    }
}
public class SpaceResult
{
    public List<Tile> toAddToMaster = new List<Tile>();
    public List<Tile> toAddToQueue = new List<Tile>();
    public List<Prop> toAddToProps = new List<Prop>();
    public bool success = false;
}

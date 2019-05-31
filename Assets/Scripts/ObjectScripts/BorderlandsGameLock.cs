using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderlandsGameLock : MonoBehaviour
{
    public Interactable[] toggleables; // Expects bool updaters...
    public GameLock gameLock; 
    public GameLock[] totoggle; // Must be of toggable variant...
    public bool[][] b;
    public bool finished;
    public int SizeX;
    public int SizeY;
    public float prob = 0f;
    public bool LightPuzzle; // If true, toggle neighbors, if false, don't
    public IEnumerator SendDelayed(float f, int i)
    {
        yield return new WaitForSeconds(f);
        toggleables[i].SendUpdate("1");
    }
    public void Release() // Aka, allowed to generate....
    {

        // First determine the size and if it works.
        if(SizeX * SizeY!= toggleables.Length || toggleables.Length != totoggle.Length)
        {
            throw new System.Exception("SizeX *SizeY, toggleables, toToggle do not match!");
        }
        b = new bool[SizeX][];
        int num = 0;
        for(int i = 0; i < SizeX; i++)
        {
            b[i] = new bool[SizeY];
            for(int j = 0; j < SizeY; j++)
            {
                b[i][j] = Random.Range(0,1f) <= prob / (num + 1.0f);
            }
        }
        // Register to each of their interacts.
        for(int i = 0; i < toggleables.Length; i++)
        {
            int k = i; // Ensures that k is the correct local value when creating below.
            toggleables[i].interactEvent += (c => Interacted(c, k));
            toggleables[i].updateEvent += (c=>UpdatedEvent(c, k));
            toggleables[i].gameInteractComplete += GameFin;
            if(b[i % SizeX][(i-(i % SizeX)) / SizeX])
            {
                StartCoroutine(SendDelayed(1, i));
            }
        }
    }

    public void GameFin()
    {
        if (finished)
            return;
        finished = true;
        gameLock.GFinished(RoomManager.instance.Player.cam);
        foreach (GameLock p in totoggle)
        {
            p.GSetState(RoomManager.instance.Player.cam, true);
        }
        
    }

    public void UpdatedEvent(string state, int pos)
    {
        // Forcably go through and update them :P
        
        totoggle[pos].GToggleState(RoomManager.instance.Player.cam);
        
    }

    public bool Interacted(CameraController cc, int pos)
    {
        Debug.Log("Interact with me :D" + pos);
        if (finished)
            return false;

        // Toggle this light...
        int X = pos % SizeX;
        int Y = (pos - X) / SizeX;
        // You update those around the button 
        /*
         * X X X        X X O
         * X X P(x) --> X O O
         * X X X        X X O
         */
        List<Vector2Int> ptt = new List<Vector2Int>();
        if (LightPuzzle)
        {
            ptt.Add(new Vector2Int(X - 1, Y));
            ptt.Add(new Vector2Int(X + 1, Y));
            ptt.Add(new Vector2Int(X, Y + 1));
            ptt.Add(new Vector2Int(X, Y - 1));
        }
        // Finally flip me :D
        ptt.Add(new Vector2Int(X, Y));

        foreach(Vector2Int vi in ptt)
        {
            if (vi.x < 0 || vi.y < 0 || vi.x >= SizeX || vi.y >= SizeY)
                continue;
            // Toggle
            Debug.Log("RUNNING ON " + vi);
            b[vi.x][vi.y] = !b[vi.x][vi.y];
            toggleables[vi.x + (vi.y * SizeX)].SendUpdate("" + (b[vi.x][vi.y] ? "1" : "0"));
        }
        // Could wait...or could do it now
        foreach(bool[] bg in b)
        {
            foreach(bool bj in bg)
            {
                if (!bj)
                {
                    return false;
                }
            }
        }
        // Done :D
        // We should get a SF back....
        foreach(Interactable c in toggleables)
        {
            c.SendSF();
        }

        // Final check see if everything toggled
        return false;
    }
    bool hasUpdated = false;
    public void Update()
    {
        if (hasUpdated)
            return;
        hasUpdated = true;
        Release();

    }
}
[System.Serializable]
public class SecretArray<T>
{
    public T[] elements;
}
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
public class NetBookcase : PuzzleObj
{

    public GameObject[] bookInstances;
    public Transform bookParent;
    public bool hasRan = false;

    public override void GenerateAsProp(long propSeed)
    {
        // First generate a number of books to place on each shelf..
        Random.InitState((int)propSeed);
        int shelf1 = Random.Range(0, 15);
        int shelf2 = Random.Range(0, 15);
        int shelf3 = Random.Range(shelf1, 16);
        StringBuilder temp = new StringBuilder();
        temp.Append(propSeed + ";0;" + shelf1 + "," + shelf2 + "," + shelf3);

        for(int i = 0; i < shelf1 + shelf2 + shelf3; i++)
        {
            temp.Append("," + Random.Range(0, bookInstances.Length));
        }

        state = temp.ToString();
        // Debug.Log(state + " " + this.netId);
        RpcGeneratePropWithState(state);
    }

    [ClientRpc]
    public override void RpcGeneratePropWithState(string state)
    {
        //Debug.Log(state);
        char[] ss = state.ToCharArray();
        int pos = 0;
        StringBuilder sbb = new StringBuilder();
        long pps = 0;
        while(ss[pos] != ';')
        {
            sbb.Append(ss[pos++]);
        }
        pos++;
        pps = long.Parse(sbb.ToString());
        Random.InitState((int)pps);
        sbb.Clear();
        switch (ss[pos++])
        {
            // No hidden state
            case '0':
                break;
            case '1':

                break;
        }
        pos++;
        int a = 0;
        int b = 0;
        int c = 0;
        
        while (ss[pos] != ',')
        {
            sbb.Append(ss[pos++]);
        }
        a = int.Parse(sbb.ToString());
        pos++;
        sbb.Clear();
        while (ss[pos] != ',')
        {
            sbb.Append(ss[pos++]);
        }
        b = int.Parse(sbb.ToString());
        pos++;
        sbb.Clear();
        while (ss[pos] != ',')
        {
            if(ss[pos] == '0' && sbb.Length == 0)
            {
                sbb.Append(ss[pos++]);
                break;
            }
            sbb.Append(ss[pos++]);
        }
        c = int.Parse(sbb.ToString());
        
        for (int x = 0; x < a + b + c; x++) { 
            pos++;
            // Generate stuff :D
            int ccp = (x < a ? a : (x - a < b ? b : c));
            int de = (x < a ? x : (x - a < b ? x-a : (x-b)-a));
            int ypos = (x < a ? 2 : (x - a < b ? 1 : 0));
            sbb.Clear();
            while (pos < ss.Length && ss[pos] != ',')
            {
                sbb.Append(ss[pos++]);
            }
            int bs = int.Parse(sbb.ToString());
            GameObject go = Instantiate(bookInstances[bs]);
            go.transform.parent = bookParent;
            // Offset of (ccp % 9)*1.5
            go.transform.localPosition = new Vector3((ccp% 9) * .2f + (de + Random.Range(0f,0.15f)) * .25f - .864f, ypos, -.5f);
            go.transform.localRotation = Quaternion.Euler(0,180,0);
            go.SetActive(true);
        }
        hasRan = true;

    }
    public override void GenerateAsPuzzle(long puzzleSeed)
    {
        throw new System.NotImplementedException();
    }

    public override void GenerateAsProp(PuzzleObj po, PuzzleType pt)
    {
        throw new System.NotImplementedException();
    }
}

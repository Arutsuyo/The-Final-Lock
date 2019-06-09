using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintForcer : MonoBehaviour
{
    // Start is called before the first frame update
    public List<string> hints;
    public List<int> pos;
    void Start()
    {
        foreach (string s in hints)
        {
            cc.hints.Add(s);
            cc.hintPos.Add(s, cc.hints.Count - 1);
            pos.Add(cc.hints.Count - 1);
            cc.hintRemoved.Add(false);
            cc.decodedPercent.Add(cc.hints.Count - 1, 0);
            cc.availableHints.Add(cc.hints.Count - 1);
        }
        cc.RecalculateHints();
    }
    public bool removeOne = false;
    void Update()
    {

        if (removeOne)
        {
            removeOne = false;
            if (pos.Count != 0)
            {
                cc.hintRemoved[pos[0]] = true;
                cc.availableHints.RemoveAt(0);
                pos.RemoveAt(0);
                cc.RecalculateHints();
            }
        }

    }
    public CameraController cc;
}

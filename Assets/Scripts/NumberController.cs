using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberController : MonoBehaviour
{
    public GameObject TOP;
    public GameObject BOT;
    public GameObject TL;
    public GameObject BL;
    public GameObject MID;
    public GameObject TR;
    public GameObject BR;

    public int NUM = 0;

    public bool Debug = false;

    public void ChangeUpdate()
    {
        if (NUM == -1)
        {
            TOP.SetActive(false);
            BOT.SetActive(false);
            TL.SetActive(false);
            TR.SetActive(false);
            MID.SetActive(false);
            BR.SetActive(false);
            BL.SetActive(false);
            return;
        }
        TOP.SetActive(NUM != 1 && NUM != 4);
        BOT.SetActive(NUM != 7 && NUM != 4 && NUM != 1);
        MID.SetActive(NUM != 1 && NUM != 0 && NUM != 7);
        TL.SetActive(NUM == 4 || NUM == 5 || NUM == 6 || NUM >= 8 || NUM == 0);
        BL.SetActive(NUM == 2 || NUM == 6 || NUM == 8 || NUM == 0);
        TR.SetActive(NUM != 5 && NUM != 6);
        BR.SetActive(NUM != 2);
    }
    public void Update()
    {
        if (Debug)
        {
            Debug = false;
            // Fire :D
            ChangeUpdate();
        }
    }
}

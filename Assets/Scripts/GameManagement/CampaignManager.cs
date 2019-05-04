using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

using System.Xml;
using System;

public class CampaignManager : MonoBehaviour
{
    [Header("Object References")]
    public Button BackBtn;
    public Button StartBtn;
    public ScrollRect CampaignRect;
    public RectTransform CampaignsContent;
    public Button CampaignClone;
    public Button Selected = null;
    public GameObject[] holoDeck;
    public string[] holoDeckNames;
    public GameObject holoStorage;
    public new Animator animation;  //New keyword fixes warning CS0108

    [Header("Rooms Offered")]
    public string[] Rooms;

    private string RoomSearchPath = @"Rooms/";

    [Header("Debugging")]
    public EscapeRoom[] erooms;
    public Button[] Campaigns; // Created at run time.
    public GameObject holo;

    public void UpdateCampaignUI(int id)
    {
        //Debug.Log("Clicked campaign " + id);
        if(holo != null)
        {
            Destroy(holo);
        }
        holo = Instantiate(holoDeck[Array.FindIndex(holoDeckNames, w=> w.Equals(erooms[id].holoname))],holoStorage.transform);
        Debug.Log(erooms[id].holoname + " " + Array.FindIndex(holoDeckNames, w=>w.Equals(erooms[id].holoname)));
        StartBtn.gameObject.SetActive(true);
        StartBtn.interactable = true;
        StartBtn.onClick.RemoveAllListeners();
        StartBtn.onClick.AddListener(() => Go(id));
    }    
    public void Go(int id)
    {
        Debug.Log("Launching campaign ID " + id + " in 3...2....1...JUMP");
        animation.SetTrigger("GoToRoom");

    }
    public void Starting()
    {
        BackBtn.interactable = false;
        StartBtn.interactable = false;
    }
    public void HideUI()
    {
        BackBtn.interactable = false;
        StartBtn.interactable = false;
        CampaignRect.gameObject.SetActive(false);
        BackBtn.gameObject.SetActive(false);
        StartBtn.gameObject.SetActive(false);
        Selected = null;
        foreach (Button b in Campaigns)
        {
            Destroy(b.gameObject);
        }
        if(holoStorage != null)
        {
            Destroy(holo);
        }
    }
    public void ShowUI()
    {
        BackBtn.interactable = true;
        Selected = null;
        QueryRooms();
        List<Button> bb = new List<Button>();
        int count = 0;
        foreach(EscapeRoom ee in erooms)
        {
            RectTransform gg = Instantiate<RectTransform>(CampaignClone.GetComponent<RectTransform>(), CampaignsContent.transform);
            gg.localPosition += new Vector3(0, -count * gg.sizeDelta.y, 0);
            bb.Add(gg.GetComponent<Button>());
            gg.GetChild(0).GetComponent<Text>().text = ee.name;
            Debug.Log(count);
            int copy = count + 0;
            gg.GetComponent<Button>().onClick.AddListener(delegate { UpdateCampaignUI(copy); });
            count++;
        }
        Campaigns = bb.ToArray<Button>();
        BackBtn.gameObject.SetActive(true);
        CampaignRect.gameObject.SetActive(true);
        Selected = null;
        Debug.Log("Show UI!");
    }

    public void Start()
    {
        HideUI();
    }
    private EscapeRoom ParseFile(XmlDocument x)
    {
        foreach(XmlElement node in x)
        {
            if (node.Name.ToLower().Equals("room"))
            {
                return new EscapeRoom(node);
            }
            else
            {
                Debug.LogError("Unexpected node found in base of XML tree: " + node.Name);
            }
        }
        Debug.LogError("File did not contain an escape room!");
        return null;
    }

    public void QueryRooms()
    {
        TextAsset[] files = Resources.LoadAll<TextAsset>(RoomSearchPath);
        if (Directory.Exists(RoomSearchPath))
        {
            List<EscapeRoom> eerooms = new List<EscapeRoom>();
            foreach(string s in Directory.EnumerateFiles(RoomSearchPath))
            {
                if (s.EndsWith(".room"))
                {
                    XmlDocument xx = new XmlDocument();
                    xx.Load(s);
                    try
                    {
                        EscapeRoom e = ParseFile(xx);

                        if (e != null)
                        {
                            eerooms.Add(e);
                            continue;
                        }
                    }
                    catch (System.Exception)
                    {
                        Debug.LogError("Malformed escape room file! Ignoring.");
                    }
                    
                }
                else
                {
                    // ignore :D
                    Debug.Log("Ignoring file " + s);
                }
            }
            erooms = eerooms.ToArray<EscapeRoom>();
        }
        else
        {
            if (Directory.CreateDirectory(RoomSearchPath).Exists)
            {
                Debug.LogWarning("Rooms file was missing. No available campaigns!");
            }
            else
            {
                Debug.LogError("Could not create rooms file!");
            }

        }
        
    }
}
[System.Serializable]
public class EscapeRoom
{
    public string name;
    public string holoname;
    public int numRooms;
    public int[] sizeofRooms;
    public bool isRandom;
    
    public Puzzle mainPuzzle;

    public EscapeRoom(XmlElement xe)
    {
        foreach(XmlElement ee in xe)
        {
            switch (ee.Name.ToLower())
            {
                case "name":
                    name = ee.InnerText;
                    break;
                case "holoname":
                    holoname = ee.InnerText;
                    break;
                case "numrooms":
                    numRooms = int.Parse(ee.InnerText);
                    break;
                case "userandomgeneration":
                    isRandom = int.Parse(ee.InnerText) == 1;
                    break;
                case "sizeofrooms":
                    DetermineRooms(ee);
                    break;
                case "puzzle":
                    DeterminePuzzles(ee);
                    break;
                default:
                    Debug.LogWarning("Unknown tag: " + ee.Name);
                    break;
            }
        }
    }
    private void DetermineRooms(XmlElement e)
    {
        List<int> sizes = new List<int>();
        foreach (XmlElement es in e)
        {
            if (es.Name.ToLower().Equals("roomsize"))
            {
                sizes.Add(int.Parse(es.InnerText));
            }
        }
        sizeofRooms = sizes.ToArray<int>();
    }

    private void DeterminePuzzles(XmlElement e)
    {
        mainPuzzle = new Puzzle(e);
    }
}
public class Puzzle
{
    public int mechanismID;
    public Key[] keys;
    public Puzzle[] puzzles; // note, puzzles are topologically ordered.
    // That is, parents may depend on children, but children CAN NOT
    // depend on parents...
    public Clue[] clues;
    public Puzzle(XmlElement e) { 
        List<Puzzle> puz = new List<Puzzle>();
        List<Clue> clues1 = new List<Clue>();
        foreach (XmlElement eel in e)
        {
            switch (eel.Name.ToLower())
            {
                case "mechanism":
                    List<Key> keys1 = new List<Key>();
                    foreach(XmlElement ej in eel)
                    {
                        switch (ej.Name.ToLower())
                        {
                            case "id":
                                mechanismID = int.Parse(ej.InnerText);
                                break;
                            case "keys":
                                foreach (XmlElement ek in ej)
                                {
                                    if (ek.Name.ToLower().Equals("key"))
                                    {
                                        keys1.Add(new Key(ek));
                                    }
                                    else
                                    {
                                        Debug.Log("Unknown tag in keys tag: " + ek.Name);
                                    }
                                }
                                break;
                            default:
                                Debug.LogError("Unknown key in mechanism tag: " + ej.Name);
                                break;
                        }
                    }
                    keys = keys1.ToArray<Key>();

                    break;
                case "clues":
                    foreach(XmlElement ee in eel)
                    {
                        switch (ee.Name.ToLower())
                        {
                            case "clue":
                                clues1.Add(new Clue(ee));
                                break;
                            case "puzzle":
                                puz.Add(new Puzzle(ee));
                                break;
                            default:
                                Debug.LogError("Unknown key in clues tag: " + ee.Name);
                                break;
                        }
                    }
                    break;
                default:
                    Debug.LogError("Unknown key in puzzle tag: " + eel.Name);
                    break;
            }
        }

        puzzles = puz.ToArray<Puzzle>();
        clues = clues1.ToArray<Clue>();
    }

}
public class Key
{
    public bool isEnviron;
    public int ID;
    public int variant;
    public Key(XmlElement e)
    {
        foreach(XmlElement ej in e)
        {
            switch (ej.Name.ToLower())
            {
                case "type":
                    isEnviron = int.Parse(ej.InnerText) == 1;
                    break;
                case "id":
                    ID = int.Parse(ej.InnerText);
                    break;
                case "variant":
                    variant = int.Parse(ej.InnerText);
                    break;
                default:
                    Debug.LogError("Unknown tag in key: " + ej.Name);
                    break;
            }
        }
    }
}

public class Clue
{
    // Uh....
    public Clue(XmlElement e)
    {
        // :D
        Debug.LogWarning("Clues have yet to be implemented!");
    }
}
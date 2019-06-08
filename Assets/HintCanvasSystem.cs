using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
public class HintCanvasSystem : MonoBehaviour
{
    public AnimationCurve anim;
    public GameObject toMove;
    public Text rec;
    public Transform pos0;
    public Transform pos1;


    public float position = 0;
    public float speed = 0.75f;
    public int direction1 = -1;

    public Text hintText;
    public GameObject back;
    public GameObject front;
    public Transform backTr;
    public Transform frontTr;
    public Transform leftWaypoint;
    public Transform topWaypoint;
    public CameraController cc;

    public Text[] toHide;
    public Text percent;
    public float swapSpeed = 1.25f;

    private bool isUpdating = false;
    public int index = 0;
    public void SwapTheCards(int dir)
    {
        if (isUpdating)
        {
            return;
        }
        isUpdating = true;
        if (stored != null) { StopCoroutine(stored); }
        StartCoroutine(SwapCards());
        if (cc.availableHints.Count == 0)
        {
            index = -1;
            DisplayCardText();
            return;

            // Don't do anything! D:
        }
        
        index = (index + dir + cc.availableHints.Count) % cc.availableHints.Count;
        
        
    }
    public IEnumerator SwapCards()
    {
        isUpdating = true;
        hintText.text = "";
        foreach(Text tt in toHide)
        {
            tt.enabled = false;
        }
        float PP = 1;
        while(PP>= -1){

            PP -= Time.deltaTime * swapSpeed;
            if (PP <= 0)
            {
                back.transform.position = Vector3.Lerp(leftWaypoint.position, backTr.position, PP * PP); // Up and down
                front.transform.position = Vector3.Lerp(topWaypoint.position, frontTr.position, PP * PP); // Left and right
            }
            else
            {
                back.transform.position = Vector3.Lerp(topWaypoint.position, backTr.position, PP * PP); // Up and down
                front.transform.position = Vector3.Lerp(leftWaypoint.position, frontTr.position, PP * PP); // Left and right
            }
            yield return null;
        }
        front.transform.position = frontTr.position;
        back.transform.position = backTr.position;
        foreach (Text tt in toHide)
        {
            tt.enabled = true;
        }
        DisplayCardText();
        isUpdating = false;
    }
    public Coroutine stored;
    public List<char> CHARS = new List<char>();
    public Dictionary<char, int> CTLP = new Dictionary<char, int>();
    public int MAXCHARS = 0;
    public void Start()
    {
        int CP = 0;
        for(int i = 33; i <= 126; i++)
        {
            CHARS.Add((char)i);
            CTLP.Add((char)i,CP++);
        }
        for(int i = 161; i <= 172; i++)
        {
            CHARS.Add((char)i);
            CTLP.Add((char)i, CP++);
        }
        for (int i = 174; i <= 272; i++)
        {
            CHARS.Add((char)i);
            CTLP.Add((char)i, CP++);
        }
        MAXCHARS = CHARS.Count;
    }
    public IEnumerator DecodeCard(string decode, int sp)
    {
        // Basically make the text corrupted :D
        // OHHH
        if(decode.Length == 0)
        {
            yield break;
        }
        if(sp == -1)
        {
            percent.text = "";
            hintText.text = decode;
            yield break;
        }
        int div =decode.Length - 1;
        StringBuilder TODISPLAY = new StringBuilder(decode);

        
        while (true)
        {
            yield return new WaitForSeconds(1f/15f);
            float targ = 0;
            for(int i = 0; i < div+1; i++)
            {
                targ = Mathf.Min(1, Mathf.Max(0,((.8f * i / ((float)div))+.2f - cc.decodedPercent[cc.availableHints[index]]) / .5f));
                TODISPLAY[i] = decode[i] == ' ' ? ' ' : CHARS[(MAXCHARS+CTLP[decode[i]] + Mathf.RoundToInt(targ*Random.Range(-MAXCHARS, MAXCHARS)))%MAXCHARS];
                
            }
            hintText.text = TODISPLAY.ToString();
            percent.text = "Decode Message (M) [" + (Mathf.RoundToInt(100f * cc.decodedPercent[cc.availableHints[index]])) + "%]";
        }

        //yield return null;
    }
    public void DisplayCardText()
    {
        string TS = "Scanning for new hints...";
        if(index != -1)
        {
            TS = cc.hints[cc.availableHints[index]];
        }
        if(stored != null)
        {
            StopCoroutine(stored);
        }
        stored = StartCoroutine(DecodeCard(TS, index));

    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            direction1 = -1*direction1;
            rec.text = (direction1 ==1? "Close":"Stuck") + "? (H)";
        }
        if (Input.GetKeyDown(KeyCode.I) && direction1 == 1)
        {
            SwapTheCards(-1);
        }
        if (Input.GetKeyDown(KeyCode.P) && direction1 == 1)
        {
            SwapTheCards(1);
        }
        if (Input.GetKey(KeyCode.M) && direction1 == 1 && index != -1)
        {
            cc.decodedPercent[cc.availableHints[index]] = Mathf.Min(1, cc.decodedPercent[cc.availableHints[index]] + Time.deltaTime * 0.1f);
        }
        position = Mathf.Max(0, Mathf.Min(1, position + speed * direction1*Time.deltaTime));
        toMove.transform.position = Vector3.Lerp(pos0.transform.position, pos1.transform.position, anim.Evaluate(position));

    }
}

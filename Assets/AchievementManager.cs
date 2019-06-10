using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AchievementManager : MonoBehaviour
{
    public Text title;
    public Text subscript;

    public GameObject panel;
    public GameObject from;
    public GameObject to;
    public float times = 1.5f;
    public bool overri = true;
    public void AwardAchievement(int id, string ti, string sub)
    {
        Debug.Log("Attempting achievment.");
        if (!PlayerPrefs.HasKey("ACH:" + id) || !PlayerPrefs.GetString("ACH:" + id).Equals("1") || overri)
        {
            Debug.Log("Achieve get.");
            title.text = ti;
            subscript.text = sub;
            PlayerPrefs.SetString("ACH:" + id, "1");
            PlayerPrefs.Save();
            StartCoroutine(MoveUpDown());
        }
    }
    public bool toggleAward = false;
    
    private IEnumerator MoveUpDown()
    {
        float Stime = Time.time;
        while(Time.time - Stime <= times)
        {
            panel.transform.position = Vector3.Lerp(to.transform.position, from.transform.position,
                Mathf.Pow((1-((Time.time - Stime)/times)), 4));
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        Stime = Time.time;
        while(Time.time - Stime <= times)
        {
            panel.transform.position = Vector3.Lerp(to.transform.position, from.transform.position,
                Mathf.Pow((((Time.time - Stime) / times)), 4));
            yield return null;
        }
        yield return null;
    }
}

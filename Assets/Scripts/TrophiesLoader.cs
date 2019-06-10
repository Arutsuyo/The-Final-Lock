using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TrophiesLoader : MonoBehaviour
{

    public Color grey = Color.grey;
    public Color white = Color.white;

    public Image[] trophies;
    public void ResetTrophies()
    {
        for(int i = 0; i < 12; i++)
        {
            PlayerPrefs.DeleteKey("ACH:" + i);
        }
        PlayerPrefs.Save();
        EDT();
    }

    public void EDT()
    {
        for(int i = 0; i < 12; i++)
        {
            trophies[i].color = (PlayerPrefs.HasKey("ACH:" + i) && PlayerPrefs.GetString("ACH:" + i).Equals("1") ? white : grey);
        }
    }
}

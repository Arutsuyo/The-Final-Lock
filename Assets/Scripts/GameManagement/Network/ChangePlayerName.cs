using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChangePlayerName : MonoBehaviour
{
	public string Username = "";
	public Text tx;
	public Color targColor;
	public Color prevColor;
	public float lastUpdateTime = 0;
	public float timeTaken = 2f;
	public PromptManager pm;
	public void Start()
	{
		if (!PlayerPrefs.HasKey("PlayerName"))
		{
			PlayerPrefs.SetString("PlayerName", "Player1" + Random.Range(0, 10000));
			PlayerPrefs.Save();
		}

		Username = PlayerPrefs.GetString("PlayerName");
		prevColor = new Color(0, 0, 0);
		targColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
	}

	public void Update()
	{
		if (Time.time - lastUpdateTime >= timeTaken)
		{
			prevColor = targColor;
			targColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
			lastUpdateTime = Time.time;
		}
		else
		{
			Color cc = Color.Lerp(prevColor, targColor, (Time.time - lastUpdateTime) / timeTaken);
			tx.text = "Player: <color=\"#" + ColorUtility.ToHtmlStringRGB(cc) + "\">" + Username + "</color>";
		}
	}
	public void UpdateName(System.Object o)
	{
		Debug.Log("UpdateName!!" + o);
		if (o != null)
		{
			Username = (string)o;
			PlayerPrefs.SetString("PlayerName", Username);
			PlayerPrefs.Save();
		}
	}

	public void ChangeName()
	{
		pm.GetString(UpdateName, 1, 20, "Enter your player name:", Username, "Change name");
	}
}

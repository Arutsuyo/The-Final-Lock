using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableButton : MonoBehaviour
{
	public Button btn;
	// Start is called before the first frame update
	void Start()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		Text txt = GetComponentInChildren<Text>();
		btn.interactable = false;
		txt.text += "\nNot Available to Web Clients";	
#endif
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stereo : MonoBehaviour
{
	public AudioSource au;
	public bool Play = false;
	// Start is called before the first frame update
	void Start()
	{
		StartCoroutine("WaitForMusic");
	}

	public IEnumerator WaitForMusic()
	{
		yield return new WaitForSeconds(1.0f);
		//au.clip = WavMusicConvert.musicClips[0];
		au.Play();
		au.loop = true;
	}

	// Update is called once per frame
	void Update()
	{
		if (Play)
		{
			Play = false;
			au.Play();
		}
	}
}

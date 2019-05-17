using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLock : MonoBehaviour
{
	// Start is called before the first frame update
	public delegate void GameEvent(CameraController cc);


	public event GameEvent GameFinished = delegate { };

	// Fires Look event
	public void GFinished(CameraController cc)
	{
		//Debug.Log("Looking at " + gameObject.name);
		GameFinished(cc);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLock : MonoBehaviour
{
	// Start is called before the first frame update
	public delegate void GameEvent(CameraController cc);
    public delegate void GameEventBool(CameraController cc, bool state);

	public event GameEvent GameFinished = delegate { };
    public event GameEvent GameStateToggle = delegate { };
    public event GameEventBool GameStateSet = delegate { };
    // Fires Look event
    public void GFinished(CameraController cc)
	{
		//Debug.Log("Looking at " + gameObject.name);
		GameFinished(cc);
	}

    public void GToggleState(CameraController cc)
    {
        GameStateToggle(cc);
    }

    public void GSetState(CameraController cc, bool k)
    {
        GameStateSet(cc, k);
    }
}

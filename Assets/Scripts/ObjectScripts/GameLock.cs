using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLock : MonoBehaviour
{
	// Start is called before the first frame update
	public delegate void GameEvent(CameraController cc, int eventID);
    public delegate void GameEventBool(CameraController cc, bool state, int eventID);

	public event GameEvent GameFinished = delegate { };
    public event GameEvent GameStateToggle = delegate { };
    public event GameEventBool GameStateSet = delegate { };

    public static int CUUID = 0;
    // Fires Look event
    public void GFinished(CameraController cc)
	{
		//Debug.Log("Looking at " + gameObject.name);
		GameFinished(cc, CUUID++);
	}
    public void Decr(CameraController c)
    {
        CUUID--;
    }

    public void GToggleState(CameraController cc)
    {
        GameStateToggle(cc,CUUID++);
    }

    public void GSetState(CameraController cc, bool k)
    {
        GameStateSet(cc, k, CUUID++);
    }
}

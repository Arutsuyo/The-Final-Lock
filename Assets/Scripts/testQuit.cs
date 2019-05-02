using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testQuit : MonoBehaviour
{
    public Material standard;
    public Material looking;

    private void Start()
    {
        Subscribe();
    }

    public void Subscribe()
    {
        Interactable ia = gameObject.GetComponent<Interactable>();
        if (ia)
        {
            ia.lookEvent += () => { LookedAt(); };
            ia.interactEvent += () => { Interacted(); };
        }
    }

    private void Update()
    {
        gameObject.GetComponent<Renderer>().material = standard;
    }

    private void LookedAt()
    {
        Debug.Log("Look event triggered!");
        gameObject.GetComponent<Renderer>().material = looking;
    }

    private void Interacted()
    {
        //Handle event.....
        Debug.Log("Interact event triggered!");

#if UNITY_WEBPLAYER
    Application.OpenURL(webplayerQuitURL);  
#elif UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif

    }
}

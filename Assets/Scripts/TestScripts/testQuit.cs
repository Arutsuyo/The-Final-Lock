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
        // Get the Interactable script reference
        Interactable ia = gameObject.GetComponent<Interactable>();

        // Make sure it's not null
        if (ia)
        {
            // Subscribe to the event
            ia.lookEvent += LookedAt;
            ia.interactEvent += Interacted;
        }
    }

    public void Unsubscribe()
    {
        // Get the Interactable script reference
        Interactable ia = gameObject.GetComponent<Interactable>();

        // Make sure it's not null
        if (ia)
        {
            // Unsubscribe to the event
            ia.lookEvent -= LookedAt;
            ia.interactEvent -= Interacted;
        }
    }

    private void LookedAt(CameraController cc)
    {
        //Handle event.....
        Debug.Log("Look event triggered!");
        // Can call whatever function you want
        SwapMaterial();
    }

    private bool Interacted(CameraController cc)
    {
        //Handle event.....
        Debug.Log("Interact event triggered!");
        // Can call whatever function you want
        QuitEditor();
        return false;
    }
    private void Update()
    {
        gameObject.GetComponent<Renderer>().material = standard;
    }

    private void QuitEditor()
    {

#if UNITY_WEBPLAYER
        Application.OpenURL(webplayerQuitURL);  
#elif UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }

    private void SwapMaterial()
    {
        gameObject.GetComponent<Renderer>().material = looking;
    }
}

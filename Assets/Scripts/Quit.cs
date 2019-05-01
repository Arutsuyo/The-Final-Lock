using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quit : MonoBehaviour
{
    public static string webplayerQuitURL = "http://google.com";
    public void QuitMe()
    {
#if UNITY_WEBPLAYER
    Application.OpenURL(webplayerQuitURL);  
#elif UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif

    }
}

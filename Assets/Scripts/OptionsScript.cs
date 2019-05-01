using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OptionsScript : MonoBehaviour
{
    public Text fullScreen;
    public Color fullScreenColor;
    public Color windowScreenColor;
    public void ToggleFullScreen()
    {
        Screen.fullScreenMode = (Screen.fullScreenMode == FullScreenMode.Windowed ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed);
        fullScreen.text = (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ? "Window Mode" : "Fullscreen Mode");
        fullScreen.color = (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ? fullScreenColor : windowScreenColor);
    }
}

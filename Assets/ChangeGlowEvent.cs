using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeGlowEvent : MonoBehaviour
{
    public GameLock listening;
    public GlowObject[] glowing;
    public Color[] toSetTo;
    // Start is called before the first frame update
    void Start()
    {
        listening.GameFinished += Listening_GameFinished;
    }

    private void Listening_GameFinished(CameraController cc, int _)
    {
        // Changes all the colors in glowing to toSetTo
        for(int i = 0; i < glowing.Length; i++)
        {
            glowing[i].GlowColor = toSetTo[i];
            glowing[i].EnableGlow();
        }
    }
}

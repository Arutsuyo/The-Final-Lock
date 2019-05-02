using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyMenuSound : MonoBehaviour
{
    FMOD.Studio.ParameterInstance val;
    FMOD.Studio.EventInstance EI;
    // Start is called before the first frame update
    void Awake()
    {
        EI = FMODUnity.RuntimeManager.CreateInstance("event:/MainMenuMusic");
        EI.getParameter("Speed", out val);

    }
    void Start()
    {
        EI.start();
        val.setValue(0);

    }

    public float getVal()
    {
        float f;
        val.getValue(out f);
        return f;
    }
    public void setVal(float f) {
        val.setValue(Mathf.Clamp(f, 0, 100f));
    }
}

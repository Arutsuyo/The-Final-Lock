using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DestroyEverything : MonoBehaviour
{
    public void DoubleThanosSnap(int nextScene)
    {
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject gg in go)
        {
            if (gg.Equals(this.gameObject))
            {
                continue;
            }
            else
            {
                Destroy(gg);
            }
        }
        SceneManager.LoadScene(nextScene);
    }
}

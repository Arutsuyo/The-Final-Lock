using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TransferScenes : MonoBehaviour
{
    public Animator Anim;
    private bool HasJumped = false;

    public int SceneIndex;
    public LoadSceneMode LSM;
    public GameObject toHold;
    public bool DestroyOnLoad = true;
    public void Update()
    {
        // Gotta check it here :|
        if(Anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !Anim.IsInTransition(0) && Anim.GetCurrentAnimatorStateInfo(0).IsName("CreationToDoor") && !HasJumped)
        {
            HasJumped = true;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(toHold);
            SceneManager.LoadSceneAsync(SceneIndex, LSM);
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.Log("Loaded new scene!");
        if (DestroyOnLoad)
        {
            Destroy(toHold);
            Destroy(gameObject);
        }
    }
}

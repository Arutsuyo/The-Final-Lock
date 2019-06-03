using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnItem : MonoBehaviour
{

    public Vector3 direction;
    // Instead of having all of the individual parts move, instead move an object...
    public ItemsCanSpawn[] cool;
    public GameObject parentGO;
    [HideInInspector] public GameObject[] moving;
    public GameObject empty;
    public void Start()
    {
        moving = new GameObject[cool.Length];
        for (int i = 0; i < cool.Length; i++)
        {
            GameObject k = Instantiate(empty, parentGO.transform);
            k.transform.position = new Vector3();
            moving[i] = k;
            StartCoroutine(moveGO(k, i, cool[i].speed, direction));
        }
    }
    void PreGenerate(int i)
    {
        // :P
    }
    IEnumerator moveGO(GameObject toMove, int i, float speed, Vector3 dir)
    {
        PreGenerate(i);
        bool ch = false;
        while (true)
        {
            if (Time.time - cool[i].lastSpawnedTime > .05f)
            {
                if (Mathf.Max(0, Mathf.Atan((Time.time - cool[i].lastSpawnedTime) / (5f / cool[i].prob)) / (Mathf.PI / 2f)) >= Random.Range(0, 1f))
                {
                    // Spawn an item somewhere :D
                    int SPL = Random.Range(0, cool[i].spawnLocations.Length);
                    GameObject rso = Instantiate(cool[i].toSpawn[Random.Range(0, cool[i].toSpawn.Length)], cool[i].spawnLocations[SPL].transform.position, cool[i].spawnLocations[SPL].transform.rotation);
                    rso.transform.SetParent(toMove.transform, true);
                    Destroy(rso, cool[i].time);
                    cool[i].lastSpawnedTime = Time.time;
                }
            }
            toMove.transform.Translate(dir * Time.deltaTime * speed);
            if (toMove.transform.position.sqrMagnitude > 10000000 && !ch)
            {
                // D:
                HashSet<Transform> tt = new HashSet<Transform>();
                foreach (Transform t in toMove.transform)
                {
                    t.SetParent(null, true);
                    tt.Add(t);
                }
                toMove.transform.position = -toMove.transform.position;
                foreach (Transform t in tt)
                {
                    t.SetParent(toMove.transform, true);
                }
                ch = true;
            }
            else if (ch)
            {
                ch = false;
            }
            yield return null;
        }
    }

}
[System.Serializable]
public class ItemsCanSpawn
{
    [HideInInspector] public float lastSpawnedTime = 0;
    public GameObject[] spawnLocations;
    public GameObject[] toSpawn;
    public float speed;
    public float time;
    public float prob;
}

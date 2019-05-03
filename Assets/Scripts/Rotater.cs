using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotater : MonoBehaviour
{
    public float speed = 0.01f;

    private float delta = 0;
    public void Update()
    {
        gameObject.transform.localRotation *= Quaternion.Euler(0, delta, 0);
        delta = speed * Time.deltaTime;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Oscillator : MonoBehaviour
{
    [SerializeField] Vector3 movement = new Vector3(0f, -5f, 0f);
    [SerializeField] float period = 2f;

    Vector3 position;

    void Start()
    {
        position = transform.position;
    }

    void Update()
    {
        if (period <= Mathf.Epsilon) { return; }

        Vector3 offset = movement * (Mathf.Sin(Time.time / period * Mathf.PI * 2) / 2f + 0.5f);
        transform.position = position + offset;
    }
}
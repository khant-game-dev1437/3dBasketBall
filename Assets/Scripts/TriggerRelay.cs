using UnityEngine;
using System;

public class TriggerRelay : MonoBehaviour
{
    public Action<Collider> onEnter;

    void OnTriggerEnter(Collider other)
    {
        onEnter?.Invoke(other);
    }
}

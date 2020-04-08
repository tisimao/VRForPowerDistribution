using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerReport : MonoBehaviour
{

    public Dictionary<Collider, int> Colliders { get; private set; }

    private void Start()
    {
        Colliders = new Dictionary<Collider, int>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Colliders.ContainsKey(other))
        {
            Colliders[other]++;
        }
        else
        {
            Colliders.Add(other, 1);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Colliders.ContainsKey(other))
        {
            Colliders[other]--;
            if (Colliders[other] <= 0)
            {
                Colliders.Remove(other);
            }
        }

    }
}

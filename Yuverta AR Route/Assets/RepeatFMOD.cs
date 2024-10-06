using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class RepeatFMOD : MonoBehaviour
{
    // Reference to the FMOD Event Enmiter
    [SerializeField] private StudioEventEmitter _eventEmitter;

    public void Repeat()
    {
        _eventEmitter.Play();
    }
}

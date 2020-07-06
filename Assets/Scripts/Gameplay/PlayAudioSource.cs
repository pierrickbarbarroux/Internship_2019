using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Joue un son quand la fonction PlayAudio() est appelée (sûrement dans des events system)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayAudioSource : MonoBehaviour
{

    public void PlayAudio()
    {
        this.GetComponent<AudioSource>().Play(0);
    }
}

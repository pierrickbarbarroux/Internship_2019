using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ce script doit être placé sur l'écran de transition pour jouer la transition au début de la scène
/// </summary>
public class PLayAnimationAtBegin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animator>().Play("Transition_At_Begin");
    }
}

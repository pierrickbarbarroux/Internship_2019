using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe permet d'activer la VR. 
/// Ce script doit être placé sur n'importe quel objet de la scène 'Stage_IGN'
/// </summary>
public class EnableVR : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        UnityEngine.XR.XRSettings.enabled = true;
    }

}

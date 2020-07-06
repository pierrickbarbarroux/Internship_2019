using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Cette classe permet de désactiver la VR au démarrage d'une scène.
/// Il suffit de placer le script sur n'importe quel objet de la scène.
/// </summary>
public class DisableVR : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.XR.XRSettings.enabled = false;
    }
}

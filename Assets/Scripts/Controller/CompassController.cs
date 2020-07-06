using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère l'orientation de la flèche sontenue dans la boussole (celle dont on dispose quand on se déplace en VR)
/// </summary>
public class CompassController : MonoBehaviour
{
    [Tooltip("La flèche invisible qui permet d'obtenir l'orientation désirée")]
    public Transform other_arrow;

    [Tooltip("La boussole")]
    public Transform compass;

    Vector3 orientation;

    void Update()
    {
        orientation.y = other_arrow.eulerAngles.y - compass.eulerAngles.y;
        transform.localEulerAngles = orientation;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe permettant d'obtenir la direction du nord dans unity
/// </summary>
public class GetNorthDirection : MonoBehaviour
{

    Vector3 north;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, Input.compass.trueHeading-180, 0);
    }
}

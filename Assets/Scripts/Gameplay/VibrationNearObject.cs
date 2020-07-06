using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// Cette classe fait vibrer les controlleur VR lorsque l'objet mygo se rapproche de this 
/// </summary>
public class VibrationNearObject : MonoBehaviour
{
    public GameObject mygo;

    // Update is called once per frame
    void Update()
    {
        //racine_carre((x_point - x_centre)² + (y_centre - y_point)) < rayon
        if (Mathf.Sqrt((mygo.transform.position.x - this.transform.position.x)* (mygo.transform.position.x - this.transform.position.x) + (mygo.transform.position.z - this.transform.position.z) * (mygo.transform.position.z - this.transform.position.z))<3)
        {
            SteamVR_Actions._default.Haptic.Execute(0f, 1f, 20f, 20f, SteamVR_Input_Sources.Any);
        }
    }
}

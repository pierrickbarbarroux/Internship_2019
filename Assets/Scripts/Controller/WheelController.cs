using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère la rotation des roues des voitures. Mais ne sert à rien d'autres et les voitures ne sont pas 
/// implémenter dans la version finale
/// </summary>
public class WheelController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        transform.Rotate(new Vector3(250, 0, 0) * Time.deltaTime);
    }
}

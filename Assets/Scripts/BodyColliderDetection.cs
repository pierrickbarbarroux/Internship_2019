using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe de test pour résoudre un bug avec les colliders et la VR
/// Sans intérêt et non utilisée par la suite.
/// </summary>
public class BodyColliderDetection : MonoBehaviour
{

    Vector3 old_position;


    void Start()
    {
        Debug.Log("bbbbbbbbbbbbbbbbbbb" + transform.parent.parent.gameObject.name);

    }
    void Update()
    {
        if ((int)Time.time % 5 == 0)
        {
            old_position = transform.parent.parent.position;
        }
    }

    //void OnCollisionEnter(Collision other)
    //{
    //    Debug.Log(other.gameObject.name);
    //    transform.parent.parent.position = old_position;

    //    //if (other.gameObject.tag == "Terrain_tag")
    //    //{
    //    //    Debug.Log("collision avec un terrain");
    //    //    transform.parent.parent.position = old_position;
    //    //}

    //}

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        transform.parent.parent.position = old_position;
    }
}

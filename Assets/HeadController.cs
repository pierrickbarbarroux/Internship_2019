using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Classe de test. Plus utilisée par la suite. 
/// Elle servait à détecter les collisions entre la tête du joueur et les murs. Le cas échéant, le joueur devait être téléporté
/// à la position qu'il occupait quelques secondes auparavant. 
/// </summary>
public class HeadController : MonoBehaviour
{
    Vector3 old_position;


    void Start()
    {
        Debug.Log("bbbbbbbbbbbbbbbbbbb" + transform.parent.parent.parent.parent.gameObject.name);

    }
    void Update()
    {
        Debug.Log(old_position);

        if ((int)Time.time%2==0)
        {
            Debug.Log(old_position);
            old_position = transform.parent.parent.parent.parent.position;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        transform.parent.parent.parent.parent.position = old_position;

        //if (other.gameObject.tag == "Terrain_tag")
        //{
        //    Debug.Log("collision avec un terrain");
        //    transform.parent.parent.parent.parent.position = old_position;
        //}
        
    }
}

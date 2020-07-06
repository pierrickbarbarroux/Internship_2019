using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Classe gérant la rotation des rotor du drone
/// </summary>
public class MyDroneController : MonoBehaviour
{
    [Tooltip("Transform que suit le drone. Il s'agit de 'FollowHead' (objet VR situé au niveau du Player)")]
    public Transform target_position;
    Vector3 vec;

    //Les quatres rotors
    public GameObject rotor1;
    public GameObject rotor2;
    public GameObject rotor3;
    public GameObject rotor4;

    [Tooltip("Vitesse de rotation des rotor")]
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        //vec permet de légèrement décaler la position du drone vers le haut pour ne pas avoir 
        //le drone en plein milieu des yeux
        vec = Vector3.up * 0.2f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target_position.position +vec;
    }

    //On applique une rotation sur chaque rotor
    void Update()
    {
        rotor1.transform.Rotate(Vector3.forward * Time.deltaTime * speed);
        rotor2.transform.Rotate(Vector3.forward * Time.deltaTime * speed);
        rotor3.transform.Rotate(Vector3.forward * Time.deltaTime * speed);
        rotor4.transform.Rotate(Vector3.forward * Time.deltaTime * speed);
    }
}

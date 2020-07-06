using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe gérant la clé : savoir s'il s'agit de la bonne clé (celle qui donne acces à l'ordinateur)
/// ou de la mauvasie
/// </summary>
public class KeyController2 : MonoBehaviour
{

    public bool is_good_key;

    public void PutOffGravity(Rigidbody myrigidbody)
    {
        myrigidbody.useGravity = false;
    }
    public void PutOnGravity(Rigidbody myrigidbody)
    {
        myrigidbody.useGravity = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCollision : MonoBehaviour
{

    void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.gameObject.name);
    }
}
